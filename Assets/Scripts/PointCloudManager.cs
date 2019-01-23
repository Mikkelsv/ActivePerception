using UnityEngine;
using System.Collections.Generic;

public class PointCloudManager
{
    private Vector3[] _viewportArray;

    private float _depthSawOff;

    private Camera _depthCamera;

    private HashSet<Vector3> _pointCloud = new HashSet<Vector3>();

    public PointCloudManager(RenderTexture rTex, float depthSawOff, Camera depthCamera)
    {
        _viewportArray = CreateViewPortArray(rTex);
        _depthSawOff = depthSawOff;
        _depthCamera = depthCamera;
    }

    private Vector3[] CreateViewPortArray(RenderTexture rTex)
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

    public HashSet<Vector3> CreatePointSet(Texture2D tex)
    {
        int w = tex.width;
        int h = tex.height;
        Debug.Log(w);
        Debug.Log(h);
        float near = _depthCamera.nearClipPlane;
        float far = _depthCamera.farClipPlane;
        float frustum_dist = far - near;
        Color[] colors = tex.GetPixels();

        Matrix4x4 rotMatrix = GetCameraRotationMatrix(_depthCamera);

        HashSet<Vector3> pointSet = new HashSet<Vector3>();

        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        for (int j = 0; j < w; j++)
        {
            for (int i = 0; i < h; i++)
            {
                float depth = colors[i * h + j].r;


                float z = frustum_dist * (1f - depth);
                Vector3 p = rotMatrix.MultiplyVector(_viewportArray[i * h + j] * z) + _depthCamera.transform.position;

                if (depth > _depthSawOff)
                {
                    pointSet.Add(p);
                    min = Vector3.Min(p, min);
                    max = Vector3.Max(p, max);
                }
            }

        }
        Debug.Log(min.ToString("F4"));
        Debug.Log(max.ToString("F4"));
        return pointSet;
    }

    public void AddToCloud(HashSet<Vector3> points)
    {
        _pointCloud.UnionWith(points);
    }

    public HashSet<Vector3> GetPointCloud()
    {
        return _pointCloud;
    }



    private Matrix4x4 GetCameraRotationMatrix(Camera cam)
    {
        GameObject tempGO = new GameObject();
        Transform pointTranform = tempGO.transform;
        pointTranform.rotation = cam.transform.rotation;
        Matrix4x4 transMatrix = pointTranform.localToWorldMatrix;
        Object.Destroy(tempGO);
        return transMatrix;
    }


    /*
    public Vector3[] CreatePointCloud(Texture2D tex, Camera cam)
    {
        int w = tex.width;
        int h = tex.height;
        Debug.Log(w);
        Debug.Log(h);
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
                Vector3 p = rotMatrix.MultiplyVector(_viewportArray[i * h + j] * z) + cam.transform.position;
                points[i * h + j] = p;
            }
        }

        return points;
      
    }
      */
}