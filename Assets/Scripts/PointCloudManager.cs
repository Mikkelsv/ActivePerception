﻿using UnityEngine;
using System.Collections.Generic;

public class PointCloudManager
{
    private Vector3[] _viewportArray;

    public PointCloudManager(RenderTexture rTex)
    {
        _viewportArray = CreateViewPortArray(rTex);
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

    public HashSet<Vector3> CreatePointSet(Texture2D tex, Camera cam)
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

        HashSet<Vector3> pointSet = new HashSet<Vector3>();

        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        for (int j = 0; j < w; j++)
        {
            for (int i = 0; i < h; i++)
            {
                float depth = colors[i * h + j].r;


                float z = frustum_dist * (1f - depth);
                Vector3 p = rotMatrix.MultiplyVector(_viewportArray[i * h + j] * z) + cam.transform.position;

                if (depth > 0.6f)
                {
                    pointSet.Add(p);
                    min = Vector3.Min(p, min);
                    max = Vector3.Max(p, max);
                }
            }

        }
        Debug.Log(min);
        Debug.Log(max);
        return pointSet;
    }



    Matrix4x4 GetCameraRotationMatrix(Camera cam)
    {
        GameObject tempGO = new GameObject();
        Transform pointTranform = tempGO.transform;
        pointTranform.rotation = cam.transform.rotation;
        Matrix4x4 transMatrix = pointTranform.localToWorldMatrix;
        Object.Destroy(tempGO);
        return transMatrix;
    }
}