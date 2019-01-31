using UnityEngine;

public class DepthRenderingManager
{
    private RenderTexture _renderTexture;

    private Camera _depthCamera;

    private Texture2D _currentRendering;

    private int _textureSixe;

    public DepthRenderingManager(Camera depthCamera, float nearClipPlane, float farClipPlane)
    {
        _depthCamera = depthCamera;
        _depthCamera.farClipPlane = farClipPlane;
        _depthCamera.nearClipPlane = nearClipPlane;
        _renderTexture = _depthCamera.targetTexture;
        _textureSixe = _renderTexture.height;
        _currentRendering = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGBAFloat, false);
      
    }

    public Texture2D GetDepthRendering()
    {

        RenderTexture.active = _depthCamera.targetTexture;
        _depthCamera.Render();
        _currentRendering.ReadPixels(new Rect(0, 0, _textureSixe, _textureSixe), 0, 0);
        
        _currentRendering.Apply();
        return _currentRendering;
    }
  
    public void SetCameraView(Vector3 view)
    {
        _depthCamera.transform.position = view;
        _depthCamera.transform.rotation = Quaternion.LookRotation(-view);
    }
}