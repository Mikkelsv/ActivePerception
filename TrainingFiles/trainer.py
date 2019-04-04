from mlagents.envs import UnityEnvironment
import numpy as np

from memory import Memory
from synopsis_manager import SynopsisManager

import random
import time


class Trainer:

    def __init__(self, model, env, max_step):
        self.model = model
        self.env = env
        self.exploration = 0.9
        self.exploration_decay = 0.999
        self.exploration_minimum = 0.5
        self.gamma = 0.0  # Future reward discount
        self.top_n_actions = 3

        # Reward variables
        self.alpha_accuracy = 1.0
        self.alpha_distance = 0.0
        self.alpha_steps = 0.0
        self.normalize_reward_alphas()
        self.alpha_views = -0.3

        # Setup environment dependant variables
        self.default_brain = self.env.brain_names[0]
        env_info = self.env.reset(train_mode=False)[self.default_brain]
        self.max_step = max_step
        self.num_actions = len(env_info.action_masks[0])
        self.num_views = self.num_actions * 2  # Both current and visited views
        self.g = 32
        self.gCubed = self.g ** 3
        self.observation_shape = (32, 32, 32, 1)
        self.views_shape = (self.num_views, 1)

        # Memory specifics
        self.replay = False
        self.buffer_size = 100
        self.batch_training_size = 32
        buffer_filling_rate = 0.2
        self.fill_memory_runs = int(self.buffer_size / 10 * buffer_filling_rate)

        # Init Memory
        input_shape = (self.observation_shape, self.views_shape)
        # self.memory = Memory(input_shape, self.num_actions, self.buffer_size, replay=self.replay)

        # Runtime variables
        self.step_count = 0
        self.batch_count = 0
        self.generation_count = 0
        self.generation_time_stamp = time.time()
        self.num_generations = 0
        self.num_batches = 0
        self.batch_size = 0
        self.test_cases = np.empty(0)
        self.num_tests = 0
        self.generation_size = 0
        self.num_episodes = 0

        # Timekeeping variables
        self.duration_total = 0
        self.duration_training = [0, 0]
        self.duration_environment = [0, 0]
        self.duration_selecting_action = [0, 0]

        self.sm: SynopsisManager = None
        self.generation_reward = []
        self.loss = []

    def set_synopsis_manager(self, sm: SynopsisManager):
        self.sm = sm

    def train(self, num_generations=1, num_batches=1, batch_size=1, num_tests=10):
        """
            Trains the trainer using environment and model
        :param num_generations: Number of generations
        :param num_batches: Number of training batches in each generation
        :param batch_size: Number of episodes in each batch
        :param num_tests: Number of episodes in each generation test
        """
        self.num_tests = num_tests
        self.num_generations = num_generations
        self.num_batches = num_batches
        self.batch_size = batch_size

        self.num_episodes = num_generations * num_batches * batch_size
        self.generation_size = batch_size * num_batches

        self.sm.print_training_config()
        self.loss = []

        episode = 0
        generation = 1

        self.duration_total = time.time()
        self.generation_time_stamp = time.time()

        # if self.replay:
        #   self.fill_memory()

        while episode < self.num_episodes:
            episode += 1
            self.run_episode()

            # if episode % self.batch_size == 0:
            # Train model on batch
            #   now = time.time()
            #  o, v, a = self.memory.get_random_batch(self.batch_training_size)
            # loss = self.model.train_on_batch([o, v], a)
            # self.update_time_keeper(self.duration_training, time.time() - now)
            # self.loss.append(loss)

            if episode % self.generation_size == 0:
                self.evaluate_generation(generation)
                # Update runtime variables
                generation += 1
                self.exploration = max(self.exploration_decay * self.exploration_decay, self.exploration_minimum)
        self.duration_total = time.time() - self.duration_total

    def run_episode(self, train=True, stochastic=False):
        """
            Resets and runs a single episode in the trainer environment
        :param stochastic: Determine action randomly
        :param train: Whether to train or test
        :return: Steps in episode, Distances, Accuracies and Mean Reward of episode
        """
        observations = np.empty(0).reshape((0,) + self.observation_shape)
        views = np.empty(0).reshape((0,) + self.views_shape)
        predictions = np.empty(0).reshape(0, self.num_actions)
        action_indexes = []
        rewards = []
        steps = 0
        distances = []
        accuracies = []
        # env_info = self.env.reset(train_mode=False)[self.default_brain]
        env_info = self.env.step(0)[self.default_brain]
        while True:
            steps += 1

            observation = env_info.vector_observations
            observation, view, distance, accuracy = self.reshape_input(observation)
            distances.append(distance)
            accuracies.append(accuracy)

            action_indexed, prediction = self.get_action([observation, view], train, stochastic)

            # Fetch next environment and reward, track the time
            now = time.time()
            env_info = self.env.step(action_indexed)[self.default_brain]
            self.update_time_keeper(self.duration_environment, time.time() - now)

            reward = env_info.vector_observations[0, -3:]

            # Update memory
            observations = np.vstack([observations, observation])
            views = np.vstack([views, view])
            predictions = np.vstack([predictions, prediction])
            action_indexes.append(action_indexed)
            rewards.append(reward)

            # If end of episode
            if env_info.local_done[0]:
                # Update Memory
                rewards = self.compute_rewards(rewards)
                predictions_corrected, discounted_rewards = self.get_corrected_predictions(predictions,
                                                                                           action_indexes, rewards)
                if train:
                    # self.memory.add(observations, views, predictions_corrected)
                    now = time.time()
                    loss = self.model.train_on_batch([observations, views], predictions_corrected)
                    self.update_time_keeper(self.duration_training, time.time() - now)
                    self.loss.append(loss)
                return steps, distances, accuracies, discounted_rewards

    def get_action(self, observation, training, stochastic):
        predictions = self.model.predict(observation)[0]
        if (training and random.random() < self.exploration) or stochastic:  # Either training or filling memory
            action_index = np.random.randint(0, self.num_actions)
        else:
            now = time.time()
            top_n = np.asarray(predictions).argsort()[-self.top_n_actions:][::-1]
            action_index = np.random.choice(top_n)
            self.update_time_keeper(self.duration_selecting_action, time.time() - now)
        return action_index, predictions

    def get_corrected_predictions(self, predictions, action_indexes, rewards):
        # Compute true action prediction derived from discounted reward
        prior = 0
        discounted_rewards = np.empty(len(rewards))
        if self.gamma > 0:
            for i in reversed(range(len(rewards))):
                r = rewards[i]
                new_r = r + prior * self.gamma
                discounted_rewards[i] = new_r
                prior = new_r
        else:
            discounted_rewards = rewards
        discounted_activated_rewards = self.sigmoid(np.array(discounted_rewards))
        # discounted_activated_rewards = discounted_rewards

        # Update chosen action prediction to match reward
        for i in range(len(predictions)):
            correct_prediction = discounted_activated_rewards[i]
            predictions[i, action_indexes[i]] = correct_prediction  # Set true output to be discounted reward
        return predictions, discounted_activated_rewards

    def reshape_input(self, observations):
        grid = observations[0, :self.gCubed].reshape((-1, 32, 32, 32, 1))
        views = observations[0, self.gCubed:self.gCubed + self.num_views].reshape((-1, self.num_views, 1))
        # Account for added rewards
        distance = observations[0, -2 - 3]
        accuracy = observations[0, -1 - 3]
        return grid, views, distance, accuracy

    def compute_rewards(self, rewards):
        rewards = np.asarray(rewards)
        accuracy_reward = rewards[:, 0]
        distance_reward = rewards[:, 1]
        view_reward = rewards[:, 2]
        l = len(accuracy_reward)
        step_reward = (self.max_step - l) / self.max_step
        computed_rewards = np.zeros(l)
        for i in range(l):
            if view_reward[i] > 0:
                computed_rewards[i] = self.alpha_views
            else:
                computed_rewards[i] = self.alpha_accuracy * accuracy_reward[i] \
                                      + self.alpha_distance * distance_reward[i] \
                                      + self.alpha_steps * step_reward

        return computed_rewards

    def fill_memory(self):
        for i in range(self.fill_memory_runs):
            print("\rFilling buffer - {}/{} episodes".format(i + 1, self.fill_memory_runs), end="")
            self.run_episode(stochastic=True)
        print("\n")

    def evaluate_model(self, num_runs):
        rewards = []
        sum_distances = np.zeros(num_runs)
        episode_steps = np.zeros(num_runs)
        max_accuracies = np.zeros(num_runs)
        agg_distances = np.zeros((num_runs, self.max_step))
        agg_accuracies = np.zeros((num_runs, self.max_step))
        for i in range(num_runs):
            steps, distances, accuracies, discounted_rewards = self.run_episode(train=False)
            rewards.append(np.mean(discounted_rewards))
            sum_distances[i] = np.sum(distances)
            episode_steps[i] = steps
            max_accuracies[i] = np.max(accuracies)
            agg_distances[i, :len(distances)] = distances[:]
            agg_accuracies[i, :len(accuracies)] = accuracies[:]
        avg_distances = np.mean(agg_distances, axis=0)
        cumsum_distances = np.cumsum(avg_distances)
        avg_accuracies = np.mean(agg_accuracies, axis=0)
        avg_reward = np.mean(rewards)
        avg_steps = np.mean(episode_steps)
        avg_distance = np.mean(sum_distances)
        avg_accuracy = np.mean(max_accuracies)
        return avg_reward, avg_steps, avg_distance, avg_accuracy, cumsum_distances, avg_accuracies

    def evaluate_generation(self, generation_number):
        if self.num_tests:
            avg_reward, avg_steps, avg_distance, max_acc, cumsum_dist, avg_acc = self.evaluate_model(self.num_tests)

            dur = self._get_generation_duration()
            self.generation_reward.append(np.array([avg_reward, avg_steps, avg_distance, max_acc]))
            self.sm.write(("Gen {}\t Avg Reward: {:.4}, Duration {:.1f}s, SpE {:.1f}, " +
                           "AvgDistance: {:.1f}, AvgAccuracy: {:.3f}")
                          .format(generation_number, avg_reward, dur, avg_steps, avg_distance, max_acc))

    def evaluate_solution(self, num_runs):
        if num_runs:
            avg_reward, avg_steps, avg_dist, max_acc, cumsum_dist, avg_acc = self.evaluate_model(num_runs)
            self.sm.print_evaluation(num_runs, avg_reward, avg_steps, avg_dist, max_acc, cumsum_dist, avg_acc)

    def get_model(self):
        return self.model

    def close_environment(self):
        self.env.close()

    def sigmoid(self, x):
        return 1 / (1 + np.exp(-x))

    def _get_generation_duration(self):
        generation_end_time = time.time()
        dur = generation_end_time - self.generation_time_stamp
        self.generation_time_stamp = generation_end_time
        return dur

    @staticmethod
    def update_time_keeper(keeper, duration):
        keeper[0] += duration
        keeper[1] += 1

    def normalize_reward_alphas(self):
        alphas = np.array([self.alpha_accuracy, self.alpha_distance, self.alpha_steps])
        alphas = alphas / np.sum(alphas)
        self.alpha_accuracy = alphas[0]
        self.alpha_distance = alphas[1]
        self.alpha_steps = alphas[2]

    def set_reward_values(self, alpha_accuracy, alpha_distance, alpha_steps, alpha_views, normalize=False):
        self.alpha_accuracy = alpha_accuracy
        self.alpha_distance = alpha_distance
        self.alpha_steps = alpha_steps
        if normalize:
            self.normalize_reward_alphas()
        self.alpha_views = alpha_views
