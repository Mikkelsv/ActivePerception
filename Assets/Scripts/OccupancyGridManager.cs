using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OccupancyGridManager {

    private int _gridCount;
    private int[] _grid;
    private float _gridSize;

    public OccupancyGridManager(int g, float gridSize = 1f)
    {
        _gridCount = g;
        _grid = new int[g * g * g];
        _gridSize = gridSize;
        
    }


    public void AddPoints(HashSet<Vector3> points)
    {
        int[] newPoints = GenerateGrid(points);
        
        for(int i=0; i<_grid.Length; i++)
        {
            _grid[i] += newPoints[i];
        }
    }



    private int[] GenerateGrid(HashSet<Vector3> points)
    {
        int[] grid = new int[_gridCount*_gridCount*_gridCount];
        int y_s = _gridCount ;
        int z_s = _gridCount * _gridCount;
        Vector3 alignVector = Vector3.one * _gridSize / 2;
        foreach(Vector3 p0 in points)
        {
            Vector3 p = p0 + alignVector;
            int x = Mathf.FloorToInt(p.x / _gridSize * _gridCount);
            int y = Mathf.FloorToInt(p.y / _gridSize * _gridCount) * y_s;
            int z = Mathf.FloorToInt(p.z / _gridSize* _gridCount) * z_s;
            grid[x + y + z] = 1;
        }
        return grid;
    }

    

    public GameObject BuildGridObject(Vector3 position)
    {
        Vector3 scale = new Vector3(1f / _gridCount, 1f / _gridCount, 1f / _gridCount);
        GameObject structure = new GameObject();
        for (int z = 0; z<_gridCount; z++)
        {
            for(int y = 0; y < _gridCount; y++)
            {
                for(int x = 0; x<_gridCount; x++)
                {
                    int i = z * _gridCount * _gridCount + y * _gridCount + x;
                    if (_grid[i] > 0)
                    {
                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        Vector3 pointPosition = new Vector3(x - _gridSize / 2f, y - _gridSize / 2f, z - _gridSize);
                        c.transform.position = Vector3.Scale(pointPosition, scale);
                        c.transform.localScale = scale;
                        c.transform.parent = structure.transform;
                        c.name = z.ToString();
                    }
                  
                }
            }
        }
        structure.transform.position = position;

        return structure;
        
    }

    


}
