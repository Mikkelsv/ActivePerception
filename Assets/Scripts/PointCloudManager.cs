using UnityEngine;
using System.Collections.Generic;
using System;

public class PointCloudManager
{
    private Vector3[] _viewportArray;

    //private float[] _angleArray;

    private float _depthSawOff;

    private Camera _depthCamera;

    private GameObject _representationPrefab;

    private HashSet<Vector3> _pointCloud = new HashSet<Vector3>();

    private float _farPlane;
    private float _viewFrustumDistance;

    public PointCloudManager(RenderTexture rTex, float depthSawOff, Camera depthCamera)
    {
        float frustumAngle = depthCamera.fieldOfView / 2f;
        _viewportArray = CreateViewPortArray(rTex, frustumAngle);
        //_angleArray = CreateViewPortAngles(rTex, frustumAngle);
        _depthSawOff = depthSawOff;
        _depthCamera = depthCamera;

        _farPlane = _depthCamera.farClipPlane;
        _viewFrustumDistance = _farPlane;

        _representationPrefab = (GameObject)Resources.Load("Prefabs/LowResSphere");
        Debug.Log(_representationPrefab.name + " used as point cloud representation object");
    }

    private Vector3[] CreateViewPortArray(RenderTexture rTex, float frustumAngle)
    {
        int h = rTex.height;
        int w = rTex.width;
        Vector3[] array = new Vector3[w * h];
        for (int j = 0; j < w; j++)
        {
            for (int i = 0; i < h; i++)
            {
                //float y = Mathf.Tan((j * 2f / w - 1f) * frustumAngle * Mathf.Deg2Rad);
                float y = (j * 2f / w - 1f) * Mathf.Tan(frustumAngle * Mathf.Deg2Rad);
                //float x = Mathf.Tan((i * 2f / h - 1f) * frustumAngle * Mathf.Deg2Rad);
                float x = (i * 2f / h - 1f) * Mathf.Tan(frustumAngle * Mathf.Deg2Rad);

                float z = 1.0f;
                array[j * h + i] = new Vector3(x, y, z);
            }
        }
        return array;
    }

    private Vector3[] CreateViewPortArrayUsingVectors(RenderTexture rTex, float frustumAngle)
    {
        int h = rTex.height;
        int w = rTex.width;
        Vector3[] array = new Vector3[w * h];
        for (int j = 0; j < w; j++)
        {
            for (int i = 0; i < h; i++)
            {
                float y = Mathf.Tan((j * 2f / w - 1f) * frustumAngle * Mathf.Deg2Rad);
                float x = Mathf.Tan((i * 2f / h - 1f) * frustumAngle * Mathf.Deg2Rad);
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

        Matrix4x4 cameraRotationMatrix = GetCameraRotationMatrix(_depthCamera);
        Vector3 cameraOfsett = _depthCamera.transform.position; //account for camera position
        //cameraOfsett -= cameraOfsett.normalized * _depthCamera.nearClipPlane; //account for nearplane

        HashSet<Vector3> pointSet = new HashSet<Vector3>();

        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        for (int j = 0; j < w; j++)
        {
            for (int i = 0; i < h; i++)
            {
                Color c = colors[j * h + i];
                float depth = c.r;

                float z = _viewFrustumDistance * (1f - depth);

                Vector3 p = cameraRotationMatrix.MultiplyVector(_viewportArray[j * h + i] * z) + cameraOfsett;

                if (depth > _depthSawOff)
                {
                    pointSet.Add(p);
                    min = Vector3.Min(p, min);
                    max = Vector3.Max(p, max);
                }
            }

        }
        //Debug.Log(min.ToString("F6"));
        //Debug.Log(max.ToString("F6"));
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

    public void BuildPointCloudObject(Vector3 position, Vector3 scale)
    {
        BuildPointCloudObjectFromCloud(position, _pointCloud, scale);
    }

    public void BuildPointCloudObjectFromCloud(Vector3 position, HashSet<Vector3> pointCloud, Vector3 scale)
    {
        GameObject pointCloudObject = new GameObject();
        pointCloudObject.name = "PointCloudRepresentation";
        pointCloudObject.transform.position = position;
        pointCloudObject.transform.localScale = scale;

        foreach (Vector3 p in pointCloud)
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

    //Not required
    //private float[] CreateViewPortAngles(RenderTexture rTex, float frustumAngle)
    //{
    //    int h = rTex.height;
    //    int w = rTex.width;
    //    float[] angleArray = new float[w * h];
    //    for (int j = 0; j < w; j++)
    //    {
    //        for (int i = 0; i < h; i++)
    //        {
    //            float y = Mathf.Tan((j * 2f / w - 1f) * frustumAngle * Mathf.Deg2Rad);
    //            float x = Mathf.Tan((i * 2f / h - 1f) * frustumAngle * Mathf.Deg2Rad);
    //            Vector3 direction = new Vector3(x, y, 1.0f);
    //            angleArray[j * h + i] = Vector3.Angle(direction, Vector3.forward) * Mathf.Deg2Rad;
    //        }
    //    }
    //    return angleArray;
    //}
}