class SynopsisManager:

    def __init__(self, trainer, run_name="defaultRun"):
        self.t = trainer
        self.t.set_synopsis_manager(self)

        self.run_name = run_name
        self.folder = "Summaries/"
        self.file_path = ""
        self.name = ""

        self.summary = []
        self.n = "\n"

        self.create_summary_file()

    def create_summary_file(self):
        import datetime
        suffix = datetime.datetime.now().strftime("%y%m%d_%H%M%S")
        self.name = self.folder + "_".join([self.run_name, suffix])  # e.g. 'mylogfile_120508_171442'
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
        a.append("Memory - {} buffer size, {} batch training size".format(
            self.t.buffer_size, self.t.batch_training_size))
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

    def print_evaulation(self, num_runs, mean, std):
        a = []
        a.append("\n-------------------- Results --------------------")
        a.append("Number of tests: {}".format(num_runs))
        a.append("Mean reward: {:.3f} \t std: {:.4f}".format(mean, std))
        self.writelines(a)

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
        f.write("Reward,Steps")
        for a, b, in reward:
            f.write("\n{:.4f},{}".format(a, b))
        f.close()

    @staticmethod
    def get_time_keeper_average(keeper):
        return keeper[0] / keeper[1]
