using UnityEngine;
using UnityEditor;

public class NbvManager
{
    public int maxStep = 2;
    private int _currentStep = 0;
    private SystemInterface _si;

    public NbvManager(Camera depthCamera)
    {
        _si = new SystemInterface(depthCamera);
    }


    public void Reset()
    {
        _currentStep = 0;
        _si.Reset();
    }

    public float[] CollectObservations()
    {
        return _si.GetOngoingOccupancyGrid();
    }

    public System.Tuple<float, bool> Action(float[] vectorAction)
    {
        ApplyAction(vectorAction);
        return ComputeReward();
    }

    private void ApplyAction(float[] vectorAction)
    {
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

        if (_currentStep > maxStep)
        {
            done = true;
        }

        return System.Tuple.Create(reward, done);
    }

}