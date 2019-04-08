import matplotlib.pyplot as plt
import numpy as np


class SynopsisManager:

    def __init__(self, trainer, model_manager, run_name="defaultRun", max_step=15):
        self.t = trainer
        self.t.set_synopsis_manager(self)
        self.mm = model_manager

        self.run_name = run_name
        self.folder = "Summaries/"
        self.file_path = ""
        self.name = ""
        self.max_step = max_step

        self.summary = []
        self.n = "\n"

        self.create_summary_file()
        self.writelines(self.mm.get_summary())
        self.mm.print_model()

    def create_summary_file(self):
        import datetime
        suffix = datetime.datetime.now().strftime("%y%m%d_%H%M%S")
        self.name = self.folder + "_".join([self.run_name, suffix])
        self.file_path = self.name + ".txt"
        f = open(self.file_path, "w+")
        f.write("{}".format(self.run_name))
        f.close()

    def print_training_config(self):
        a = []
        a.append("\n-------------------- Training --------------------")

        a.append("{} generations, {} batches, {} episodes, {} tests".format(
            self.t.num_generations, self.t.num_batches, self.t.batch_size, self.t.num_tests))
        a.append("\t - In total {} training episodes".format(self.t.num_episodes))
        if self.t.replay:
            a.append("ReplayMemory - {} buffer size, {} batch training size".format(
                self.t.buffer_size, self.t.batch_training_size))
        else:
            a.append("No replay - {} buffer size, {} batch training size".format(
                self.t.buffer_size, self.t.batch_training_size))
        a.append("Reward alphas:")
        a.append("\t Accuracy: {:.2f}".format(self.t.alpha_accuracy))
        a.append("\t Distance: {:.2f}".format(self.t.alpha_distance))
        a.append("\t Steps: {:.2f}".format(self.t.alpha_steps))
        a.append("\t Visited Views: {:.2f}".format(self.t.alpha_views))
        a.append("\n")
        self.writelines(a)

    def print_training_summary(self):
        avg_duration_env = self.get_time_keeper_average(self.t.duration_environment)
        avg_duration_training = self.get_time_keeper_average(self.t.duration_training)
        avg_duration_action = self.get_time_keeper_average(self.t.duration_selecting_action)
        a = []
        a.append("\n-------------------- Training Summary --------------------")
        a.append("Time Durations")
        a.append("Total Training Time: {:.1f}s".format(self.t.duration_total))
        a.append("Interacting with Environment: {:.2f} - Average {:.4f}s"
                 .format(self.t.duration_environment[0], avg_duration_env))
        a.append("Training Model: {:.2f} - Average {:.4f}s"
                 .format(self.t.duration_training[0], avg_duration_training))
        a.append("Selecting Action: {:.2f} - Average{:.4f}s"
                 .format(self.t.duration_selecting_action[0], avg_duration_action))
        self.writelines(a)
        self.generation_reward_summary()
        self.plot_reward_summary()

    def print_evaluation(self, num_runs, avg_reward, avg_steps, avg_distance, avg_accuracy, distances, accuracies):
        a = []
        a.append("\n-------------------- Results --------------------")
        a.append("Number of tests: {}".format(num_runs))
        a.append("Mean reward: {:.3f},\t Mean steps: {:.1f},\t Mean distance: {:.1f},\t Avg Accuracy: {:.3f}"
                 .format(avg_reward, avg_steps, avg_distance, avg_accuracy))
        a.append("Distances: ")
        a.append(np.array2string(np.asarray(distances), separator=', ', precision=1))
        a.append("Accuracy:")
        a.append(np.array2string(np.asarray(accuracies), separator=', ', precision=3))
        self.writelines(a)
        self.plot_evaluation_summary(distances, accuracies)

    def write(self, string):
        print(string)
        f = open(self.file_path, "a")
        f.write("\n" + string)
        f.close()

    def writelines(self, lines):
        f = open(self.file_path, "a")
        for string in lines:
            print(string)
            f.write("\n" + string)
        f.close()

    def generation_reward_summary(self):
        reward = self.t.generation_reward
        f = open(self.name + ".csv", "a")
        f.write("#Reward, Steps, Distance, Accuracy\t- {} tests/gen ".format(self.t.num_tests))
        for reward, steps, distance, acc in reward:
            f.write("\n{:.4f}, {:.1f}, {:.1f}, {:.3f}".format(reward, steps, distance, acc))
        f.close()

    def plot_reward_summary(self):

        generation_reward = np.asarray(self.t.generation_reward)

        rewards = generation_reward[:, 0]
        steps = generation_reward[:, 1]
        distance = generation_reward[:, 2]
        accuracy = generation_reward[:, 3]

        plt.figure(figsize=(6, 12))

        plt.subplot(411)
        plt.title("Average Reward")
        plt.ylim([-0.5, 0.5])
        plt.plot(rewards)

        plt.subplot(412)
        plt.title("Steps")
        plt.ylim([0, self.t.max_step+1])
        plt.plot(steps)

        plt.subplot(413)
        plt.title("Distance")
        plt.ylim([0, 1500])
        plt.plot(distance)

        plt.subplot(414)
        plt.title("Accuracy")
        plt.ylim([0, 1.1])
        plt.plot(accuracy)

        plt.tight_layout(pad=0.4, w_pad=0.5, h_pad=1.2)
        plt.savefig(self.name + "_training")
        print("[Training Summary plotted]")

    def plot_evaluation_summary(self, dist, acc):
        plt.figure()

        plt.subplot(211)
        plt.title("Average Cumulative Distance")
        plt.ylim([0, 1500])
        plt.xlim([0, self.max_step])
        plt.plot(dist)

        plt.subplot(212)
        plt.title("Completion Accuracy")
        plt.ylim([0, 1.1])
        plt.xlim([0, self.max_step])
        plt.plot(acc)

        plt.tight_layout(pad=0.4, w_pad=0.5, h_pad=1.5)
        plt.savefig(self.name + "_evaluation")
        print("[Evaluation plotted]")
        plt.close()

    @staticmethod
    def get_time_keeper_average(keeper):
        return keeper[0] / keeper[1]
