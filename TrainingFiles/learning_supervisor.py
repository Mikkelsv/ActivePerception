from mlagents.envs import UnityEnvironment

from model_manager import ModelManager
from trainer import Trainer
from synopsis_manager import SynopsisManager
import numpy as np


class Supervisor:

    def __init__(self):
        self.use_executable = True
        self.load_model = False

        # REBUILD ENVIRONMENT FOR LEARNING
        self.env_name = "Env/ActivePerceptionTraining"

        self.max_step = 15
        self.num_generations = 100  # 500
        self.num_batches = 100  # 100
        self.batch_size = 1
        self.test_size = 20  # 20
        self.evaluation_size = 100  # 100

        self.alpha_views = -0.4
        self.alpha_steps = 0.2
        self.learning_rate = 0.001

        self.exp_acc = 1
        self.exp_dist = 1

    def run(self):
        self.execute_session("testingModel100", 1, 0.5, 1.0, 1.0, 0.5, 0)
        # self.run_multiple_variations()

    def run_multiple_variations(self):
        runs = self.fetch_selected_runs()
        print("-Commensing learning of {} models-".format(len(runs)))

        types = [2]
        for t in types:
            for acc, exp_acc, dist, exp_dist, steps in runs:
                model_name = self.generate_name_with_name(acc, exp_acc, dist, exp_dist, steps, t)
                print("{} using architecture {}".format(model_name,t))
        for t in types:
            for acc, exp_acc, dist, exp_dist, steps in runs:
                model_name = self.generate_name_with_name(acc, exp_acc, dist, exp_dist, steps, t)
                self.execute_session(model_name, acc, exp_acc, dist, exp_dist, steps, t)

    def execute_session(self, model_name, alpha_acc, exp_acc, alpha_dist, exp_dist, alpha_steps, t):
        print("\n==================== New Session {} ============================".format(model_name))
        print("acc: {} - {}, dist: {} - {}, steps {}, views: {}, LR: {}\n"
              .format(alpha_acc, exp_acc, alpha_dist, exp_dist, alpha_steps, self.alpha_views, self.learning_rate))

        if self.use_executable:
            env = UnityEnvironment(file_name=self.env_name)
        else:
            env = UnityEnvironment(file_name=None)
        default_brain = env.brain_names[0]
        env_info = env.reset(train_mode=False)[default_brain]
        num_output = len(env_info.action_masks[0])

        # Fetching model
        model_manager = ModelManager(load=self.load_model, num_views=num_output, num_output=num_output,
                                     model_name=model_name, learning_rate=self.learning_rate, variation=t)

        # Train
        trainer = Trainer(model_manager, env, self.max_step)
        trainer.set_reward_values(alpha_acc, exp_acc, alpha_dist, exp_dist, alpha_steps, self.alpha_views)
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

    def generate_name_with_name(self, acc, exp_acc, dist, exp_dist, steps, t):
        return "100Arch{}_{:.0f}_{:.0f}_{:.0f}_{:.0f}_{:.0f}".format(t, acc * 10, exp_acc * 10,
                                                              dist * 10, exp_dist * 10, steps * 10)

    def generate_name(self, acc, exp_acc, dist, exp_dist, steps):
        return "{}model_{:.0f}_{:.0f}_{:.0f}_{:.0f}_{:.0f}".format(self.num_generations, acc * 10, exp_acc * 10,
                                                                   dist * 10, exp_dist * 10, steps * 10)

    def fetch_runs(self):
        runs = []
        acc = [0.8, 1.2]
        exp_acc = [0.5]
        dist = [0.8, 1.2]
        exp_dist = [1.0]
        steps = [0.5, 0.7]
        for a in acc:
            for e_a in exp_acc:
                for d in dist:
                    for e_d in exp_dist:
                        for s in steps:
                            runs.append((a, e_a, d, e_d, s))
        return runs

    def fetch_selected_runs(self):
        conf = []
        #  conf.append([0.8, 1.2, 0.7])
        # conf.append([1.2, 1.2, 0.5])
        conf.append([1.0, 1.0, 0.6])
        runs = []
        for c in conf:
            r = [c[0], 0.5, c[1], 1.0, c[2]]
            runs.append(r)
        return runs


def main():
    supervisor = Supervisor()
    supervisor.run()


if __name__ == "__main__":
    main()
