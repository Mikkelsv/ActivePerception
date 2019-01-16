using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccupancyGrid {
    
    public static int[] GenerateGrid(HashSet<Vector3> points, int g)
    {
        int[] grid = new int[g*g*g];
        int x_s = 1;
        int y_s = g ;
        int z_s = g * g;
        Vector3 alignVector = new Vector3(3f, 3f, 3f);
        foreach(Vector3 p0 in points)
        {
            Vector3 p = p0 + alignVector;
            int x = Mathf.FloorToInt(p.x /6f * g);
            int y = Mathf.FloorToInt(p.y /6f * g) * y_s;
            int z = Mathf.FloorToInt(p.z /6f * g) * z_s;
            grid[x + y + z] = 1;
        }
        return grid;
    }

    public static GameObject GenerateOccupancyStructure(int[] grid, int g)
    {
        Vector3 scale = new Vector3(1f / g, 1f / g, 1f / g);
        Vector3 scale2 = new Vector3(1 / 4f, 1 / 4f, 1 / 4f);
        Vector3 scale3 = new Vector3(6f / 32f, 6f/32f, 6f/32f);
        GameObject structure = new GameObject();
     
        for (int z = 0; z<g; z++)
        {
            for(int y = 0; y < g; y++)
            {
                for(int x = 0; x<g; x++)
                {
                    int i = z * g * g + y * g + x;
                    if (grid[i] == 1)
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
