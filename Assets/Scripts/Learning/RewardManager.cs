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

    public int prediscovered;

    public float reward;
    public float distance;
    public float accuracy;
    public float increasedAccuracy;
    public float viewReward;

    public RewardManager(GroundTruthGenerator gtg, OccupancyGridManager ogm, StudyObjectMamanger som, ViewManager vm, float requiredAccuracy)
    {
        _gtg = gtg;
        _ogm = ogm;
        _som = som;
        _vm = vm;
        this.requiredAccuracy = requiredAccuracy;
        prediscovered = 0;
        distance = 0f;
        accuracy = 0f;
        increasedAccuracy = 0f;
        viewReward = 0f;
    }

    public void Reset()
    {
        prediscovered = 0;
        distance = 0f;
        accuracy = 0f;
        increasedAccuracy = 0f;
        viewReward = 0f;
    }

    public void ComputeRewards()
    {
        ComputeAccuracy();
        distance = ComputeDistanceReward();
        viewReward = ComputeViewReward();
    }

    public float[] GetRewardArray()
    {
        float[] rewards = new float[]
        {
            increasedAccuracy,
            distance,
            viewReward
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

    private void ComputeAccuracy()
    {
        int undiscovered = _gtg.gridCount[_som.CurrentObject()] - prediscovered;
        int occupied = EvaluateGrid();
        accuracy = occupied * 1f / _gtg.gridCount[_som.CurrentObject()];

        int discovered = occupied - prediscovered;
        increasedAccuracy= discovered * 1f / undiscovered;

        prediscovered = occupied;
    }

    private float ComputeDistanceReward()
    {
         return _vm.distanceTravelled / 180f;
    }

    public bool DetermineDone()
    {
        return accuracy >= this.requiredAccuracy;
    }

    private static float Sigmoid(double value)
    {
        return 1.0f / (1.0f + (float)System.Math.Exp(-value));
    }

    private int EvaluateGrid()
    {
        int[] grid = _ogm.GetPointGrid();
        int[] gt = _gtg._grids[_som.CurrentObject()];
        int c = 0;
        //Parallel.For(0, _ogm.gridCountCubed, i =>
        for(int i=0;i<_ogm.gridCountCubed; i++)
        {
            if (grid[i] > 0 && gt[i] > 0)
            {
                c++;
            }
        };
        return c;
    }
}
