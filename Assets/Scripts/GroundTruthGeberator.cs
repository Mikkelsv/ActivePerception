using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GroundTruthGenerator
{
    private readonly string _path = "TrainingFiles/OccupancyGrids/saved_occupancy_grids.txt";

    private DepthRenderingManager _drm;
    private ViewManager _vm;
    private PointCloudManager _pcm;
    private OccupancyGridManager _ogm;
    private StudyObjectMamanger _som;
    private Texture2D _currentRendering;

    public float[][] _grids;
    
    public int[] gridCount;

    public int[] requiredCount;
    public float requiredAccuracy;



    public GroundTruthGenerator(DepthRenderingManager drm, ViewManager vm, PointCloudManager pcm, OccupancyGridManager ogm, StudyObjectMamanger som, float requiredAccuracy = 0.99f)
    {
        // Save function messes up executable file, must make due without it
        _drm = drm;
        _vm = vm;
        _pcm = pcm;
        _ogm = ogm;
        _som = som;
        this.requiredAccuracy = requiredAccuracy;
        Generate(false, false);
    }

    public float[][] Grids()
    {
        return _grids;
    }

    public int CurrentObjectGridCount()
    {
        return gridCount[_som.CurrentObject()];
    }

    public float[][] Generate(bool load = false, bool save = true)
    {
        if (load) {
            Load();
        }
        else
        {
            Build();
            if (save)
            {
                Save();
            }
        }
        CountGrids();
        Debug.Log("Ground Truth Objects Generated");
        return _grids;
    }

    public float[][] Build()
    {
        float[][] grids = new float[_som.Count()][];

        for (int objectIndex = 0; objectIndex < _som.Count(); objectIndex++)
        {
            _som.PrepareStudyObject(objectIndex);
            _ogm.ClearGrid();
            RenderAllViews();
            float[] g = _ogm.GetGrid();
            grids[objectIndex] = g;
        }
        _grids = grids;
        Debug.Log("Generated grids of " + _som.Count() + " objects");
        _ogm.ClearGrid();
        return grids;
    }

    private void CountGrids()
    {
        gridCount = new int[_som.Count()];
        requiredCount = new int[_som.Count()];

        for (int objectIndex = 0; objectIndex < _grids.Length; objectIndex++)
        {
            gridCount[objectIndex] = CountGrid(_grids[objectIndex]);
            requiredCount[objectIndex] = (int)(gridCount[objectIndex] * requiredAccuracy);
        }
    }


    private void RenderAllViews()
    {
        for (int view = 0; view < _vm.Count(); view++)
        {
            RenderView(view);
        }
    }


    private void RenderView(int view)
    {
        Vector3 newView = _vm.SetView(view);
        _drm.SetCameraView(newView);
        _currentRendering = _drm.GetDepthRendering();
        HashSet<Vector3> pointCloud = _pcm.CreatePointSet(_currentRendering);
        _ogm.AddPoints(pointCloud);
    }

    private void Save()
    {
        using (var outf = new StreamWriter(_path))
            for (int i = 0; i < _grids.Length; i++)
            {
                outf.WriteLine(string.Join(",", _grids[i]));
            }
        Debug.Log("Ground truth stoed in " + _path);

    }

    private float[][] Load()
    {
        string[] lines = System.IO.File.ReadAllLines(_path);
        _grids = new float[lines.Length][];
        int i = 0;
        foreach (string s in lines)
        {

            _grids[i] = Array.ConvertAll(s.Split(','), float.Parse);
            i++;
        }
        Debug.Log("Loaded grids of " + lines.Length + " objects");
        return _grids;
    }  

    private int CountGrid(float[] grid)
    {
        int count = 0;
        foreach(float g in grid)
        {
            if (g > 0)
            {
                count++;
            }
        }
        return count;
    }
    

    
}
