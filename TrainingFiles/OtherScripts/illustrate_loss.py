import datetime

import numpy as np
import matplotlib.pyplot as plt
from os import listdir
from os.path import isfile, join

from random import randint

import csv

filepath = "testingModel100_190528_105622_loss_development_190528_123720.csv"

from numpy import genfromtxt

data = genfromtxt(filepath, delimiter=',')
loss = data[:, 0]
acc = data[:, 1]


plt.figure(figsize=(12, 6))
plt.ticklabel_format(style='sci', axis='y', scilimits=(0,0))
plt.title("Evaluated TestModel - Generation Loss Development")
# plt.ylim([0, 1500])
plt.ylim( (10**-5, 10**-4))
# plt.xlim([0, self.max_step])
plt.xlabel("Generation")
plt.ylabel("Loss")
plt.plot(loss)
plt.savefig("LossDevelopment.png")