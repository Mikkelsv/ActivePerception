using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

public class MainController : MonoBehaviour
{

    [SerializeField]
    private Camera _depthCamera;

    [SerializeField]
    private int _setView;

    [SerializeField]
    private int _compareViewWith;

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

    private Vector3 _gridPosition = new Vector3(8, 0, 0);

    //Mesh Creatonr
    private Vector3 _meshPosition = new Vector3(0,0,0);
    private Vector3 _pointCloudScale = new Vector3(1, 1, 1);
    

    
    private Stopwatch _timer;

    private PointCloudManager _pcm;
    private OccupancyGridManager _ogm;
    private DepthRenderingManager _drm;
    private StudyObjectMamanger _som;
    private ViewManager _vm;
    private GroundTruthGenerator _gtg;
    private RewardManager _rm;

    private List<Vector3> _views;


    private void Start()
    {
        _timer = new Stopwatch();


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
        _rm = new RewardManager(_gtg, _ogm, _som, _vm);

        Vector3 v = _vm.GetView(0);
        _drm.SetCameraView(v);
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            RenderView();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            AddView();
        } 

        if(Input.GetKeyDown(KeyCode.V))
        {
            BuildViewSphere();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            Vector3 v = _vm.SetNeighbouringView();
            _drm.SetCameraView(v);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Vector3 v = _vm.SetNeighbouringView(10);
            _drm.SetCameraView(v);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            _pcm.BuildPointCloudObject(_meshPosition, _pointCloudScale);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            _som.PrepareNextStudyObject();
            _ogm.ClearGrid();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            float[][] grids = _gtg.Grids();
            _ogm.BuildGrid(grids[_som.CurrentObject()]);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Vector3 v = _vm.SetView(_setView);
            _drm.SetCameraView(v);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            UnityEngine.Debug.Log(_vm.GetDistance(_vm.GetCurrentViewIndex(), _compareViewWith).ToString());
        }
    }

    private void RenderView()
    {
        Texture2D tex = _drm.GetDepthRendering();
        HashSet<Vector3> pointCloud = _pcm.CreatePointSet(tex);
        _pcm.BuildPointCloudObjectFromCloud(_meshPosition, pointCloud, _pointCloudScale);
    }

    private void AddView()
    {
        _timer.Reset();
        _timer.Start();
        Texture2D tex = _drm.GetDepthRendering();
        HashSet<Vector3> pointSet = _pcm.CreatePointSet(tex);
        UnityEngine.Debug.Log("Creating point set -" + _timer.Elapsed);
        _pcm.AddToCloud(pointSet);
        UnityEngine.Debug.Log("Add To Cloud -" + _timer.Elapsed);
        _ogm.AddPoints(pointSet);
        UnityEngine.Debug.Log("Add to occupancy grid - " + _timer.Elapsed);
        _ogm.UpdateGridObject();
        UnityEngine.Debug.Log("Update Grid - " + _timer.Elapsed);
        UnityEngine.Debug.Log(_rm.ComputeGlobalIncreasedAccuracy().ToString());
        UnityEngine.Debug.Log(_rm.ComputeDistanceReward().ToString());
        _timer.Stop();
    }


    private void CreateGameObject(Mesh m)
    {
        var go = new GameObject("Empty");

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshFilter>().mesh = m;
        go.transform.position = _meshPosition;
    }

    private void BuildViewSphere()
    {
         _vm.BuildSphere(Vector3.zero);
    }

    private float GetMaxElement(Vector3 v)
    {
        return Mathf.Max(v.x,v.y,v.z);
    }
}
