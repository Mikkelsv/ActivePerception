using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class GroundTruthGenerator
{
    private readonly string _basePath = "TrainingFiles/OccupancyGrids/saved_occupancy_grids"; //for running in environment
    //private readonly string _basePath = "OccupancyGrids/saved_occupancy_grids"; //for runinng executeable
    private string _path;

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

    private int viewRequirement = 4;
    private int pointRequirement = 12;


    public GroundTruthGenerator(DepthRenderingManager drm, ViewManager vm, PointCloudManager pcm, OccupancyGridManager ogm, StudyObjectMamanger som, float requiredAccuracy = 0.99f)
    {
        // Save function messes up executable file, must make due without it
        _path = _basePath + "_" + vm.Count().ToString() + ".txt";
        _drm = drm;
        _vm = vm;
        _pcm = pcm;
        _ogm = ogm;
        _som = som;
        this.requiredAccuracy = requiredAccuracy;
        //Generate(false); // Do not count tallies
        //Save();
        _grids = Load();
        CountGrids();
    }

    public int[][] Grids()
    {
        return _grids;
    }

    public int CurrentObjectGridCount()
    {
        return gridCount[_som.CurrentObject()];
    }

    public int[][] Generate(bool document = true)
    {
        if (document)
        {
            return GenerateWithDocumentation();
        }

        int[][] grids = new int[_som.Count()][];
        for (int objectIndex = 0; objectIndex < _som.Count(); objectIndex++)
        {
            _som.PrepareStudyObject(objectIndex);
            _ogm.ClearGrid();
            RenderAllViews();

            int[] viewGrid = _ogm.GetViewGrid();
            int[] pointGrid = _ogm.GetPointGrid();

            ReduceGrid(viewGrid, pointGrid);

            grids[objectIndex] = viewGrid;
        }
        _grids = grids;
        _ogm.ClearGrid();
        Debug.Log("Generated grids of " + _som.Count() + " objects");
        return grids;
    }

    private int[][] GenerateWithDocumentation()
    {
        int[][] grids = new int[_som.Count()][];


        int[][] talliesPoints = new int[_som.Count()][];
        int[][] talliesReducedPoints = new int[_som.Count()][];
        int[][] talliesRemovedPoints = new int[_som.Count()][];

        int[][] talliesViews = new int[_som.Count()][];
        int[][] talliesReducedViews = new int[_som.Count()][];
        int[][] talliesRemovedViews = new int[_som.Count()][];

        int[] pointsRemovedGrid;
        int[] viewsRemovedGrid;

        for (int objectIndex = 0; objectIndex < _som.Count(); objectIndex++)
        {
            _som.PrepareStudyObject(objectIndex);
            _ogm.ClearGrid();
            RenderAllViews();
            int[] pointGrid = _ogm.GetPointGrid();
            int[] viewGrid = _ogm.GetViewGrid();

            int[] tallyPoints = TallyGrid(pointGrid, 2000);
            int[] tallyViews = TallyGrid(viewGrid, _vm.Count());

            pointsRemovedGrid = new int[pointGrid.Length];
            viewsRemovedGrid = new int[viewGrid.Length];

            ReduceGrid(viewGrid, pointGrid, viewsRemovedGrid, pointsRemovedGrid);

            int[] tallyReducedPoints = TallyGrid(pointGrid, 2000);
            int[] tallyReducedViews = TallyGrid(viewGrid, _vm.Count());

            int[] tallyRemovedPoints = TallyGrid(pointsRemovedGrid, pointRequirement);
            int[] tallyRemovedViews = TallyGrid(viewsRemovedGrid, viewRequirement);

            talliesPoints[objectIndex] = tallyPoints;
            talliesReducedPoints[objectIndex] = tallyReducedPoints;
            talliesRemovedPoints[objectIndex] = tallyRemovedPoints;

            talliesViews[objectIndex] = tallyViews;
            talliesReducedViews[objectIndex] = tallyReducedViews;
            talliesRemovedViews[objectIndex] = tallyRemovedViews;

            grids[objectIndex] = viewGrid;
        }

        WriteIntsToFile("TrainingFiles/Tallies/tally_of_ground_truth_points_raw.csv", talliesPoints);
        WriteIntsToFile("TrainingFiles/Tallies/tally_of_ground_truth_points_reduced.csv", talliesReducedPoints);
        WriteIntsToFile("TrainingFiles/Tallies/tally_of_removed_points.csv", talliesRemovedPoints);

        WriteIntsToFile("TrainingFiles/Tallies/tally_of_ground_truth_views_raw.csv", talliesViews);
        WriteIntsToFile("TrainingFiles/Tallies/tally_of_ground_truth_views_reduced.csv", talliesReducedViews);
        WriteIntsToFile("TrainingFiles/Tallies/tally_of_removed_views.csv", talliesRemovedViews);

        _grids = grids;
        _ogm.ClearGrid();
        CountGrids();
        Debug.Log("Generated grids and tallies of " + _som.Count() + " objects");
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
        Debug.Log("Ground truth stored in " + _path);

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
        Debug.Log("Loaded grids of " + lines.Length + " objects from " + _path);
        return _grids;
    }  

    private int CountGrid(int[] grid)
    {
        int count = 0;
        foreach(int g in grid)
        {
            if (g > 0)
            {
                count++;
            }
        }
        return count;
    }

    private void ReduceGrid(int[] viewGrid, int[] pointGrid)
    {
        for (int i = 0; i < viewGrid.Length; i++)
        {
            if (viewGrid[i] <= viewRequirement && pointGrid[i] <= pointRequirement && viewGrid[i] > 0)
            {
                viewGrid[i] = 0;
            }
        }
    }

    private void ReduceGrid(int[] viewGrid, int[] pointGrid, int[] removedViewsGrid, int[] removedPointsGrid)
    {
        for(int i=0; i<viewGrid.Length; i++)
        {
            if (viewGrid[i] < viewRequirement && pointGrid[i] < pointRequirement && viewGrid[i]>0)
            {
                removedViewsGrid[i] = viewGrid[i];
                removedPointsGrid[i] = pointGrid[i];
                pointGrid[i] = 0;
                viewGrid[i] = 0;
            }
        }
    }

    private int[] TallyGrid(int[] grid, int tallyLength)
    {
        // Takes in count of the different voxels.
        // Returns a tally of the counted voxels, i.e. how many times they have been seen
        
        int[] tally = new int[tallyLength];
        foreach (int i in grid)
        {
            if (i > 0)
            {
                if (i >= tallyLength)
                {
                    tally[tallyLength-1]++;
                }
                else
                {
                    tally[i]++;
                }
            }
        }

        return tally;
    }

    private void WriteIntsToFile(string path, int[][] lines)
    {
        using (var outf = new StreamWriter(path))
        {
            foreach(int[] ints in lines)
            {
                string[] strings = Array.ConvertAll(ints, x => x.ToString());
                string s = string.Join(", ", strings);
                outf.WriteLine(s);
            }
        }
    }
    
    private void WriteToFile(string path, string[] lines)
    {
        using (var outf = new StreamWriter(path))
            foreach(string s in lines)
            {
                outf.WriteLine(s);
            }
    }

    
}
