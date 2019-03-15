using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class NbvAgent : Agent {

    [SerializeField]
    Camera depthCamera;

    private NbvManager _nvbManager;

    void Awake()
    {
        _nvbManager = new NbvManager(depthCamera);
    }

    void Start () {
        
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
