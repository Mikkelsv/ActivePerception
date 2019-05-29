using UnityEngine;
using UnityEditor;

/// <summary>
/// Abstraction for the NBV system, used by both agent and manual inspection, mainly communicating with the SystemInterface
/// Holds the necessary functions required of a mlagent's Agent: Reset, CollectObservations, Action.
/// </summary>
public class NbvManager
{
    private int _maxStep = 15;
    private int _currentStep = 0;
    private SystemInterface _si;

    public NbvManager(Camera depthCamera)
    {
        _si = new SystemInterface(depthCamera);
    }


    public void Reset()
    {
        //Resets the environment
        _currentStep = 0;
        _si.Reset();
    }

    public float[] CollectObservations()
    {
        //Collects the observations(input) for the neural network
        return _si.CollectObservations();
    }

    public System.Tuple<float, bool> Action(float[] vectorAction)
    {
        //Applies the action and returns the reward of said action 
        ApplyAction(vectorAction);
        return ComputeReward();
    }

    private void ApplyAction(float[] vectorAction)
    {
        //Applies the action from the actionvector, that is moving the camera to the selected view
        int view = (int)vectorAction[0];
        _si.RenderView(view);
        _currentStep++;
    }

    private System.Tuple<float, bool> ComputeReward()
    {
        //Compute the reward of the chosen view
        //Determine if done based on accuracy or other measures

        bool done = false;
        float reward = _si.GetScore();

        if (_currentStep > _maxStep || _si.GetDoneOnAccuracy())
        {
            done = true;
        }

        return System.Tuple.Create(reward, done);
    }

}