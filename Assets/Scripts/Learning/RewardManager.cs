using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager {
    private GroundTruthGenerator _gtg;
    private OccupancyGridManager _ogm;
    private StudyObjectMamanger _som;
    private ViewManager _vm;

    public RewardManager(GroundTruthGenerator gtg, OccupancyGridManager ogm, StudyObjectMamanger som, ViewManager vm)
    {
        _gtg = gtg;
        _ogm = ogm;
        _som = som;
        _vm = vm;
    }

    public float ComputeReward()
    {
        // Should be casted to 0-1 using Sidmoid function, done in python
        float r = ComputeDistanceReward() + ComputeGlobalIncreasedAccuracy();
        return r;
    }

    public float ComputeLocalIncreasedAccuracy()
    {
        return 1f;
    }

    public float ComputeGlobalIncreasedAccuracy()
    {
        int prediscovered = _ogm.increasedOccupiedCount - _ogm.increasedOccupiedCount;
        int undiscovered = _gtg.gridCount[_som.CurrentObject()] - prediscovered;
        float increasedAccuracy = _ogm.increasedOccupiedCount * 1f / undiscovered;
        return increasedAccuracy;
    }

    public float ComputeDistanceReward()
    {
        float r = -_vm.distanceTravelled / 180f;
        if (r == 0f)
        {
            return -1f;
        }
        else
        {
            return r;
        }
    }

    public bool DetermineDone()
    {
        return _ogm.occupiedCount > _gtg.requiredCount[_som.CurrentObject()];
    }

    public static float Sigmoid(double value)
    {
        return 1.0f / (1.0f + (float)System.Math.Exp(-value));
    }
}
