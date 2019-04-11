import numpy as np
import matplotlib.pyplot as plt


def sigmoid(x):
    return 1. / (1. + np.exp(-x))


fidelity = 1000
x = np.linspace(0, 1, fidelity)
scale = 10
a = np.array([0.5, 0.75, 1, 1.5, 2])

r = []

for i in range(len(a)):
    r.append([])
    for j in range(fidelity):
        v = x[j] ** a[i]
        r[i].append(v)

fig = plt.figure()
plt.title("Reward Transformed using Exponent")

ax = plt.axes()
for l in range(len(r)):
    # plt.plot(r[l],label="{}".format(a[l]))
    plt.plot(x, r[l], label="{}".format(a[l]))

plt.xlabel("Reward")
plt.ylabel("Transformed Reward")
plt.legend(loc='upper center', bbox_to_anchor=(0.5, -0.1), shadow=True, ncol=10)
plt.tight_layout(pad=0.4, w_pad=0.5, h_pad=1.2)

plt.show()
plt.close()

# ----------------------------------------------------- #

fidelity = 1000
x = np.linspace(0, 1, fidelity)
scale = 10
a = np.array([20, 15, 10])

exp = np.array([1.5, 2, 3])
r = []

for i in range(len(a)):
    r.append([])
    for j in range(fidelity):
        xv = x[j]
        v = (xv - np.sin(2 * np.pi * xv) / a[i]) ** exp[i]
        r[i].append(v)

fig = plt.figure()
plt.title("Reward Transformed using Exponent")

ax = plt.axes()
for l in range(len(r)):
    # plt.plot(r[l],label="{}".format(a[l]))
    plt.plot(x, r[l], label="{}".format(a[l]))

plt.xlabel("Reward")
plt.ylabel("Transformed Reward")
plt.legend(loc='upper center', bbox_to_anchor=(0.5, -0.1), shadow=True, ncol=10)
plt.tight_layout(pad=0.4, w_pad=0.5, h_pad=1.2)

# plt.show()
