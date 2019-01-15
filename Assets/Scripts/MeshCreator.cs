using UnityEngine;
using System.Collections.Generic;

public class MeshCreator{

    public static Mesh GenerateMesh(Vector3[] points, Vector3 origin, Vector3 orientation, Color color, float length)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[points.Length * 3];
        if(points.Length * 3 > 65000)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Debug.Log("Big Mesh Required");
        }

        int[] indices = new int[points.Length * 3];
        Color[] colors = new Color[points.Length * 3];
        int index;
        Vector3 rs = new Vector3(length/2f, -length, 0);
        Vector3 ls = new Vector3(-length/2f, -length, 0);

        for (int i=0; i<points.Length; i++)
        {
            Vector3 point = points[i];
            for(int j = 0; j<3; j++)
            {
                index = 3 * i + j;
                indices[index] = 3*i+j;
                colors[index] = color;
            }
            vertices[3 * i + 0] = point;
            vertices[3 * i + 1] = point + rs;
            vertices[3 * i + 2] = point + ls;
            
        }
        mesh.vertices = vertices;
        mesh.SetTriangles(indices,0);
        mesh.colors = colors;
        return mesh;
    }

    public static Mesh GenerateMeshFromSet(HashSet<Vector3> points, Vector3 origin, Vector3 orientation, Color color, float length)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[points.Count * 3];
        if (points.Count * 3 > 65000)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Debug.Log("Big Mesh Required");
        }

        int[] indices = new int[points.Count * 3];
        Color[] colors = new Color[points.Count * 3];
        int index;
        Vector3 rs = new Vector3(length / 2f, -length, 0);
        Vector3 ls = new Vector3(-length / 2f, -length, 0);

        int i = 0;
        foreach (Vector3 point in points)
        {

            for (int j = 0; j < 3; j++)
            {
                index = 3 * i + j;
                indices[index] = 3 * i + j;
                colors[index] = color;
            }
            vertices[3 * i + 0] = point;
            vertices[3 * i + 1] = point + rs;
            vertices[3 * i + 2] = point + ls;
            i++;

        }
        mesh.vertices = vertices;
        mesh.SetTriangles(indices, 0);
        mesh.colors = colors;
        return mesh;
    }
}
