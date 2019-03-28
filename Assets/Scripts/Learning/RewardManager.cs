using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager {
    private GroundTruthGenerator _gtg;
    private OccupancyGridManager _ogm;
    private StudyObjectMamanger _som;
    private ViewManager _vm;
    private readonly float requiredAccuracy;

    public RewardManager(GroundTruthGenerator gtg, OccupancyGridManager ogm, StudyObjectMamanger som, ViewManager vm, float requiredAccuracy)
    {
        _gtg = gtg;
        _ogm = ogm;
        _som = som;
        _vm = vm;
        this.requiredAccuracy = requiredAccuracy;
    }

    public float ComputeReward()
    {
        // Should be casted to 0-1 using Sidmoid function, done in python
        float r = ComputeDistanceReward() + ComputeGlobalIncreasedAccuracy();
        return r;
    }

    public float[] ComputeRewardArray()
    {
        float[] rewards = new float[]
        {
            ComputeGlobalIncreasedAccuracy(),
            ComputeDistanceReward(),
            ComputeViewReward()
        };
        return rewards;
    }

    private float ComputeViewReward()
    {
        if (_vm.GetRevisited())
        {
            return 1f;
        }
        return 0f;
    }

    public float ComputeAccuracy()
    {
        float acc = _ogm.occupiedCount *1f/ _gtg.gridCount[_som.CurrentObject()];
        return acc;
    }

    public float ComputeGlobalIncreasedAccuracy()
    {
        int prediscovered = _ogm.occupiedCount - _ogm.increasedOccupiedCount;
        int undiscovered = _gtg.gridCount[_som.CurrentObject()] - prediscovered;
        float increasedAccuracy = _ogm.increasedOccupiedCount * 1f / undiscovered;
        return increasedAccuracy;
    } 

    public float ComputeDistanceReward()
    {
         return (180f -_vm.distanceTravelled) / 180f;
    }

    public bool DetermineDone()
    {
        return ComputeAccuracy() >= this.requiredAccuracy;
    }

    public static float Sigmoid(double value)
    {
        return 1.0f / (1.0f + (float)System.Math.Exp(-value));
    }
}
