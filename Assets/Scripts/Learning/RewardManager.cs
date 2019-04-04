using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RewardManager {
    private GroundTruthGenerator _gtg;
    private OccupancyGridManager _ogm;
    private StudyObjectMamanger _som;
    private ViewManager _vm;
    private readonly float requiredAccuracy;

    private int prediscovered;

    public RewardManager(GroundTruthGenerator gtg, OccupancyGridManager ogm, StudyObjectMamanger som, ViewManager vm, float requiredAccuracy)
    {
        _gtg = gtg;
        _ogm = ogm;
        _som = som;
        _vm = vm;
        this.requiredAccuracy = requiredAccuracy;
        prediscovered = 0;

    }

    public void Reset()
    {
        prediscovered = 0;
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
        int undiscovered = _gtg.gridCount[_som.CurrentObject()] - prediscovered;
        int occupied = EvaluateGrid();
        int discovered = occupied - prediscovered;
        float increasedAccuracy = discovered / undiscovered;
        prediscovered = occupied;
     
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

    public int EvaluateGrid()
    {
        int[] grid = _ogm.GetGrid();
        int[] gt = _gtg._grids[_som.CurrentObject()];
        int c = 0;
        Parallel.For(0, _ogm.gridCountCubed, i =>
        {
            if (grid[i] > 0 && gt[i] > 0)
            {
                c++;
            }
        });
        return c;
    }
}
