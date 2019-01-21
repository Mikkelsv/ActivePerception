using UnityEngine;

public class DepthRenderingManager
{
    private RenderTexture _renderTexture;

    private Camera _depthCamera;

    private Texture2D _currentRendering;

    public DepthRenderingManager(Camera depthCamera, float nearClipPlane, float farClipPlane)
    {
        _depthCamera = depthCamera;
        _depthCamera.farClipPlane = farClipPlane;
        _depthCamera.nearClipPlane = nearClipPlane;
        _renderTexture = _depthCamera.targetTexture;
        _currentRendering = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGB24, false);
      
    }

    public Texture2D GetDepthRendering()
    {

        RenderTexture.active = _depthCamera.targetTexture;
        _depthCamera.Render();
        _currentRendering.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        _currentRendering.Apply();
        return _currentRendering;
    }

    public void SetCameraView(Vector3 view)
    {
        _depthCamera.transform.position = view;
        _depthCamera.transform.rotation = Quaternion.LookRotation(-view);
    }
}