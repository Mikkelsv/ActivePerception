using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OccupancyGridManager {

    private int _gridSize;
    private int[] _grid;

    public OccupancyGridManager(int g)
    {
        _gridSize = g;
        _grid = new int[g * g * g];
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
        int[] grid = new int[_gridSize*_gridSize*_gridSize];
        int x_s = 1;
        int y_s = _gridSize ;
        int z_s = _gridSize * _gridSize;
        Vector3 alignVector = new Vector3(3f, 3f, 3f);
        foreach(Vector3 p0 in points)
        {
            Vector3 p = p0 + alignVector;
            int x = Mathf.FloorToInt(p.x /6f * _gridSize);
            int y = Mathf.FloorToInt(p.y /6f * _gridSize) * y_s;
            int z = Mathf.FloorToInt(p.z /6f * _gridSize) * z_s;
            grid[x + y + z] = 1;
        }
        return grid;
    }

    

    public GameObject BuildGridObject()
    {
        Vector3 scale = new Vector3(1f / _gridSize, 1f / _gridSize, 1f / _gridSize);
        Vector3 scale2 = new Vector3(1 / 4f, 1 / 4f, 1 / 4f);
        Vector3 scale3 = new Vector3(6f / 32f, 6f/32f, 6f/32f);
        GameObject structure = new GameObject();
     
        for (int z = 0; z<_gridSize; z++)
        {
            for(int y = 0; y < _gridSize; y++)
            {
                for(int x = 0; x<_gridSize; x++)
                {
                    int i = z * _gridSize * _gridSize + y * _gridSize + x;
                    if (_grid[i] > 0)
                    {
                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        c.transform.position = Vector3.Scale(new Vector3(x, y, z), scale3);
                        c.transform.localScale = scale3;
                        c.transform.parent = structure.transform;
                        c.name = z.ToString();
                    }
                  
                }
            }
        }

        return structure;
        
    }

    


}
