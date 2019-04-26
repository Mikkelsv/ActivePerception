import datetime

import numpy as np
import matplotlib.pyplot as plt
from os import listdir
from os.path import isfile, join

from random import randint

folder = "Summaries/Stored/"


def parse_results(results):
    a = results[2].replace(",", "").split()
    s = []
    for i in a:
        if is_number(i):
            s.append(float(i))
    reward, step, distance, acc = tuple(s)
    distances = results[4] + results[5]
    accuracies = results[7] + results[8]

    distances = parse_array_from_string(distances, True)
    accuracies = parse_array_from_string(accuracies, True)

    return reward, step, distance, acc, distances, accuracies


def parse_size(size):
    a = size.split()
    s = []
    for i in a:
        if i.isdigit():
            s.append(int(i))
    return tuple(s)


def fetch_evaluation_files(p):
    onlyfiles = [f for f in listdir(p) if isfile(join(p, f))]
    return onlyfiles


def plot_1(names, distances, accuracies):
    plt.figure("Distance & Accuracy Comparison", figsize=(12, 16))
    l = len(accuracies)
    plt.subplot(211)
    plt.title("Accuracy")
    plt.xlabel("Steps")
    plt.ylabel("Accuracy")
    plt.ylim([0, 1.1])
    for i in range(l):
        plt.plot(accuracies[i], label=names[i])

    plt.subplot(212)
    plt.title("Distance")
    plt.ylim([0, 1500])
    plt.xlabel("Steps")
    plt.ylabel("Distance in Angles")
    for i in range(l):
        plt.plot(distances[i], label=names[i])

    plt.legend(loc='upper center', bbox_to_anchor=(0.5, -0.1), shadow=True, ncol=3)
    plt.tight_layout(pad=0.4, w_pad=0.5, h_pad=1.2)
    suffix = datetime.datetime.now().strftime("%y%m%d_%H%M%S")
    plt.savefig(folder + suffix + "Progress_Comparison")
    plt.show()
    plt.close()
    print("[Training Summary plotted]")


def plot_2(names, mean_dist, mean_acc, steps, rewards):
    y_pos = np.arange(len(names))

    colors = [(230, 25, 75), (60, 180, 75), (255, 225, 25), (0, 130, 200), (245, 130, 48), (145, 30, 180),
              (70, 240, 240), (240, 50, 230), (210, 245, 60), (250, 190, 190), (0, 128, 128), (230, 190, 255),
              (170, 110, 40), (255, 250, 200), (128, 0, 0), (170, 255, 195), (128, 128, 0), (255, 215, 180),
              (0, 0, 128), (128, 128, 128), (0, 0, 0)]

    colors = np.asarray(colors) / 255

    suffix = datetime.datetime.now().strftime("%y%m%d_%H%M%S")
    plt.figure("Result Comparison - Distance", figsize=(8, 6))

    plt.barh(y_pos, mean_dist, 0.4, align='center', alpha=0.5, color=colors)
    plt.yticks(y_pos, names)
    plt.xlabel('Mean Distance')
    plt.xlim([0, 1500])
    plt.ylabel("Model")
    plt.tight_layout(pad=4.0)
    plt.savefig(folder + suffix + "_Results_Distance")

    plt.figure("Result Comparison - Steps", figsize=(8, 6))
    plt.barh(y_pos, steps, 0.4, align='center', alpha=0.5, color=colors)
    plt.yticks(y_pos, names)
    plt.xlabel('Mean Steps')
    plt.xlim([0, 16])
    plt.ylabel("Model")
    plt.tight_layout(pad=4.0)
    plt.savefig(folder + suffix + "_Results_Steps")

    plt.figure("Result Comparison - Accuracy", figsize=(8, 6))
    plt.barh(y_pos, mean_acc, 0.4, align='center', alpha=0.5, color=colors)
    plt.yticks(y_pos, names)
    plt.xlabel('Mean Accuracy')
    plt.xlim([0, 1.1])
    plt.ylabel("Model")
    plt.tight_layout(pad=4.0)
    plt.savefig(folder + suffix + "_Results_Accuracy")

    plt.figure("Result Comparison - Reward", figsize=(8, 6))
    plt.barh(y_pos, rewards, 0.4, align='center', alpha=0.5, color=colors)
    plt.yticks(y_pos, names)
    plt.xlabel('Reward')
    plt.xlim([-0.3, 0.7])
    plt.ylabel("Model")
    plt.ylabel("Model")
    plt.tight_layout(pad=4.0)
    plt.savefig(folder + suffix + "_Results_Reward")

    # plt.show()
    plt.close()


def evaluate(path, full=True):
    files = fetch_evaluation_files(path)
    names = []
    rewards = []
    steps = []
    mean_dist = []
    mean_acc = []
    dist = []
    acc = []
    i = 0
    for p in files:
        p = path + p
        f = open(p, "r")
        content = f.readlines()
        name = content[0]
        names.append(name)
        print(p)
        if (full):
            size = content[10]
            generations, batches, episodes, tests = parse_size(size)

            r = 19 + generations + 9
        else:
            r = 9
        results = content[r:]
        reward, step, distance, accuracy, distances, accuracies = parse_results(results)
        rewards.append(reward)
        steps.append(step)
        mean_dist.append(distance)
        mean_acc.append(accuracy)
        dist.append(distances)
        acc.append(accuracies)
    plot_1(names, dist, acc)
    plot_2(names, mean_dist, mean_acc, steps, rewards)


def is_number(s):
    try:
        float(s)
        return True
    except ValueError:
        return False


def parse_array_from_string(a, add_zero=False):
    a = a.replace("[", "").replace("]", "").replace(" ", "").replace("\n", "").split(",")
    f = []
    if add_zero:
        f.append(0)
    for i in a:
        if is_number(i):
            f.append(float(i))
    return f


def main():
    folder = "Summaries/Stored/Evaluate/"
    evaluate(folder, False)


if __name__ == "__main__":
    main()
