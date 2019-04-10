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

        self.num_generations = 2
        self.num_batches = 2
        self.batch_size = 1
        self.test_size = 2
        self.evaluation_size = 10

        self.alpha_views = 0.0
        self.learning_rate = 0.001

        self.exp_acc = 1
        self.exp_dist = 1

    def run(self):
        self.execute_session("moderateNBV_views_2", 1, 0.25, 1.0, 0.5, 0.2)

    def run_multiple_variations(self):
        runs = self.fetch_runs()
        print("-Commensing learning of {} models-".format(len(runs)))
        for acc, exp_acc, dist, exp_dist, step in runs:
            model_name = self.generate_name(acc, dist, step)
            self.execute_session(model_name, acc, self.exp_acc, dist, self.exp_dist, step)

    def execute_session(self, model_name, alpha_acc, exp_acc, alpha_dist, exp_dist, alpha_steps):
        print("\n==================== New Session {} ============================".format(model_name))
        print("acc: {} - {}, dist: {} - {}, steps{}, views: {}, LR: {}\n"\
              .format(alpha_acc, exp_acc, alpha_dist,exp_dist, alpha_steps, self.alpha_views, self.learning_rate))

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

    def generate_name(self, acc, dist, steps):
        return "model_{:.0f}_{:.0f}_{:.0f}".format(acc * 100, dist * 100, steps * 100)

    def fetch_runs(self):
        runs = []
        for i in range(0, 100, 50):
            for j in range(0, 100, 50):
                for k in range(0, 100, 50):
                    if i or j or k:
                        a = np.array([i, j, k])
                        a = a / np.sum(a)
                        runs.append(tuple(a.tolist()))
        runs = list(set(runs))
        runs.sort()
        runs.reverse()
        return runs


def main():
    supervisor = Supervisor()
    supervisor.run()


if __name__ == "__main__":
    main()
