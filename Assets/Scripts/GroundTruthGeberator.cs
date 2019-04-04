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

    public int[][] _grids;
    
    public int[] gridCount;

    public int[] requiredCount;
    public float requiredAccuracy;

    private int gridRequirement = 10;


    public GroundTruthGenerator(DepthRenderingManager drm, ViewManager vm, PointCloudManager pcm, OccupancyGridManager ogm, StudyObjectMamanger som, float requiredAccuracy = 0.99f)
    {
        // Save function messes up executable file, must make due without it
        _drm = drm;
        _vm = vm;
        _pcm = pcm;
        _ogm = ogm;
        _som = som;
        this.requiredAccuracy = requiredAccuracy;
        Generate();
    }

    public int[][] Grids()
    {
        return _grids;
    }

    public int CurrentObjectGridCount()
    {
        return gridCount[_som.CurrentObject()];
    }

    public int[][] Generate()
    {
        int[][] grids = new int[_som.Count()][];

        for (int objectIndex = 0; objectIndex < _som.Count(); objectIndex++)
        {
            _som.PrepareStudyObject(objectIndex);
            _ogm.ClearGrid();
            RenderAllViews();
            int[] g = _ogm.GetGrid();
            ReduceGrid(g);
            grids[objectIndex] = g;
            EvaluateGrid(g);

        }
        _grids = grids;
        _ogm.ClearGrid();
        CountGrids();
        Debug.Log("Generated grids of " + _som.Count() + " objects");
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
        _ogm.AddPoints(pointCloud, false);
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

    private int[][] Load()
    {
        string[] lines = System.IO.File.ReadAllLines(_path);
        _grids = new int[lines.Length][];
        int i = 0;
        foreach (string s in lines)
        {

            _grids[i] = Array.ConvertAll(s.Split(','), int.Parse);
            i++;
        }
        Debug.Log("Loaded grids of " + lines.Length + " objects");
        return _grids;
    }  

    private int CountGrid(int[] grid)
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

    private void ReduceGrid(int[] grid)
    {
        for(int i=0; i<grid.Length; i++)
        {
            if (grid[i] < gridRequirement)
            {
                grid[i] = 0;
            }
        }
    }

    private void EvaluateGrid(int[] grid)
    {
        int g10 = 0;
        int o5 = 0;
        int o4 = 0;
        int o3 = 0;
        int o2 = 0;
        int o1 = 0;
        int count = CountGrid(grid);
        foreach (float g in grid)
        {
            if (g > 10)
            {
                g10++;
            }
            else if (g > 4)
            {
                o5++;
            }
            else if (g > 3)
            {
                o4++;
            }
            else if (g > 2)
            {
                o3++;
            }
            else if (g > 1)
            {
                o2++;
            }
            else if (g > 0)
            {
                o1++;
            }
        }
        Debug.Log(String.Format("{0} object - {1}: {2}, {3}, {4}, {5}, {6}, {7}", _som.CurrentObject(), count, g10, o5, o4, o3, o2, o1));

    }
    

    
}
