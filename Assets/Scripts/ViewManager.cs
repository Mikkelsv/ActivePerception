using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ViewManager {

    private int _viewLayers;

    private float _viewRadius;

    private Vector3[] _views;
    private float[][] _viewDistances;

    public float distanceTravelled = 0;
    private int _currentView = 0;
    private float[] _currentViewArray;
    private float[] _visitedViews;
    private int _viewCount;
    private bool _revisited = false;

    private GameObject _viewSphereObject;
    private Quaternion _sceneRotation;

    private int defaultView = 31;

	public ViewManager(int viewLayers, float viewRadius, int numberViews=100)
    {
        _viewLayers = viewLayers;
        _viewRadius = viewRadius;
        _views = GenerateViews(numberViews);
        _viewCount = _views.Length;
        _viewDistances = GenerateViewNeighbourhood();
        _sceneRotation = Quaternion.identity;
        Reset();
    }

    public Vector3 Reset(bool rotation = true, float rotationValue = -1f)
    {
        distanceTravelled = 0f;
        _visitedViews = new float[_viewCount];
        _currentViewArray = new float[_viewCount];
        _revisited = false;
        if(rotation && rotationValue > 0)
        {
            _sceneRotation = Quaternion.Euler(0, rotationValue, 0);
            _currentView = defaultView;
        }
        else if (rotation)
        {
            _sceneRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            _currentView = defaultView;
        }
        else
        {
            _sceneRotation = Quaternion.identity;
            _currentView = 0;
        }
        return SetView(defaultView);
    }

    public Vector3 SetView(int view)
    {
        _currentViewArray[_currentView] = 0f;
        distanceTravelled = GetDistance(_currentView, view);
        _currentView = view;
        _currentViewArray[_currentView] = 1f;
        if(_visitedViews[view] == 0f)
        {
            _visitedViews[view] = 1.0f;
            _revisited = false;
        }
        else
        {
            _revisited = true;
        }
        return _sceneRotation * _views[view];
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
        int nextView = (_currentView + increment)%_viewCount;
        return SetView(nextView);
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

    private Vector3[] GenerateViews(int numberViews)
    {
        HashSet<Vector3> viewsUnsorted = GenerateViewSphereAntipodally(numberViews);
        Vector3[] viewsSorted = SortViews(viewsUnsorted);
        Debug.Log(viewsSorted.Length.ToString() + " views created");
        return viewsSorted;
    }

    private HashSet<Vector3> GenerateViewSphereAntipodally(int numberViews)
    {

        /*
         * https://www.ncbi.nlm.nih.gov/pmc/articles/PMC3223966/
         */

        HashSet<Vector3> views = new HashSet<Vector3>();

        float pi = Mathf.PI;

        int K = numberViews;
        int n = (int)Math.Round(Math.Sqrt(K * pi / 8f));
        int ksum = 0;
        int ki;

        for (int i=1; i<=n; i++)
        {
            float theta = (float)((i - 0.5f) * pi / 2f / n);

            if (i == n)
            {
                ki = K - ksum;
            }
            else
            {
                ki = (int)Mathf.Round(2 * pi * Mathf.Sin(theta) / (Mathf.PI * (1f / Mathf.Sin(pi / (4 * n)))) * K);
                ksum += ki;
            }
          
            for(int j=1; j<=ki; j++)
            {
                float phi = (float)((j - 0.5f) * 2 * pi / (ki));

                float x = Mathf.Sin(theta) * Mathf.Cos(phi);
                float z = -Mathf.Sin(theta) * Mathf.Sin(phi);
                float y = Mathf.Cos(theta);
                Vector3 vector = new Vector3(x, y, z);
                vector.Normalize();
                Vector3 scaledVector = vector * this._viewRadius;
                views.Add(scaledVector);
            }
        }



        return views;
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

    internal IEnumerable<float> GetVisitedViews()
    {
        return _visitedViews;
    }

    internal float[] GetCurrentViews()
    {
        if(_currentViewArray.Sum() != 1) {
            throw new ArgumentOutOfRangeException();
        }
        return _currentViewArray;
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

    public bool GetRevisited()
    {
        return _revisited;
    }
}
