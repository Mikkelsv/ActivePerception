from mlagents.envs import UnityEnvironment

from model_manager import ModelManager
from trainer import Trainer
from synopsis_manager import SynopsisManager


def main():
    use_executable = False

    env_name = "Env/ActivePerception"
    if use_executable:
        env = UnityEnvironment(file_name=env_name)
    else:
        env = UnityEnvironment(file_name=None)  # Add seed=n for consistent results

    # Investigate environment
    default_brain = env.brain_names[0]

    env_info = env.reset(train_mode=False)[default_brain]
    num_input = len(env_info.vector_observations[0])
    num_output = len(env_info.action_masks[0])

    # Fetching model
    learning_rate = 0.001
    load = False
    model_name = "greedyNBVmodel_100views"
    model_manager = ModelManager(load=load, num_views=num_output, num_output=num_output,
                                 model_name=model_name, learning_rate=learning_rate)
    model = model_manager.get_model()

    # Train
    trainer = Trainer(model, env)
    synopsis = SynopsisManager(trainer, run_name=model_name)
    trainer.train(10, 10, 5, 2)
    synopsis.print_training_summary()
    trainer.evaluate_solution(5)

    # Close environment
    env.close()

    # Save model
    model_manager.save_model()


if __name__ == "__main__":
    import sys

    print("\n--------------------------- Environment --------------------")
    print("Python version: {}".format(sys.version))
    # check Python version
    if sys.version_info[0] < 3:
        raise Exception("ERROR: ML-Agents Toolkit (v0.3 onwards) requires Python 3")
    main()
