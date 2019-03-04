from mlagents.envs import UnityEnvironment
import numpy as np
import sys
import deprecated_slam_model
import random
import time

from memory import Memory

"""
    Based on Simple Reinforcement Learning with Keras: Part 2 - Policy-based Agents
    https://github.com/breeko/Simple-Reinforcement-Learning-with-Tensorflow/blob/master/Part%202%20-%20Policy-based%20Agents%20with%20Keras.ipynb
"""


def test_perfect_actions(env, default_brain, train_mode=False, num_generations=50, num_epochs=10):
    for episode in range(num_generations):
        cum_reward = 0
        for epoch in range(num_epochs):
            env_info = env.reset(train_mode=train_mode)[default_brain]
            done = False
            episode_rewards = 0
            # action_size = brain.vector_action_space_size
            while not done:
                observation = env_info.vector_observations
                x = observation[0, 0]
                z = observation[0, 1]
                action = np.array([-x, -z])
                env_info = env.step(action)[default_brain]

                episode_rewards += env_info.rewards[0]
                done = env_info.local_done[0]
            cum_reward += episode_rewards

        print("Generation: {} \t- average reward: {}".format(episode, cum_reward / num_epochs))


def compute_discounted_rewards(rewards, gamma=0.0):
    prior = 0
    out = []
    for r in rewards:
        new_r = r + prior * gamma
        out.append(new_r)
        prior = new_r
    return np.array(out)


def get_action(model, observation, exploration, num_actions=4):
    actions = np.zeros(num_actions)
    if random.random() < exploration:
        action = np.random.randint(0, num_actions)
    else:
        action_values = model.predict(observation)[0]
        action = np.argmax(action_values)
    actions[action] = 1
    return actions


def get_discounted_action_rewards(actions, rewards, gamma=0.9):
    discounted_rewards = compute_discounted_rewards(rewards)
    action_rewards = np.multiply(actions, discounted_rewards)
    return action_rewards



def run_training(model, env, default_brain, num_generations=25, generation_size=10, batch_size=10):
    train_mode = False
    exploration = 0.8
    exploration_reduction_rate = 0.9
    batch_training_size = 1000
    buffer_size = 10000

    num_episodes = num_generations * generation_size * batch_size

    env_info = env.reset(train_mode=train_mode)[default_brain]

    num_observations = len(env_info.vector_observations[0])

    num_actions = len(env_info.action_masks[0])

    observations = np.empty(0).reshape(0, num_observations)
    action_rewards = np.empty(0).reshape(0, num_actions)
    actions = np.empty(0).reshape(0, num_actions)
    rewards = np.empty(0).reshape(0, 1)
    losses = []
    generation_reward = []



    episode = 0
    batch = 0
    generation = 0

    stamp = time.time()
    obs_time = 0
    steps = 0
    memory = Memory(num_observations, num_actions, buffer_size)


    print("Commencing Training - {} generations, {} batches, {} episodes"
          .format(num_generations, generation_size, batch_size))
    while episode < num_episodes:
        steps += 1
        obs_stamp = time.time()
        observation = env_info.vector_observations

        action = get_action(model, observation, exploration)

        # Fetch next environment and reward
        env_info = env.step(action)[default_brain]
        reward = env_info.rewards[0]

        obs_time += time.time() - obs_stamp

        # Update memory
        observations = np.vstack([observation, observations])
        actions = np.vstack([action, actions])
        rewards = np.vstack([reward, rewards])

        if env_info.local_done[0]:

            # Store action rewards values
            action_rewards_episode = get_discounted_action_rewards(actions, rewards)
            action_rewards = np.vstack([action_rewards_episode, action_rewards])

            # Reset memory

            actions = np.empty(0).reshape(0, num_actions)
            rewards = np.empty(0).reshape(0, 1)

            # Update Environment
            episode += 1
            r = env.reset(train_mode=False)[default_brain]

            if episode % batch_size == 0:
                batch += 1

                # Update memory
                memory.add(observations, action_rewards)
                o,a = memory.get_random_batch(batch_training_size)

                # commence training
                loss = model.train_on_batch(o, a)
                #loss = model.train_on_batch(observations, action_rewards)
                generation_reward.append(np.mean(action_rewards))

                # reset batch
                observations = np.empty(0).reshape(0, num_observations)
                action_rewards = np.empty(0).reshape(0, num_actions)

                if batch % generation_size == 0:
                    generation += 1
                    now = time.time()
                    diff = now - stamp
                    stamp = now
                    exploration = exploration * exploration_reduction_rate
                    score = np.mean(generation_reward)
                    std = np.std(generation_reward)
                    generation_reward = []
                    print("Generation {} \t - Average score: {:.2f}, {:.2f} std \t in {:.1f}s, {} steps /{} at {:.4f}hz"
                          .format(generation, score, std, diff, steps, steps/(generation_size * batch_size), diff/steps))
                    obs_time = 0
                    steps = 0


def main():
    # env_name = "../envs/3DBall"  # Name of the Unity environment binary to launch
    env = UnityEnvironment(file_name=None, worker_id=0) # Add seed=n for consistent results
    train_mode = False  # Whether to run the environment in training or inference mode

    # Set the default brain to work with
    default_brain = env.brain_names[0]
    # brain = env.brains[default_brain]

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
    print(model.summary())

    # test_perfect_actions(env, default_brain)
    run_training(model, env, default_brain, 1, 1, 10)

    # Save model

    deprecated_slam_model.save_model(model, model_name)


    # Final operation; closing the environment
    env.close()


if __name__ == "__main__":
    print("Python version:")
    print(sys.version)
    # check Python version
    if sys.version_info[0] < 3:
        raise Exception("ERROR: ML-Agents Toolkit (v0.3 onwards) requires Python 3")
    main()
