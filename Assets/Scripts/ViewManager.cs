using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ViewManager{

    private int _viewLayers;

    private float _viewRadius;

    private List<Vector3> _views;

    private int _currentView;

	public ViewManager(int viewLayers, float viewRadius)
    {
        _viewLayers = viewLayers;
        _viewRadius = viewRadius;
        _views = GenerateViews();
        _currentView = 0;
    }

    public Vector3 SetView(int view)
    {
        _currentView = view;
        return _views[view];
    }

    public Vector3 GetView(int view)
    {
        return _views[view];
    }

    private List<Vector3> GenerateViews()
    {
        HashSet<Vector3> viewsUnsorted = GenerateViewSphere();
        List<Vector3> viewsSorted = SortViews(viewsUnsorted);
        Debug.Log(viewsSorted.Count.ToString() + " views created");
        return viewsSorted;
    }

    private HashSet<Vector3> GenerateViewSphere()
    {
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
                    if (!float.IsNaN(x))
                    {
                        if (z <= -0.01f)
                        {
                            views.Add(new Vector3(x, -z, y));

                        }
                        else if (z <= 0f)
                        {
                            views.Add(new Vector3(x, 0, z));
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
                            views.Add(new Vector3(x, -z, y));

                        }
                        else if (z <= 0f)
                        {
                            views.Add(new Vector3(x, 0, y));
                        }
                    }

                }
            }
        }
        views.Add(new Vector3(0, r, 0));
        return views;
    }

    private List<Vector3> SortViews(HashSet<Vector3> views)
    {
        List<Vector3> sortedViews = views.ToList();
        sortedViews = sortedViews.OrderBy(v => -v.y).ThenBy(v => v.x).ThenBy(v => -v.z).ToList();
        return sortedViews;
    }
}
