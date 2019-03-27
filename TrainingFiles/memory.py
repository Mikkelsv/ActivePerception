import numpy as np


class Memory:

    def __init__(self, shape_input, num_output, buffer_size=500, replay=True):
        self.num_actions = num_output

        self.observations_shape = shape_input[0]
        self.views_shape = shape_input[1]

        self.observations = np.zeros(((buffer_size,) + self.observations_shape))
        self.views = np.zeros(((buffer_size,) + self.views_shape))
        self.actions = np.zeros((buffer_size, num_output))

        self.memory_index = 0
        self.buffer_size = buffer_size
        self.memory_full = False
        self.replay = replay

    def add(self, state_observations, state_views, reward):
        # new_info = np.hstack((state, reward))
        m = self.memory_index

        new_length = len(state_observations)
        current_index = min(m + new_length, self.buffer_size)

        if current_index < self.buffer_size:
            self.observations[m:current_index, :] = state_observations[:]
            self.views[m:current_index, :] = state_views[:]
            self.actions[m:current_index, :] = reward[:]
            self.memory_index += new_length
        else:
            overflow_count = (m + new_length) % self.buffer_size
            fit_count = new_length - overflow_count

            # Fit end of buffer
            self.observations[m:, :] = state_observations[:fit_count]
            self.views[m:, :] = state_views[:fit_count]
            self.actions[m:, :] = reward[:fit_count]

            # Fit start of buffer
            self.observations[:overflow_count, :] = state_observations[fit_count:]
            self.views[:overflow_count, :] = state_views[fit_count:]
            self.actions[:overflow_count, :] = reward[fit_count:]
            self.memory_index = overflow_count

            # The memory is full
            self.memory_full = True

    def get_random_batch(self, batch_size=32):
        if self.memory_full:
            indices = np.random.randint(0, self.buffer_size, batch_size)
        else:
            indices = np.random.randint(0, self.memory_index, batch_size)
        batch_observations = self.observations[indices]
        batch_views = self.views[indices]
        batch_actions = self.actions[indices]
        if not self.replay:
            self.reset_buffers()

        return batch_observations, batch_views, batch_actions

    def reset_buffers(self):
        self.observations = np.zeros(((self.buffer_size,) + self.observations_shape))
        self.views = np.zeros(((self.buffer_size,) + self.views_shape))
        self.actions = np.zeros((self.buffer_size, self.num_actions))

def main():
    d = 2
    cases = 4
    input_shape = (d, d, d, 1)
    num_actions = 2
    memory = Memory(input_shape, num_actions)
    for i in range(100):
        m = np.arange(d * d * d * cases).reshape(((-1,) + input_shape))
        r = np.arange(num_actions * cases).reshape((-1, num_actions))
        memory.add(m, r)
    o, s = memory.get_random_batch(6)
    print(o)
    print(s)


if __name__ == "__main__":
    main()
