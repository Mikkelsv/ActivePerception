# Active Perception Using Deep Learning and Dense Point Clouds

Repository form the course TDT4900 - Computer Science, Master's Thesis. All work and experiments is done by Mikkel Svagård during fall 2018 and spring 2019.
The work includes the investigations in regards to the implemented simulated environment of Unity and the training environment in Python.

## Requirements
[Unity](https://unity3d.com/get-unity/download),
[Ml-Agents](https://github.com/Unity-Technologies/ml-agents),
[Python 3.6 (Anaconda)](https://www.anaconda.com/),
TensorFlow 1.13,
Tf.Keras 2.2.4-tf,
Numpy,
Matplotlib

## Python - Training Files
In the TrainingFiles-folder, the python files regarding the training, evaluating and testing the Voxel-based NBV policy-models resides.
This section gives a breif introduction to each file, with more information within the comments of the code:
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
## Unity - Simulated Environment
The entire repository of ActivePerception doubles as a Unity Project. The following sections describe the structure of the project and introduces the most important files.

### Unity Structure
The project is structured as most Unity Projects, with the highly essential **MlAgents** library imported as an asset:
* **Assets/**
  * **Resources/** - Contains assets such as the 3D models, model prefabs and trained policy-models
  * **Scenes/** - The main scene of the simulated environment
  * **Scripts/** - All the scripts of the program, expanded on below in **Unity Files**.
  * **Shaders/** - Contains the depth shader for the virtual depth camera

### Unity Files
 The following files are of the most importance once in regards to the simulated environment, and is breifly introduced. For further information, see the comments within the code:
* **MainController** -




## Authors

* **Mikkel Svagård** - *TDT4900 - Computer Science, Master's Thesis* - IDI, NTNU

## Acknowledgments

* Theoharis Theoharis - IDI, NTNU
* Ekrem Misimi - SINTEF Ocean

