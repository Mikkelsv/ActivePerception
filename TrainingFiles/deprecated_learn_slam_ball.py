from mlagents.envs import UnityEnvironment
import numpy as np
import sys

print("Python version:")
print(sys.version)

# check Python version
if (sys.version_info[0] < 3):
    raise Exception("ERROR: ML-Agents Toolkit (v0.3 onwards) requires Python 3")

#env_name = "../envs/3DBall"  # Name of the Unity environment binary to launch
env = UnityEnvironment(file_name=None, worker_id=0, seed=1)
train_mode = False  # Whether to run the environment in training or inference mode

# Set the default brain to work with
default_brain = env.brain_names[0]
brain = env.brains[default_brain]

# Reset the environment
env_info = env.reset(train_mode=train_mode)[default_brain]

#Runtime parameters
num_generations = 30;
num_epochs = 50;


# Examine the state space for the default brain
print("Agent state looks like: \n{}".format(env_info.vector_observations[0]))

# Examine the observation space for the default brain
for observation in env_info.visual_observations:
    print("Agent observations look like:")
    if observation.shape[3] == 3:
        plt.imshow(observation[0,:,:,:])
    else:
        plt.imshow(observation[0,:,:,0])

for episode in range(num_generations):
    cum_reward = 0
    for epoch in range(num_epochs):
        env_info = env.reset(train_mode=train_mode)[default_brain]
        done = False
        episode_rewards = 0
        action_size = brain.vector_action_space_size
        while not done:
            observation = env_info.vector_observations
            x = observation[0, 0]
            z = observation[0, 2]
            action = np.array([-x, -z])
            env_info = env.step(action)[default_brain]

            episode_rewards += env_info.rewards[0]
            done = env_info.local_done[0]
        cum_reward += episode_rewards

    print("Generation: {} \t- average reward: {}".format(episode,cum_reward/num_epochs))


env.close()