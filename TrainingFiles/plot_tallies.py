import numpy as np
import matplotlib.pyplot as plt


def plot_tallies(views, points, name="tallies"):
    view_tally = np.genfromtxt(views, delimiter=", ")
    view_cumsum = np.cumsum(view_tally, axis=1)
    point_tally = np.genfromtxt(points, delimiter=", ")
    point_cumsum = np.cumsum(point_tally, axis=1)

    plt.figure(name,figsize=(12, 6))
    models = ["cave", "cube", "cube_deformed", "cylinder", "cylinder_deformed", "pyramid", "pyramid_deformed", "shroom",
              "terrain_1", "terrain_2"]

    plt.subplot(221)
    plt.title("View Tally")
    plt.xlabel("Views")
    plt.ylabel("Tally")
    for i in range(len(view_tally)):
        plt.plot(view_tally[i], label=models[i])

    plt.subplot(222)
    plt.title("View Cumsum")
    plt.xlabel("Views")
    plt.ylabel("Tally Cumsum")
    for i in range(len(view_tally)):
        plt.plot(view_cumsum[i], label=models[i])

    plt.subplot(223)
    plt.title("Point Tally")
    plt.xlabel("Points")
    plt.ylabel("Tally")
    for i in range(len(view_tally)):
        plt.plot(point_tally[i], label=models[i])

    plt.subplot(224)
    plt.title("Point Cumsum")
    plt.xlabel("Points")
    plt.ylabel("Tally Cumsum")
    for i in range(len(view_tally)):
        plt.plot(point_cumsum[i], label=models[i])

    plt.legend(bbox_to_anchor=(1.05, 1), loc=2, borderaxespad=0.)
    plt.tight_layout(pad=0.4, w_pad=0.5, h_pad=1.2)
    plt.savefig("Tallies/" + name)
    plt.show()


def main():
    tally_path_views = "Tallies/tally_of_ground_truth_views.csv"
    tally_path_points = "Tallies/tally_of_ground_truth_points.csv"
    plot_tallies(tally_path_views, tally_path_points, "tallies_raw")

    tally_path_views = "Tallies/tally_of_ground_truth_views_reduced.csv"
    tally_path_points = "Tallies/tally_of_ground_truth_points_reduced.csv"
    plot_tallies(tally_path_views, tally_path_points, "tallies_reduced")

    tally_path_views = "Tallies/tally_of_removed_views.csv"
    tally_path_points = "Tallies/tally_of_removed_points.csv"
    plot_tallies(tally_path_views, tally_path_points, "tallies_removed")


if __name__ == "__main__":
    main()
