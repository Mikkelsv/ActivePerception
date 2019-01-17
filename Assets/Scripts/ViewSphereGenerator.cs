using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ViewSphereGenerator
{
    /*
     * Generates View Sphere using Lambert's projection
     * Based on paper:
     * D Roşca. “New uniform grids on the sphere”. In: Astronomy & Astrophysics 520 (2010), A63.
     */

    public static HashSet<Vector3> GenerateViewSphere(int gridNumber=3, float radius=1)
    {
        int g = gridNumber;
        float pi = Mathf.PI;
        float r = radius;
        float a, b;
    
        
        HashSet<Vector3> views = new HashSet<Vector3>();

        for(int B=-2*g; B <= 2*g; B++)
        {
            for(int A=-2*g; A<=2*g; A++)
            {
                a = r / g * A;
                b = r / g * B;
                if (Mathf.Abs(b) <= Mathf.Abs(a))
                {
                    float abJoint = 2 * a / pi * Mathf.Sqrt(pi - a * a / (r * r));
                    float x = abJoint * Mathf.Cos(b * pi / (4 * a));
                    float y = abJoint * Mathf.Sin(b * pi / (4 * a));
                    float z = 2 * a * a / (pi * r) - r;
                    if (!float.IsNaN(x))
                    {
                        if (z <= -0.01f)
                        {
                            views.Add(new Vector3(x, y, z));
                            views.Add(new Vector3(x, y, -z));
                        }
                        else if (z <= 0f)
                        {
                            views.Add(new Vector3(x, y, 0));
                        }
                    }
                }
                if (Mathf.Abs(a) <= Mathf.Abs(b))
                {
                    float abJoint = 2 * b / pi * Mathf.Sqrt(pi - b * b / (r * r));
                
                    float x = abJoint * Mathf.Sin(a * pi / (4 * b));
                    float y = abJoint * Mathf.Cos(a * pi / (4 * b));
                    float z = 2 * b * b / (pi * r) - r;
                    if (!float.IsNaN(x))
                    {
                        if (z <= -0.01f)
                        {
                            views.Add(new Vector3(x, y, z));
                            views.Add(new Vector3(x, y, -z));
                        }
                        else if (z <= 0f)
                        {
                            views.Add(new Vector3(x, y, 0));
                        }
                    }

                }
            }
        }
        views.Add(new Vector3(0, 0, r));
        views.Add(new Vector3(0, 0, -r));
        Debug.Log(views.Count);
        return views;
    }

    public static GameObject BuildSphere(HashSet<Vector3> views)
    {
        float s = 0.05f;
        GameObject viewSphere = new GameObject();
        viewSphere.transform.position = new Vector3(-2, 0, 0);
        Vector3 scale = new Vector3(s, s, s);
        foreach(Vector3 view in views)
        {
            
            GameObject v = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            v.transform.parent = viewSphere.transform;
            v.transform.localPosition = view;
            v.transform.localScale = scale;
            
          
        }
        return viewSphere;
    }

    public static List<Vector3> SortViews(HashSet<Vector3> views)
    {
        List<Vector3> sortedViews = views.ToList();
        sortedViews = sortedViews.OrderBy(v => v.x).ThenBy(v => v.y).ThenBy(v => v.z).ToList();
        return sortedViews;

    }
}