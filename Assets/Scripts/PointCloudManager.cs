using UnityEngine;
using System.Collections.Generic;
using System;

public class PointCloudManager
{
    private Vector3[] _viewportArray;

    private float _depthSawOff;

    private Camera _depthCamera;

    private GameObject _representationPrefab;

    private HashSet<Vector3> _pointCloud = new HashSet<Vector3>();

    private float _nearPlane;
    private float _farPlane;
    private float _viewFrustumDistance;

    public PointCloudManager(RenderTexture rTex, float depthSawOff, Camera depthCamera, GameObject representationPrefab, float studyGridSize)
    {
        float frustumAngle = depthCamera.fieldOfView / 2f;
        _viewportArray = CreateViewPortArray(rTex, studyGridSize, frustumAngle);
        _depthSawOff = depthSawOff;
        _depthCamera = depthCamera;
        _representationPrefab = representationPrefab;

        _nearPlane = _depthCamera.nearClipPlane;
        _farPlane = _depthCamera.farClipPlane;
        _viewFrustumDistance = _farPlane - _nearPlane;
    }

    private Vector3[] CreateViewPortArray(RenderTexture rTex, float studyGridSize, float frustumAngle)
    {
        float studyGridSizeHalved = studyGridSize / 2f;
        int h = rTex.height;
        int w = rTex.width;
        Vector3[] array = new Vector3[w * h];
        for (int j = 0; j < w; j++)
        {
            for (int i = 0; i < h; i++)
            {
                float y = Mathf.Tan((j * studyGridSize / w - studyGridSizeHalved) * frustumAngle * Mathf.Deg2Rad);
                float x = Mathf.Tan((i * studyGridSize / h - studyGridSizeHalved) * frustumAngle * Mathf.Deg2Rad);
                float z = 1.0f;
                array[j * h + i] = new Vector3(x, y, z);
            }
        }
        return array;
    }

    public HashSet<Vector3> CreatePointSet(Texture2D tex)
    {
        int w = tex.width;
        int h = tex.height;
      
        Color[] colors = tex.GetPixels();

        Matrix4x4 rotMatrix = GetCameraRotationMatrix(_depthCamera);
        Vector3 cameraOfsett = _depthCamera.transform.position;
        //cameraOfsett -= cameraOfsett.normalized * _depthCamera.nearClipPlane;

        HashSet<Vector3> pointSet = new HashSet<Vector3>();

        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        for (int j = 0; j < w; j++)
        {
            for (int i = 0; i < h; i++)
            {
                Color c = colors[j * h + i];
                float depth = c.r + c.g / 256f + c.b / 256f / 256f;

                float z = _viewFrustumDistance * (1f - depth) + _nearPlane;
                Vector3 p = rotMatrix.MultiplyVector(_viewportArray[j * h + i] * z) + cameraOfsett;

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

    public void BuildPointCloudObject(Vector3 position)
    {
        GameObject pointCloudObject = new GameObject();
        pointCloudObject.name = "PointCloudRepresentation";
        pointCloudObject.transform.position = position;
        pointCloudObject.transform.localScale = new Vector3(10f, 10f, 10f);

        foreach(Vector3 p in _pointCloud)
        {
            GameObject point = UnityEngine.Object.Instantiate(_representationPrefab, pointCloudObject.transform);
            point.transform.localPosition = p;
        }
    }

   

    private Matrix4x4 GetCameraRotationMatrix(Camera cam)
    {
        GameObject tempGO = new GameObject();
        Transform pointTranform = tempGO.transform;
        pointTranform.rotation = cam.transform.rotation;
        Matrix4x4 transMatrix = pointTranform.localToWorldMatrix;
        UnityEngine.Object.Destroy(tempGO);
        return transMatrix;
    }
}