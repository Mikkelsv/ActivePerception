using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SystemInterface {

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
    private float _sphereRadius = 1.8f;

    //Occupancy Grid Settings
    private int _occupancyGridCount = 32;
    private float _studyGridSize = 1.2f;
    private float requiredAccuracy = 0.99f;

    private Vector3 _gridPosition = new Vector3(8, 0, 0);


    private PointCloudManager _pcm;

    private OccupancyGridManager _ogm;
    private GroundTruthGenerator _gtg;
    private RewardManager _rm;
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
        _gtg = new GroundTruthGenerator(_drm, _vm, _pcm, _ogm, _som);
        _rm = new RewardManager(_gtg, _ogm, _som, _vm, requiredAccuracy);


        Reset();
    }

    public void Reset()
    {
        _ogm.ClearGrid();
        _som.PrepareRandomStudyObject();
        _vm.Reset();
        RenderView(0);
    }

    public void RenderView(int viewIndex)
    {
        Vector3 newView = _vm.SetView(viewIndex);
        _drm.SetCameraView(newView);
        _currentRendering = _drm.GetDepthRendering();
        HashSet<Vector3> pointCloud = _pcm.CreatePointSet(_currentRendering);
        _ogm.AddPoints(pointCloud);
    }

    public float GetScore()
    {
        //Compute score of last rendering
        return _rm.ComputeReward();
    }
    public bool GetDoneOnAccuracy()
    {
        return _rm.DetermineDone();
    }

    public float[] CollectObservations()
    {

        float[] distanceAndCount = new float[]
        {
            _vm.distanceTravelled,
            _rm.ComputeAccuracy()
        };

        float[] rewards = _rm.ComputeRewardArray();

        var obs = _ogm.GetGrid()
            .Concat(_vm.GetCurrentViews())
            .Concat(_vm.GetVisitedViews())
            .Concat(distanceAndCount)
            .Concat(rewards);

        return obs.ToArray();
    }
}
