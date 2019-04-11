from mlagents.envs import UnityEnvironment

from model_manager import ModelManager
from trainer import Trainer
from synopsis_manager import SynopsisManager
import numpy as np


class Supervisor:

    def __init__(self):
        self.use_executable = True
        self.load_model = False
        self.env_name = "Env/ActivePerception"
        self.max_step = 15

        self.num_generations = 200
        self.num_batches = 50
        self.batch_size = 1
        self.test_size = 20
        self.evaluation_size = 100

        self.alpha_views = -0.3
        self.alpha_steps = 0.2
        self.learning_rate = 0.001

        self.exp_acc = 1
        self.exp_dist = 1

    def run(self):
        # self.execute_session("moderateNBV_views_2", 1, 0.25, 1.0, 0.5)
        self.run_multiple_variations()

    def run_multiple_variations(self):
        runs = self.fetch_runs()
        print("-Commensing learning of {} models-".format(len(runs)))
        for acc, exp_acc, dist, exp_dist in runs:
            model_name = self.generate_name(acc, exp_acc, dist, exp_dist)
            self.execute_session(model_name, acc, exp_acc, dist, exp_dist)

    def execute_session(self, model_name, alpha_acc, exp_acc, alpha_dist, exp_dist):
        print("\n==================== New Session {} ============================".format(model_name))
        print("acc: {} - {}, dist: {} - {}, steps {}, views: {}, LR: {}\n"
              .format(alpha_acc, exp_acc, alpha_dist, exp_dist, self.alpha_steps, self.alpha_views, self.learning_rate))

        if self.use_executable:
            env = UnityEnvironment(file_name=self.env_name)
        else:
            env = UnityEnvironment(file_name=None)
        default_brain = env.brain_names[0]
        env_info = env.reset(train_mode=False)[default_brain]
        num_output = len(env_info.action_masks[0])

        # Fetching model
        model_manager = ModelManager(load=self.load_model, num_views=num_output, num_output=num_output,
                                     model_name=model_name, learning_rate=self.learning_rate)
        model = model_manager.get_model()

        # Train
        trainer = Trainer(model, env, self.max_step)
        trainer.set_reward_values(alpha_acc, exp_acc, alpha_dist, exp_dist, self.alpha_steps, self.alpha_views)
        synopsis = SynopsisManager(trainer, model_manager, run_name=model_name, max_step=self.max_step)
        trainer.train(self.num_generations, self.num_batches, self.batch_size, self.test_size)
        synopsis.print_training_summary()
        trainer.evaluate_solution(self.evaluation_size)

        # Close environment
        env.close()

        # Save model
        model_manager.save_model()

        # Cleanup
        # del trainer.memory
        del trainer
        del synopsis
        del model_manager

    def generate_name(self, acc, exp_acc, dist, exp_dist):
        return "{}model_{:.0f}_{:.0f}_{:.0f}_{:.0f}".format(self.num_generations, acc * 100, exp_acc * 100, dist * 100,
                                                            exp_dist * 100)

    def fetch_runs(self):
        runs = []
        acc = [0.25, 0.5, 1.0]
        exp_acc = [0.5, 1.0]
        dist = [0.25, 0.5, 1.0]
        exp_dist = [1.0, 2.0]
        for a in acc:
            for e_a in exp_acc:
                for d in dist:
                    for e_d in exp_dist:
                        runs.append((a, e_a, d, e_d))
        return runs


def main():
    supervisor = Supervisor()
    supervisor.run()


if __name__ == "__main__":
    main()
