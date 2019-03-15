using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ViewManager{

    private int _viewLayers;

    private float _viewRadius;

    private Vector3[] _views;
    private float[][] _viewDistances;

    public float distanceTravelled = 0;
    private int _currentView = 0;
    private int _viewCount;

    private GameObject _viewSphereObject;

	public ViewManager(int viewLayers, float viewRadius)
    {
        _viewLayers = viewLayers;
        _viewRadius = viewRadius;
        _views = GenerateViews();
        _viewCount = _views.Length;
        _viewDistances = GenerateViewNeighbourhood();
    }

    public Vector3 SetView(int view)
    {
        distanceTravelled = GetDistance(_currentView, view);
        _currentView = view;
        return _views[view];
    }

    public int GetCurrentViewIndex()
    {
        return _currentView;
    }

    public Vector3 GetView(int view)
    {
        return _views[view];
    }

    public Vector3 SetNeighbouringView(int increment = 1)
    {
        _currentView = (_currentView + increment)%_viewCount;
        return _views[_currentView];
    }

    public int Count()
    {
        return _viewCount;
    }

    public void BuildSphere(Vector3 position)
    {
        float s = 0.05f;
        _viewSphereObject = new GameObject();
        _viewSphereObject.transform.position = position;
        Vector3 scale = new Vector3(s, s, s);
        int i = 0;
        foreach (Vector3 view in _views)
        {

            GameObject v = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            v.transform.parent = _viewSphereObject.transform;
            v.transform.localPosition = view;
            v.transform.localScale = scale;
            v.name = "Sphere_" + i.ToString();
            i++;
        }
    }

    private Vector3[] GenerateViews()
    {
        HashSet<Vector3> viewsUnsorted = GenerateViewSphere();
        Vector3[] viewsSorted = SortViews(viewsUnsorted);
        Debug.Log(viewsSorted.Length.ToString() + " views created");
        return viewsSorted;
    }

    private HashSet<Vector3> GenerateViewSphere()
    {
        /*
        * Generates View Sphere using Lambert's projection
        * Based on paper:
        * D Roşca. “New uniform grids on the sphere”. In: Astronomy & Astrophysics 520 (2010), A63.
        */
        int g = _viewLayers;
        float pi = Mathf.PI;
        float r = _viewRadius;
        float a, b;

        HashSet<Vector3> views = new HashSet<Vector3>();

        for (int B = -2 * g; B <= 2 * g; B++)
        {
            for (int A = -2 * g; A <= 2 * g; A++)
            {
                a = r / g * A;
                b = r / g * B;
                if (Mathf.Abs(b) <= Mathf.Abs(a))
                {
                    float abJoint = 2 * a / pi * Mathf.Sqrt(pi - a * a / (r * r));
                    float x = abJoint * Mathf.Cos(b * pi / (4 * a));
                    float y = abJoint * Mathf.Sin(b * pi / (4 * a));
                    float z = 2 * a * a / (pi * r) - r;
                    if (!float.IsNaN(x) && z<0)
                    {
                        views.Add(new Vector3(x, -z, y));
                        //if (z <= -0.01f)
                        //{
                            

                        //}
                        //else if (z <= 0f)
                        //{
                        //    views.Add(new Vector3(x, 0, z));
                        //}
                    }
                }
                if (Mathf.Abs(a) <= Mathf.Abs(b))
                {
                    float abJoint = 2 * b / pi * Mathf.Sqrt(pi - b * b / (r * r));

                    float x = abJoint * Mathf.Sin(a * pi / (4 * b));
                    float y = abJoint * Mathf.Cos(a * pi / (4 * b));
                    float z = 2 * b * b / (pi * r) - r;
                    if (!float.IsNaN(x) && z<0)
                    {
                        views.Add(new Vector3(x, -z, y));
                        //if (z <= -0.01f)
                        //{
                            

                        //}
                        //else if (z <= 0f)
                        //{
                        //    views.Add(new Vector3(x, 0, y));
                        //}
                    }

                }
            }
        }
        views.Add(new Vector3(0, r, 0));
        return views;
    }

    private Vector3[] SortViews(HashSet<Vector3> views)
    {
        List<Vector3> sortedViews = views.ToList();
        sortedViews = sortedViews.OrderBy(v => -v.y).ThenBy(v => v.x).ThenBy(v => -v.z).ToList();
        return sortedViews.ToArray();
    }

    private float[][] GenerateViewNeighbourhood()
    {
        float[][] distances = new float[_viewCount][];
        for(int v1 = 0; v1<_viewCount; v1++)
        {
            float[] v1Distances = new float[_viewCount];
            Vector3 vector1 = _views[v1];
            for (int v2 = 0; v2 < _viewCount; v2++)
            {
                float d = Vector3.Angle(vector1, _views[v2]);
                v1Distances[v2] = d;
            }
            distances[v1] = v1Distances;
        }
        return distances;
    }

    public float GetDistance(int from, int to)
    {
        return _viewDistances[from][to];
    }
}
