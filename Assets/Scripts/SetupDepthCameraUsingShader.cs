using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SetupDepthCameraUsingShader : MonoBehaviour {

    [SerializeField]
    Shader _depthShader;

    public void Awake()
    {
        this.GetComponent<Camera>().SetReplacementShader(_depthShader, null);
    }

}
