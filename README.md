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
* **NbvAgent** - Interface for the simulated environment seen by the learning system of python. The functions for interaction is discussed below.
* **NbvManager** - Manages the learning agent by interacting with the system interface
* **SystemInferface** - Abstraction of the system components use by the learning agent
* **MainController** - Allows to interact with the simulated environment inside of Unity


All the essential components of the simulated environments used by the **SystemInferface** and **MainController**:
* **ViewManager** - Generates the view candidate views (vectors) of the view sphere and manages the candidate views.
* **StudyObjectManager** - Imports and manages the 3d model objets to be used in the environment.
* **DepthRenderingManager** - Generates a 2DTexture (depth-image) from the virtual depth-camera.
* **PointCloudManager** - Generates and manages point clouds generated from the depth images.
* **OccupancyGridManager** - Generates and manages occupancy grids generated from the point clouds.
* **GroundTruthManager** - Generates and manages the ground truth of all the 3d study object models.
* **RewardManager** - Computes the reward for the current state and chosen action.

## Environment Interaction
The **Trainer** interacts with the simulated environment trough a **Env**-object created by mlagents.
The main functions and properties called are:
* **Step**(actions) - Passed the actions to apply onto the environment. Actions are on the format of a binary list of the candidate views, where 1 indicate which view to be selected.
* **Vector_observations** - Array of the collected observations from the NbvAgent. The list are (concatenated) on the format:
  * **Occupancy grid** - 32768 indices, binary format (32x32x32)
  * **Current View** - 100 indices, binary format
  * **visited Views** - 100 indices, binary format
  * **Distance** - single indice, float (distance travelled)
  * **Coverage** - single indice, float (current coverage)
  * **Reward** - 3 indices, floats (coverage reward, distance reward, step reward)
* **Reset** - Resets the environment, though not necessary as its automatically resets within the environment  

See Unities [Python API page](https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Python-API.md) for further information.

The **Env** object is either generated from an executeable of the Unity Solution or through Unity itself. Using an executeable is preferred as it gives up to 40% better performance.


## Authors

* **Mikkel Svagård** - *TDT4900 - Computer Science, Master's Thesis* - IDI, NTNU

## Acknowledgments

* Theoharis Theoharis - IDI, NTNU
* Ekrem Misimi - SINTEF Ocean

