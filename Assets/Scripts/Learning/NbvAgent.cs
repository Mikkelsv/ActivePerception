using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

/// <summary>
/// Implements the mlagents Agent interface. Interface for to the simulated environment from the Python learning environment.
/// </summary>
public class NbvAgent : Agent {

    [SerializeField]
    Camera depthCamera;


    private NbvManager _nvbManager;

    void Start()
    {
        _nvbManager = new NbvManager(depthCamera);
    }
    
    public override void AgentReset()
    {
        _nvbManager.Reset();
    }

    public override void CollectObservations()
    {
        AddVectorObs(_nvbManager.CollectObservations());
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        var evaluation = _nvbManager.Action(vectorAction);
        SetReward(evaluation.Item1);
        if (evaluation.Item2)
        {
            Done();
        }
    }
}
