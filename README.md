# Active Perception Using Deep Learning and Dense Point Clouds

Repository form the course TDT4900 - Computer Science, Master's Thesis. All work and experiments is done by Mikkel Svagård during fall 2018 and spring 2019.
The work includes the investigations in regards to the implemented simulated environment of Unity and the training environment in Python.

## System Overview


### Requirements
[Unity](https://unity3d.com/get-unity/download),
[Ml-Agents](https://github.com/Unity-Technologies/ml-agents),
[Python 3.6 (Anaconda)](https://www.anaconda.com/),
TensorFlow 1.13,
Tf.Keras 2.2.4-tf,
Numpy,
Matplotlib

### Training Files
In the TrainingFiles-folder, the python files regarding the training, evaluating and testing the Voxel-based NBV policy-models resides.
This section gives a breif introduction to each file:
* **Trainer** - Object responsible for training the models using the Unity Evnironment.
* **Model_Manager** - Generates and manages the neural network models.
* **Learning_Supervisor** - Manages the training session of multiple models.
* **Testing_Supervisor** - Manages the evaluation of multiple models.
* **Synopsys_Manager** - Responsible for writing, printing and illustrating the progress and evaluations.

Other subsidiary python-files are stored in **TrainingFiles/OtherScripts/**, but they have nothing to do with the training.
Furthermore, the executables generated from Unity should be stored within **TrainingFiles/Env/**, and the library of mlagents in **TrainingFiles/mlagents/**.  
The entry file should be either the **Trainer** or the **Learning_Supervisor** to get a good graps of the system.
To run the training or evaluation sessions when everything is correctly downloaded and structured should be as simple as:  

```
python ./TrainingFiles/learning_supervisor.py
```

### Unity Files



## Authors

* **Mikkel Svagård** - *TDT4900 - Computer Science, Master's Thesis* - IDI, NTNU

## Acknowledgments

* Theoharis Theoharis - IDI, NTNU
* Ekrem Misimi - SINTEF Ocean

