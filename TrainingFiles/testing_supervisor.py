from mlagents.envs import UnityEnvironment

from model_manager import ModelManager
from trainer import Trainer
from synopsis_manager import SynopsisManager
import numpy as np
import os


class Tester:

    def __init__(self):
        """
        Constructor for the tester
        Object responsible for initiating the testing and evaluation of the models in the selected folder
        """
        self.use_executable = True  # Whether using a unity executable or connecting to Unity itself
        self.path_to_models = "Models/Test_models/"

        # The enviroment to use for evaluation.
        # Almost similar to that when training, but its deterministic in choosing the next object of the 120 candidates
        self.env_name = "Env/ActivePerceptionEvaluation50"

        self.max_step = 15
        self.evaluation_size = 120  # Tests every object once

    def run_models(self):
        """
        Evaluates all the models in target folder
        """
        models = os.listdir(self.path_to_models)
        print("-Commensing evaluation of {} models-".format(len(models)))
        for model in models:
            model_name = model.split(".")[0]
            self.evaluate_model(model_name)

    def evaluate_model(self, model_name):
        """
        Session for evaluating every model. Uses the concept of the Trainer in a deterministic manner
        :param model_name: Name of the model to be evaluated
        """
        print("\n==================== New Evaluation {} ============================".format(model_name))

        if self.use_executable:
            env = UnityEnvironment(file_name=self.env_name)
        else:
            env = UnityEnvironment(file_name=None)

        default_brain = env.brain_names[0]
        env_info = env.reset(train_mode=False)[default_brain]
        num_output = len(env_info.action_masks[0])

        # Fetching model
        model_path = self.path_to_models + model_name + ".h5"
        model_manager = ModelManager(load=True, num_views=num_output, num_output=num_output,
                                     model_name=model_path)

        # Change the model name
        if "_" in model_name and False:
            model_name = "evaluation_" + model_name.split("_", 1)[1]
        else:
            model_name = "eval_" + model_name
        model_name = "eval_coverage_progression"

        # Evaluating the model
        trainer = Trainer(model_manager, env, self.max_step)
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
    tester.run_models()


if __name__ == "__main__":
    main()
