from mlagents.envs import UnityEnvironment

from model_manager import ModelManager
from trainer import Trainer


def main():
    # env_name = "../envs/3DBall"  # Name of the Unity environment binary to launch
    env_name = "Env/ActivePerception"
    env = UnityEnvironment(file_name=None, worker_id=0, seed=1)  # Add seed=n for consistent results

    # Investigate environment
    default_brain = env.brain_names[0]
    env_info = env.reset(train_mode=False)[default_brain]
    num_input = len(env_info.vector_observations[0])
    num_output = len(env_info.action_masks[0])

    # Fetching model
    load = False
    model_name = "active_perception_trained"
    model_manager = ModelManager(load=load, num_input=num_input, num_output=num_output, model_name=model_name)
    model = model_manager.get_model()

    # Train
    trainer = Trainer(model, env)
    trainer.train(1, 1, 1, 1)
    trainer.print_summary()

    # Close environment
    env.close()

    # Save model
    model_manager.save_model()


if __name__ == "__main__":
    import sys

    print("Python version: {}".format(sys.version))
    # check Python version
    if sys.version_info[0] < 3:
        raise Exception("ERROR: ML-Agents Toolkit (v0.3 onwards) requires Python 3")
    main()
