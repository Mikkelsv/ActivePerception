import numpy as np


class Memory:

    def __init__(self, num_observations, num_actions, buffer_size=500):
        self.num_actions = num_actions
        self.num_observations = num_observations
        self.memory = np.zeros((buffer_size, num_observations + num_actions))
        self.memory_index = 0
        self.buffer_size = buffer_size
        self.memory_full = False

    def add(self, state, reward):
        new_info = np.hstack((state, reward))
        m = self.memory_index

        new_length = len(new_info)
        current_index = min(m + new_length, self.buffer_size)

        if current_index < self.buffer_size:
            self.memory[m:current_index, :] = new_info[:]
            self.memory_index += new_length
        else:
            overflow_count = (m + new_length) % self.buffer_size
            fit_count = new_length - overflow_count

            # Fit end of buffer
            self.memory[m:, :] = new_info[:fit_count]
            # Fit start of buffer
            self.memory[:overflow_count, :] = new_info[fit_count:]
            self.memory_index = overflow_count

            # The memory is full
            self.memory_full = True

    def get_random_batch(self, batch_size=32):
        if self.memory_full:
            indices = np.random.randint(0, self.buffer_size, batch_size)
        else:
            indices = np.random.randint(0, self.memory_index, batch_size)
        batch = self.memory[indices]
        batch_observations = batch[:, :self.num_observations]
        batch_actions = batch[:, self.num_observations:]
        return batch_observations, batch_actions


def main():
    memory = Memory(4, 2)
    for i in range(100):
        m = np.arange(20).reshape((-1, 4))
        r = np.arange(10).reshape((-1, 2))
        memory.add(m, r)
    o, s = memory.get_random_batch(6)
    print(o)
    print(s)


if __name__ == "__main__":
    main()
