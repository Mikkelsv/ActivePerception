using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SystemInterface{
 
    private GameObject _depthCameraObject;

    GameObject _pointCloudVisualizer;

    private Camera _depthCamera;

    private Shader _depthShader;

    private GameObject _studyObject;
    private GameObject[] _studyObjects;


   
    private Vector3 _objectPosition = new Vector3(0, 0, 0);

    //Depth Camera Settings
    private float _nearClipPlane = 0.2f;
    private float _farClipPlane = 5f;
    private float _depthSawOff = 0.01f;
    private int _textureResolution = 128;

    //View Sphere Settings
    private int _viewGridLayers = 4;
    private float _sphereRadius = 2f;

    //Occupancy Grid Settings
    private int _occupancyGridCount = 32;
    private float _studyGridSize = 1.2f;

    private Vector3 _gridPosition = new Vector3(8, 0, 0);

    //private Vector3 _referenceGridPosition = new Vector3(14, 0, 0);

    //Mesh Creatonr
    private Vector3 _meshPosition = new Vector3(0, 0, 0);
    private Vector3 _pointCloudScale = new Vector3(1, 1, 1);

    
    private PointCloudManager _pcm;

    private HashSet<Vector3> _pc = new HashSet<Vector3>();

    private OccupancyGridManager _ogm;

    private DepthRenderingManager _drm;

    private StudyObjectMamanger _som;

    private ViewManager _vm;

    private Texture2D _currentRendering;

    public SystemInterface(Camera depthCamera)
    {
        //Prepare depth rendering texture
        _depthCamera = depthCamera;
        RenderTexture rTex = _depthCamera.targetTexture;
        rTex.width = _textureResolution;
        rTex.height = _textureResolution;

        //Setup Managers
        _vm = new ViewManager(_viewGridLayers, _sphereRadius);
        _som = new StudyObjectMamanger(_objectPosition);
        _drm = new DepthRenderingManager(_depthCamera, _nearClipPlane, _farClipPlane);
        _pcm = new PointCloudManager(rTex, _depthSawOff, _depthCamera);
        _ogm = new OccupancyGridManager(_occupancyGridCount, _studyGridSize, _gridPosition);
    }

    public void Reset()
    {
        _ogm.ClearGrid();
        Vector3 newView =_vm.SetView(0);
        _som.PrepareNewStudyObject();
        _drm.SetCameraView(newView);
        
    }

    public void RenderView(int viewIndex)
    {
        Vector3 newView = _vm.SetView(viewIndex);
        _drm.SetCameraView(newView);
        _currentRendering = _drm.GetDepthRendering();
        HashSet<Vector3> pointCloud = _pcm.CreatePointSet(_currentRendering);
        _ogm.AddPoints(pointCloud);
    }

    public float[] GetOngoingOccupancyGrid()
    {
        return _ogm.GetGrid();
    }

    public float GetScore()
    {
        //Compute score of last rendering
        return -1f;
    }
}
