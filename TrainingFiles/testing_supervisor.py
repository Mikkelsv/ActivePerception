from mlagents.envs import UnityEnvironment

from model_manager import ModelManager
from trainer import Trainer
from synopsis_manager import SynopsisManager
import numpy as np
import os

class Tester:

    def __init__(self):
        self.use_executable = True
        self.path_to_models = "Models/Test_models/"

        # REBUILD ENVIRONMENT FOR TESTING
        self.env_name = "Env/ActivePerception"

        self.max_step = 15
        self.evaluation_size = 120

    def run(self):
        # self.execute_session("moderateNBV_views_2", 1, 0.25, 1.0, 0.5)
        self.run_models()

    def run_models(self):
        models = os.listdir(self.path_to_models)
        print("-Commensing evaluation of {} models-".format(len(models)))
        for model in models:
            model_name = model.split(".")[0]
            self.evaluate_model(model_name)

    def evaluate_model(self, model_name):
        print("\n==================== New Evaluation {} ============================".format(model_name))

        if self.use_executable:
            env = UnityEnvironment(file_name=self.env_name)
        else:
            env = UnityEnvironment(file_name=None)
        default_brain = env.brain_names[0]
        env_info = env.reset(train_mode=False)[default_brain]
        num_output = len(env_info.action_masks[0])

        # Fetching model
        model_path = self.path_to_models + model_name+".h5"
        model_manager = ModelManager(load=True, num_views=num_output, num_output=num_output,
                                     model_name=model_path)
        model = model_manager.get_model()
        model_name = "evaluation_" + model_name.split("_", 1)[1]

        # Train
        trainer = Trainer(model, env, self.max_step)
        synopsis = SynopsisManager(trainer, model_manager, run_name=model_name, max_step=self.max_step)
        trainer.evaluate_solution(self.evaluation_size)

        # Close environment
        env.close()

        # Cleanup
        # del trainer.memory
        del trainer
        del synopsis
        del model_manager


def main():
    tester = Tester()
    tester.run()


if __name__ == "__main__":
    main()
