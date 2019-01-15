using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DepthCamera : MonoBehaviour {
    
    [SerializeField]
    Camera cam;

    RenderTexture rTex;

    [SerializeField]
    Shader shader;

    Stopwatch timer;

    Vector3[] viewport_array;
    
    private void Start()
    {
        rTex = cam.targetTexture;
        timer = new Stopwatch();
        viewport_array = CreateViewPortArray(rTex);
        cam.SetReplacementShader(shader, null);

    }

    void Update()
    {
        
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);

        RenderTexture.active = cam.targetTexture;
        cam.Render();
        tex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        tex.Apply();

        if (Input.GetKeyDown(KeyCode.C))
        {
            timer.Reset();
            timer.Start();
            // Vector3[] points = CreatePointCloud(tex);
            HashSet<Vector3> pointSet = CreatePointSet(tex);
            timer.Stop();
            Mesh m = MeshCreator.GenerateMeshFromSet(pointSet, Vector3.zero, Vector3.zero, Color.green, 0.005f);
            //timer.Stop();
            UnityEngine.Debug.Log(timer.Elapsed);
            CreateGameObject(m);


        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            UnityEngine.Debug.Log(tex.GetPixel(256, 256));
        }

    }

    
    Vector3[] CreatePointCloud(Texture2D tex)
    {
        int w = tex.width;
        int h = tex.height;
        UnityEngine.Debug.Log(w);
        
        
        UnityEngine.Debug.Log(h);
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;
        float frustum_dist = far - near;
        Color[] colors = tex.GetPixels();

        Matrix4x4 rotMatrix = GetCameraRotationMatrix(cam);

        Vector3[] points = new Vector3[w * h];
           
        for (int j = 0; j < w; j++)
        {
            for (int i = 0; i < h; i++)
            {
                float depth = colors[i * h + j].r;
                if (depth == 0f)
                {
                    points[i * h + j] = Vector3.zero;
                }

                float z = frustum_dist * (1f - depth);
                Vector3 p = rotMatrix.MultiplyVector(viewport_array[i * h + j] * z) + transform.position;
                points[i * h + j] = p;
               

            }
        }


        return points;
    }

    HashSet<Vector3> CreatePointSet(Texture2D tex)
    {
        int w = tex.width;
        int h = tex.height;
        UnityEngine.Debug.Log(w);


        UnityEngine.Debug.Log(h);
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;
        float frustum_dist = far - near;
        Color[] colors = tex.GetPixels();

        Matrix4x4 rotMatrix = GetCameraRotationMatrix(cam);


        HashSet<Vector3> pointSet = new HashSet<Vector3>();

        for (int j = 0; j < w; j++)
        {
            for (int i = 0; i < h; i++)
            {
                float depth = colors[i * h + j].r;
            

                float z = frustum_dist * (1f - depth);
                Vector3 p = rotMatrix.MultiplyVector(viewport_array[i * h + j] * z) + transform.position;
            
                if (depth > 0.6f)
                {
                    pointSet.Add(p);
                }


            }
        }
        return pointSet;
    }




    Vector3[] CreateViewPortArray(RenderTexture rTex)
    {
        int h = rTex.height;
        int w = rTex.width;
        Vector3[] array = new Vector3[w * h];
        for (int j = 0; j < w; j++)
        {
            for (int i = 0; i < h; i++)
            {
                float x = Mathf.Tan((j * 2f / w - 1.0f) * 30f * Mathf.Deg2Rad);
                float y = Mathf.Tan((i * 2f / h - 1.0f) * 30f * Mathf.Deg2Rad);
                float z = 1.0f;
                array[i * h + j] = new Vector3(x, y, z);
            }
        }
        return array;
    }

    void CreateGameObject(Mesh m)
    {
        var go = new GameObject("Empty");

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshFilter>().mesh = m;
        go.transform.position = Vector3.zero;
        go.transform.position = new Vector3(5, 0, 0);


    }

    Matrix4x4 GetCameraRotationMatrix(Camera cam)
    {
        GameObject tempGO = new GameObject();
        Transform pointTranform = tempGO.transform;
        pointTranform.rotation = cam.transform.rotation;
        Matrix4x4 transMatrix = pointTranform.localToWorldMatrix;
        Destroy(tempGO);
        return transMatrix;
    }

}

