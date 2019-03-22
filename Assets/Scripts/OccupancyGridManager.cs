﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OccupancyGridManager {

    
    private int[] _grid;
    private float[] _occupancyGrid;
    private bool[] _builtGrid;
    private float _gridSize;

    private GameObject _gridObject;
    private GameObject _referenceGrid;


    private Vector3 _alignVector;
    private float _inverseGridScale;

    private int _gridCount;
    private int _gridCountSquared;
    private int _gridCountCubed;

    public int occupiedCount;
    public int increasedOccupiedCount;

    public OccupancyGridManager(int g, float gridSize, Vector3 position)
    {
      
        _grid = new int[g * g * g];
        _occupancyGrid = new float[g * g * g + 3];
        _builtGrid = new bool[g * g * g];
        
        _gridObject = new GameObject();
        _gridObject.transform.position = position;
        _gridObject.name = "OccupancyGrid";


        //Variables
        _gridSize = gridSize;
        _gridCount = g;
        _alignVector = Vector3.one * _gridSize / 2;

        _inverseGridScale = _gridCount / _gridSize;

        _gridCountSquared = _gridCount * _gridCount;
        _gridCountCubed = _gridCountSquared * _gridCount;
    }

    public void ClearGrid()
    {
        _grid = new int[_gridCountCubed];
        _occupancyGrid = new float[_gridCountCubed] ;
        _builtGrid = new bool[_gridCountCubed];
        occupiedCount = 0;
    }

    public float[] GetGrid()
    {
        return _occupancyGrid;
    }


    public void AddPoints(HashSet<Vector3> points)
    {
        increasedOccupiedCount = 0;
        int[] newPoints = GenerateNewGrid(points);
        
        for(int i=0; i<_grid.Length; i++)
        {
            _grid[i] += newPoints[i];
            if (newPoints[i] > 0 && _occupancyGrid[i] == 0)
            {
                _occupancyGrid[i] = 1f;
                increasedOccupiedCount++;
            }
        }
        occupiedCount += increasedOccupiedCount;
    }

    public void UpdateGridObject()
    {
        //Vector3 scale = new Vector3(1f / _gridCount, 1f / _gridCount, 1f / _gridCount);
        Vector3 scale = new Vector3(_gridSize / _gridCount, _gridSize / _gridCount, _gridSize / _gridCount);
        for (int z = 0; z < _gridCount; z++)
        {
            for (int y = 0; y < _gridCount; y++)
            {
                for (int x = 0; x < _gridCount; x++)
                {
                    int i = z * _gridCount * _gridCount + y * _gridCount + x;
                    if (_grid[i] > 0 && !_builtGrid[i])
                    {

                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.parent = _gridObject.transform;

                        Vector3 pointPosition = new Vector3(x - _gridCount / 2f, y - _gridCount / 2f, z - _gridCount / 2f);
                        c.transform.localPosition = Vector3.Scale(pointPosition, scale);
                        c.transform.localScale = scale;

                        c.name = x.ToString() + "-" + y.ToString() + "-" + z.ToString();
                        _builtGrid[i] = true;
                    }
                }
            }
        }
    }

    public float[] GetObservations()
    {
        return _occupancyGrid;
    }

    public void BuildGrid(float[] grid)
    {
        Vector3 scale = new Vector3(_gridSize / _gridCount, _gridSize / _gridCount, _gridSize / _gridCount);
        for (int z = 0; z < _gridCount; z++)
        {
            for (int y = 0; y < _gridCount; y++)
            {
                for (int x = 0; x < _gridCount; x++)
                {
                    int i = z * _gridCount * _gridCount + y * _gridCount + x;
                    if (grid[i] > 0)
                    {

                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.parent = _gridObject.transform;

                        Vector3 pointPosition = new Vector3(x - _gridCount / 2f, y - _gridCount / 2f, z - _gridCount / 2f);
                        c.transform.localPosition = Vector3.Scale(pointPosition, scale);
                        c.transform.localScale = scale;

                        c.name = x.ToString() + "-" + y.ToString() + "-" + z.ToString();
                    }
                }
            }
        }

    }

    public void GenerateExampleGrid(Vector3 position)
    {
        _referenceGrid = new GameObject();
        _referenceGrid.transform.position = position;
        _referenceGrid.name = "ReferenceGrid";

        Vector3 scale = new Vector3(_gridSize / _gridCount, _gridSize / _gridCount, _gridSize / _gridCount);

        for (int z = 0; z < _gridCount; z++)
        {
            for (int y = 0; y < _gridCount; y++)
            {
                for (int x = 0; x < _gridCount; x++)
                {
                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.parent = _referenceGrid.transform;

                    Vector3 pointPosition = new Vector3(x - _gridCount / 2f, y - _gridCount / 2f, z - _gridCount / 2f);
                    c.transform.localPosition = Vector3.Scale(pointPosition, scale);
                    c.transform.localScale = scale;

                    c.name = x.ToString() + "-" + y.ToString() + "-" + z.ToString();
                }
            }
        }
    }

    private int[] GenerateNewGrid(HashSet<Vector3> points)
    {
        int[] grid = new int[_gridCountCubed];
        int x, y, z, i;
        foreach(Vector3 p0 in points)
        {
            Vector3 p = p0 + _alignVector;
            x = Mathf.FloorToInt(p.x * _inverseGridScale);
            y = Mathf.FloorToInt(p.y * _inverseGridScale) * _gridCount;
            z = Mathf.FloorToInt(p.z * _inverseGridScale) * _gridCountSquared;
            i = x + y + z;
            grid[i] += 1;
        }
        return grid;
    }


    private void UpdateGrid(HashSet<Vector3> pointCloud)
    {
        foreach (Vector3 p0 in pointCloud)
        {
            Vector3 p = p0 + _alignVector;
            int x = Mathf.FloorToInt(p.x * _inverseGridScale);
            int y = Mathf.FloorToInt(p.y * _inverseGridScale) * _gridCount;
            int z = Mathf.FloorToInt(p.z * _inverseGridScale) * _gridCountSquared;
            _grid[x + y + z] += 1;
        }
    }
}
