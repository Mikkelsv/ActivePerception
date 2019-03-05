from mlagents.envs import UnityEnvironment

import numpy as np
import random
import time

from memory import Memory


class Trainer:

    def __init__(self, model, env):
        self.model = model
        self.env = env
        self.exploration = 0.8
        self.exploration_decay = 0.9
        self.gamma = 0.9  # Future reward discount

        # Memory specifics
        self.buffer_size = 5000
        self.batch_training_size = 256

        # Setup environment dependant variables
        self.default_brain = self.env.brain_names[0]
        env_info = self.env.reset(train_mode=False)[self.default_brain]
        self.num_observations = len(env_info.vector_observations[0])
        self.num_actions = len(env_info.action_masks[0])

        # Init Memory
        self.memory = Memory(self.num_observations, self.num_actions, self.buffer_size)

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

        # Timekeeping variables
        self.duration_total = 0
        self.duration_training = [0, 0]
        self.duration_environment = [0, 0]
        self.duration_selecting_action = [0, 0]

    def generate_test_cases(self, num_test_cases):
        observations = np.zeros((num_test_cases, self.num_observations))
        for i in range(num_test_cases):
            env_info = self.env.reset(train_mode=False)[self.default_brain]
            observation = env_info.vector_observations
            observations[i, :] = observation[:]
        self.test_cases = observations

    def train(self, num_generations=1, num_batches=1, batch_size=1, test_size=10):
        """
            Trains the trainer using environment and model
        :param num_generations: Number of generations
        :param num_batches: Number of training batches in each generation
        :param batch_size: Number of episodes in each batch
        :param test_size: Number of episodes in each generation test
        """
        self.num_tests = test_size
        self.num_generations = num_generations
        self.num_batches = num_batches
        self.batch_size = batch_size

        num_episodes = num_generations * num_batches * batch_size
        self.generation_size = batch_size * num_batches

        steps = 0
        episode = 0
        generation = 1

        print("Commencing Training - {} generations, {} batches, {} episodes, {} tests"
              .format(num_generations, num_batches, batch_size, test_size))
        print("\t - In total {} training episodes".format(num_episodes))
        print("Memory - {} buffer size, {} batch training size".format(self.buffer_size, self.batch_training_size))
        self.duration_total = time.time()
        self.generation_time_stamp = time.time()
        while episode < num_episodes:
            episode += 1
            steps += self.run_episode()[0]

            if episode % self.batch_size == 0:
                # Train model on batch
                now = time.time()
                o, a = self.memory.get_random_batch(self.batch_training_size)
                loss = self.model.train_on_batch(o, a)
                self.update_time_keeper(self.duration_training, time.time() - now)

            if episode % self.generation_size == 0:
                self.evaluate_generation(generation, steps)
                # Update runtime variables
                generation += 1
                steps = 0
                self.exploration *= self.exploration_decay
        self.duration_total = time.time() - self.duration_total

    def run_episode(self, store=True, stochastic=True):
        """
            Resets and runs a single episode in the trainer environment
        :param store: Store episode in trainer memory
        :param stochastic: Use stochastic action generation based on exploration rate
        :return: Steps in episode, Mean Reward of episode
        """
        observations = np.empty(0).reshape(0, self.num_observations)
        actions = np.empty(0).reshape(0, self.num_actions)
        rewards = np.empty(0).reshape(0, 1)
        steps = 0
        env_info = self.env.reset(train_mode=False)[self.default_brain]
        while True:
            steps += 1
            observation = env_info.vector_observations

            action, action_indexed = self._get_action(observation, stochastic)

            # Fetch next environment and reward, track the time
            now = time.time()
            env_info = self.env.step(action_indexed)[self.default_brain]
            self.update_time_keeper(self.duration_environment, time.time() - now)

            reward = env_info.rewards[0]

            # Update memory
            observations = np.vstack([observation, observations])
            actions = np.vstack([action, actions])
            rewards = np.vstack([reward, rewards])

            # If end of episode
            if env_info.local_done[0]:
                # Reset Environment
                r = self.env.reset(train_mode=False)[self.default_brain]

                # Update Memory
                action_rewards = self._get_discounted_action_rewards(actions, rewards)
                if store:
                    self.memory.add(observations, action_rewards)
                return steps, np.mean(action_rewards)

    def evaluate_generation(self, generation, steps):
        rewards = np.zeros(self.num_tests)
        for i in range(self.num_tests):
            s, t = self.run_episode(False, False)
            rewards[i] = t

        avg_reward = np.mean(rewards)
        dur = self._get_generation_duration()
        avg_steps = steps / self.generation_size
        avg_step_duration = dur / steps
        print("Gen {}\t Avg Reward: {:.2f}, Duration {:.1f}s, SpE {:.1f}, DpS: {:.04f}s"
              .format(generation, avg_reward, dur, avg_steps, avg_step_duration))

    def get_model(self):
        return self.model

    def close_environment(self):
        self.env.close()

    def _get_action(self, observation, stochastic):
        actions = np.zeros(self.num_actions)
        if stochastic and random.random() < self.exploration:
            action = np.random.randint(0, self.num_actions)
        else:
            now = time.time()
            action_values = self.model.predict(observation)[0]
            action = np.argmax(action_values)
            self.update_time_keeper(self.duration_selecting_action, time.time() - now)
        actions[action] = 1
        return actions, action

    def _get_discounted_action_rewards(self, actions, rewards):
        prior = 0
        discounted_rewards = []
        if self.gamma < 0:
            for r in rewards:
                new_r = r + prior * self.gamma
                discounted_rewards.append(new_r)
                prior = new_r
        else:
            discounted_rewards = rewards

        action_rewards = np.multiply(actions, discounted_rewards)
        return action_rewards

    def _get_generation_duration(self):
        generation_end_time = time.time()
        dur = generation_end_time - self.generation_time_stamp
        self.generation_time_stamp = generation_end_time
        return dur

    def print_summary(self):
        avg_duration_env = self.get_time_keeper_average(self.duration_environment)
        avg_duration_training = self.get_time_keeper_average(self.duration_training)
        avg_duration_action = self.get_time_keeper_average(self.duration_selecting_action)
        print("\n----------Training Summary-------------")
        print("Time Durations")
        print("Total Training Time: {:.1f}s".format(self.duration_total))
        print("Interacting with Environment: {:.2f} - Average {:.4f}s"
              .format(self.duration_environment[0], avg_duration_env))
        print("Training Model: {:.2f} - Average {:.4f}s"
              .format(self.duration_training[0], avg_duration_training))
        print("Selecting Action: {:.2f} - Average{:.4f}s"
              .format(self.duration_selecting_action[0], avg_duration_action))
        print("-----------------------------------------")

    @staticmethod
    def update_time_keeper(keeper, duration):
        keeper[0] += duration
        keeper[1] += 1

    @staticmethod
    def get_time_keeper_average(keeper):
        return keeper[0] / keeper[1]


def main():
    import sys
    import deprecated_slam_model

    print("Python version: {}".format(sys.version))
    # check Python version
    if sys.version_info[0] < 3:
        raise Exception("ERROR: ML-Agents Toolkit (v0.3 onwards) requires Python 3")

    env_name = "Env/SlamBall"
    env = UnityEnvironment(file_name=None, worker_id=0, seed=1)  # Add seed=n for consistent results
    train_mode = False  # Whether to run the environment in training or inference mode

    # Set the default brain to work with
    default_brain = env.brain_names[0]

    # Investigate environment
    env_info = env.reset(train_mode=train_mode)[default_brain]
    num_inputs = len(env_info.vector_observations[0])
    num_outputs = len(env_info.action_masks[0])

    # Fetching model
    rebuild = True
    model_name = "slam_model_trained"
    if rebuild:
        model = deprecated_slam_model.generate_model(num_inputs, num_outputs)
    else:
        model = deprecated_slam_model.load_model(model_name)

    # Train
    trainer = Trainer(model, env)
    trainer.train(10, 10, 10, 10)
    trainer.print_summary()

    # Close environment
    env.close()

    # Save model
    deprecated_slam_model.save_model(model, model_name)


if __name__ == "__main__":
    main()
