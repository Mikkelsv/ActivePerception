using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureScript : MonoBehaviour {

    [SerializeField]
    private GameObject _displayObject;

    [SerializeField]
    private RenderTexture _renderTexture;

 
    Camera depthCamera;

    private void Start()
    {
        depthCamera = GameObject.FindGameObjectWithTag("DepthCamera").GetComponent<Camera>();
    }

    void Update () {
        Texture2D tex = ToTexture2D(_renderTexture);
        //Texture2D tex = renderToTexture(depthCamera, 256, 256);
        Debug.Log(tex.GetPixel(128, 128));
	}


    Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);

        RenderTexture.active = depthCamera.targetTexture;
        depthCamera.depthTextureMode = DepthTextureMode.Depth;
        depthCamera.Render();
        tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        tex.Apply();
        return tex;
    }


    Texture2D RenderToTexture(Camera camera, int w, int h)
    {
        //var tempRt = new RenderTexture(w, h, 32, RenderTextureFormat.Depth);
        //var tempRt = new RenderTexture(w, h, 0);
        //camera.depthTextureMode = DepthTextureMode.Depth;

        RenderTexture target = new RenderTexture(w, h, 0, RenderTextureFormat.Default);
        RenderTexture tempRt = new RenderTexture(w, h, 24, RenderTextureFormat.Depth);
        camera.SetTargetBuffers(target.colorBuffer, tempRt.depthBuffer);
        //camera.targetTexture = tempRt;
        camera.Render();
        RenderTexture.active = tempRt;
        var tex2d = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex2d.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex2d.Apply();

        return tex2d;
    }

    Texture2D ToTexture2DFromMaterial(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false);

        return tex;
    }
    
 }
