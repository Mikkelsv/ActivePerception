﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager {
    private GroundTruthGenerator _gtg;
    private OccupancyGridManager _ogm;
    private StudyObjectMamanger _som;

    public RewardManager(GroundTruthGenerator gtg, OccupancyGridManager ogm, StudyObjectMamanger som)
    {
        _gtg = gtg;
        _ogm = ogm;
        _som = som;
    }

    public float ComputeLocalIncreasedAccuracy()
    {
        return 1f;
    }

    public float ComputeGlobalIncreasedAccuracy()
    {
        float r = _ogm.increasedOccupiedCount*1f / _gtg.gridCount[_som.CurrentObject()];
        Debug.Log(_gtg.gridCount[_som.CurrentObject()]);
        Debug.Log(_ogm.increasedOccupiedCount);
        return r;
    }
}