using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraMode : MonoBehaviour {

    public Camera _cam;

    public float DepthLevel = 1.0F;

    void Start()
    {
        _cam.depthTextureMode |= DepthTextureMode.Depth;
    }

 
}
