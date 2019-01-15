using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ViewSphereScript : MonoBehaviour
{
    [SerializeField]
    GameObject sphere;

    // Use this for initialization
    void Start()
    {

        HashSet<Vector3> views = GenerateViewsTetrahedron(2f, 4);
        foreach (Vector3 v in views)
        {
            GameObject viewPoint = GameObject.Instantiate(sphere);
            viewPoint.transform.SetParent(this.transform);
            viewPoint.transform.position = this.transform.position + v;
            Vector3 w = v.normalized * 128;
            Color c = new Vector4(128f, 128f, 128f, 256f) + new Vector4(w.x, w.y, w.z, 0);
            viewPoint.GetComponent<MeshRenderer>().material.color = c/256;

            Debug.Log("Generating Spheres");
        }

        this.gameObject.AddComponent<MeshFilter>().mesh = TetraMesh();
        Debug.Log("Mesh added");
 

    }

    private Mesh TetraMesh()
    {
        float t = 2f * Mathf.Cos(Mathf.PI / 5);
        float d = Mathf.Sqrt(1 + t * t);
        Vector3[] vec =
       {
            new Vector3(0,t,1), //0 top n
            new Vector3(0,-t,1), //1 bottom n
            new Vector3(0,t,-1), //2 top s
            new Vector3(0,-t,-1), //3 bottom s
            new Vector3(1,0,t), //4 mid level ne
            new Vector3(-1,0,t),//5 mid level nw
            new Vector3(1,0,-t), //6 mid level se
            new Vector3(-1,0,-t), //7 mid level sw
            new Vector3(t,1,0), //8 2nd top e
            new Vector3(-t,1,0), //9 2nd top w
            new Vector3(t,-1,0), //10 2nd lowest e
            new Vector3(-t,-1,0) //11 2nd lowest w
        };
        int[] indices =
        {
            2,0,8,
            0,2,9,
            1,3,10,
            3,1,11,
            6,2,8,
            0,4,8,
            9,5,0,
            4,0,5,
            2,6,7,
            2,7,9,
            9,7,11,
            9,11,5,
            8,10,6,
            8,4,10,
            7,6,3,


        };
        
        for (int i = 0; i < vec.Length; i++)
        {
            vec[i] = vec[i] * d;
        }

        Mesh m = new Mesh
        {
            vertices = vec,
            triangles = indices
        };
        

       
    

        return m;
    }

    private void GenerateSpheres(int longitude, int latitude, float radius)
    {
        float longitudeRatio = 360f / longitude;
        float latitudeRatio = 360f / latitude;

        Vector3 vec = new Vector3(radius, 0, 0);

        for (int i = 0; i < latitude; i++)
        {
            for (int j = 0; j < longitude; j++)
            {
                GameObject viewPoint = GameObject.Instantiate(sphere);
                viewPoint.transform.SetParent(this.transform);
                Vector3 newPosition = Quaternion.Euler(0, latitudeRatio * i, longitudeRatio * j) * vec;
                viewPoint.transform.position = this.transform.position + newPosition;
                Debug.Log("Generating Spheres");
            }
        }
    }

    private HashSet<Vector3> GenerateViews(float r, int n)
    {
        float L = r * Mathf.Sqrt(Mathf.PI / 2f);

        HashSet<Vector3> views = new HashSet<Vector3>();

        float a, aTemp, b, bTemp;
        float x, y, z;
        for (int i = -n; i <= n; i++)
        {
            for (int j = -n; j <= n; j++)
            {
                aTemp = L * i / n;
                bTemp = L * j / n;
                if (Math.Abs(aTemp) >= Math.Abs(bTemp))
                {
                    a = aTemp; b = bTemp;
                }
                else
                {
                    a = bTemp; b = aTemp;
                }
                float expression = 2 * a / Mathf.PI * Mathf.Sqrt(Mathf.PI - a * a / r);
                x = expression * Mathf.Cos(b * Mathf.PI / (4 * a));
                y = expression * Mathf.Sin(b * Mathf.PI / (4 * a));
                z = 2 * a * a / (Mathf.PI * r) - r;
                views.Add(new Vector3(x, y, z));

            }
        }
        return views;

    }
    private HashSet<Vector3> GenerateViewsTetrahedron(float r, int n)
    {
        float t = 2f * Mathf.Cos(Mathf.PI / 5);
        float d = Mathf.Sqrt(1 + t * t);
        HashSet<Vector3> views = new HashSet<Vector3>();
        Vector3[] vec =
        {
            new Vector3(0,t,1),
            new Vector3(0,-t,1),
            new Vector3(0,t,-1),
            new Vector3(0,-t,-1),
            new Vector3(1,0,t),
            new Vector3(-1,0,t),
            new Vector3(1,0,-t),
            new Vector3(-1,0,-t),
            new Vector3(t,1,0),
            new Vector3(-t,1,0),
            new Vector3(t,-1,0),
            new Vector3(-t,-1,0)
        };
        foreach (Vector3 v in vec){
            views.Add(v * d);
        }
        
        return views;
    }
}
	
