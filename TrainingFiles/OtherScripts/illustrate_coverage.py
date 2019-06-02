import datetime

import numpy as np
import matplotlib.pyplot as plt
from os import listdir
from os.path import isfile, join

from random import randint

import csv

filepath = "eval_coverage_progression_coverage_190602_151650.csv"
models = ["cave", "cube", "cube_deformed", "cylinder", "cylinder_deformed", "pyramid", "pyramid_deformed", "shroom",
          "terrain_1", "terrain_2"]

from numpy import genfromtxt

data = genfromtxt(filepath, delimiter=',')
coverage = []
for i in range(10):
    a = np.mean(data[i:i+12,:], axis=0)
    coverage.append(a[:-4])

# np.flip(coverage)
plt.figure(figsize=(12, 6))
plt.ticklabel_format(style='sci', axis='y', scilimits=(0,0))
plt.title("Coverage Development")
# plt.ylim([0, 1500])
plt.ylim([0,1.1])
# plt.xlim([0, self.max_step])
plt.xlabel("Step")
plt.ylabel("Coverage")
ind = [0,1,2,4,5]
act = [6,20,29,51,62]
#for i in range(len(coverage)):
for i in range(5):
    a = act[i]
    print(a)
    d = data[a]
    print(d)
    for j in range(12):
        if(d[j]>=0.95):
            d = d[:j+1]
            d[-1] = 0.95
            break
    plt.plot(d, label=models[ind[i]])
plt.legend(bbox_to_anchor=(1.05, 1), loc=2, borderaxespad=0.4)
plt.tight_layout(pad=0.4, w_pad=0.5, h_pad=1.2)
# plt.savefig("LossDevelopment.png")
plt.show()