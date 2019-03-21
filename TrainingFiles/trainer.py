from mlagents.envs import UnityEnvironment
import numpy as np

from memory import Memory
from synopsis_manager import SynopsisManager

import random
import time


class Trainer:

    def __init__(self, model, env):
        self.model = model
        self.env = env
        self.exploration = 0.8
        self.exploration_decay = 0.9
        self.gamma = 0.0  # Future reward discount

        # Memory specifics
        self.buffer_size = 1000
        self.batch_training_size = 64

        # Setup environment dependant variables
        self.default_brain = self.env.brain_names[0]
        env_info = self.env.reset(train_mode=False)[self.default_brain]

        # self.observation_shape = len(env_info.vector_observations[0])
        self.observation_shape = (32, 32, 32, 1)
        self.views_shape = (121, 1)
        self.num_actions = len(env_info.action_masks[0])

        # Init Memory
        input_shape = (self.observation_shape, self.views_shape)
        self.memory = Memory(input_shape, self.num_actions, self.buffer_size)

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
        while episode < self.num_episodes:
            episode += 1
            self.run_episode()

            if episode % self.batch_size == 0:
                # Train model on batch
                now = time.time()
                o, v, a = self.memory.get_random_batch(self.batch_training_size)
                loss = self.model.train_on_batch([o, v], a)
                self.update_time_keeper(self.duration_training, time.time() - now)
                self.loss.append(loss)

            if episode % self.generation_size == 0:
                self.evaluate_generation(generation)
                # Update runtime variables
                generation += 1
                self.exploration *= self.exploration_decay
        self.duration_total = time.time() - self.duration_total

    def run_episode(self, train=True):
        """
            Resets and runs a single episode in the trainer environment
        :param train: Wether to train or test
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
        env_info = self.env.reset(train_mode=False)[self.default_brain]
        while True:
            steps += 1

            observation = env_info.vector_observations
            observation, view, distance, accuracy = self.reshape_input(observation)
            distances.append(distance)
            accuracies.append(accuracy)

            action_indexed, prediction = self.get_action([observation, view], train)

            # Fetch next environment and reward, track the time
            now = time.time()
            env_info = self.env.step(action_indexed)[self.default_brain]
            self.update_time_keeper(self.duration_environment, time.time() - now)

            reward = env_info.rewards[0]

            # Update memory
            observations = np.vstack([observation, observations])
            views = np.vstack([views, view])
            predictions = np.vstack([predictions, prediction])
            action_indexes.append(action_indexed)
            rewards.append(reward)

            # If end of episode
            if env_info.local_done[0]:
                # Update Memory
                action_rewards, discounted_rewards = self.get_discounted_action_rewards(predictions,
                                                                                        action_indexes, rewards)
                if train:
                    self.memory.add(observations, views, action_rewards)
                return steps, distances, accuracies, discounted_rewards

    def evaluate_model(self, num_runs):
        rewards = []
        sum_distances = np.zeros(num_runs)
        episode_steps = np.zeros(num_runs)
        max_accuracies = np.zeros(num_runs)
        for i in range(num_runs):
            steps, distances, accuracies, discounted_rewards = self.run_episode(train=False)
            rewards.append(discounted_rewards)
            sum_distances[i] = np.sum(distances)
            episode_steps[i] = steps
            max_accuracies[i] = np.max(accuracies)
        avg_reward = np.mean(rewards)
        avg_steps = np.mean(episode_steps)
        avg_distance = np.mean(sum_distances)
        avg_accuracy = np.mean(max_accuracies)
        return avg_reward, avg_steps, avg_distance, avg_accuracy

    def evaluate_generation(self, generation_number):
        if self.num_tests:
            avg_reward, avg_steps, avg_distance, avg_accuracy = self.evaluate_model(self.num_tests)

            dur = self._get_generation_duration()
            self.generation_reward.append(np.array([avg_reward, avg_steps, avg_distance, avg_accuracy]))
            self.sm.write(("Gen {}\t Avg Reward: {:.5}, Duration {:.1f}s, SpE {:.1f}, " +
                          "AvgDistance: {:.1f}, AvgAccuracy: {:.3f}")
                          .format(generation_number, avg_reward, dur, avg_steps, avg_distance, avg_accuracy))

    def evaluate_solution(self, num_runs):
        if num_runs:
            avg_reward, avg_steps, avg_distance, avg_acc = self.evaluate_model(num_runs)
            self.sm.print_evaluation(num_runs, avg_reward, avg_steps, avg_distance, avg_acc)

    def get_model(self):
        return self.model

    def close_environment(self):
        self.env.close()

    def get_action(self, observation, stochastic):
        predictions = self.model.predict(observation)[0]
        if stochastic and random.random() < self.exploration:
            action_index = np.random.randint(0, self.num_actions)
        else:
            now = time.time()
            action_index = np.argmax(predictions)
            self.update_time_keeper(self.duration_selecting_action, time.time() - now)

        return action_index, predictions

    def get_discounted_action_rewards(self, predictions, action_indexes, rewards):
        # Compute true action prediction derived from reward
        action_rewards = predictions
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
        discounted_activated_rewards = np.tanh(discounted_rewards)

        # Update chosen action prediction to match reward
        for i in range(len(predictions)):
            action_rewards[i, action_indexes[i]] = discounted_activated_rewards[i]  # Set true output to be discounted reward
        return action_rewards, discounted_activated_rewards

    def _get_generation_duration(self):
        generation_end_time = time.time()
        dur = generation_end_time - self.generation_time_stamp
        self.generation_time_stamp = generation_end_time
        return dur

    @staticmethod
    def update_time_keeper(keeper, duration):
        keeper[0] += duration
        keeper[1] += 1

    def reshape_input(self, observations):
        grid = observations[0, :-3].reshape((-1, 32, 32, 32, 1))
        views = np.zeros(self.num_actions)
        views[int(observations[0, -3])] = 1
        views = views.reshape((-1, self.num_actions, 1))
        distance = observations[0, -2]
        accuracy = observations[0, -1]
        return grid, views, distance, accuracy

    def sigmoid(self, x):
        return 1 / (1 + np.exp(-x))