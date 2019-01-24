using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

public class MainController : MonoBehaviour
{

    [SerializeField]
    private Camera _depthCamera;

    [SerializeField]
    Shader _shader;

    [SerializeField]
    GameObject _studyObject;

    [SerializeField]
    GameObject _pointCloudVisualizer;


    //Depth Camera Settings
    private float _nearClipPlane = 0.4f;
    private float _farClipPlane = 2f;
    private float _depthSawOff = 0.01f;
    private int _textureResolution = 256;

  
    //View Sphere Settings
    private int _viewGridLayers = 6;
    private float _sphereRadius = 2.0f;   

    //Occupancy Grid Settings
    private int _occupancyGridCount = 64;
    private float _gridSize = 1f;
    private Vector3 _gridPosition = new Vector3(12, 0, 0);

    //Mesh Creatonr
    private Vector3 _meshPosition = new Vector3(13,0,0);
    

    
    private Stopwatch _timer;

    private PointCloudManager _pcm;

    private HashSet<Vector3> _pc = new HashSet<Vector3>();

    private OccupancyGridManager _ogm;

    private DepthRenderingManager _drm;

    private List<Vector3> _views;

    private int _viewIndex = 0;

    private void Start()
    {
        RenderTexture rTex = _depthCamera.targetTexture;
        rTex.width = _textureResolution;
        rTex.height = _textureResolution;
        _depthCamera.SetReplacementShader(_shader, null);
        _timer = new Stopwatch();
        _drm = new DepthRenderingManager(_depthCamera, _nearClipPlane, _farClipPlane);
        _pcm = new PointCloudManager(rTex, _depthSawOff, _depthCamera, _pointCloudVisualizer);
        _ogm = new OccupancyGridManager(_occupancyGridCount, _gridSize, _gridPosition);
        SetupScene();
      
    }

    void Update()
    {
        
        Texture2D tex = _drm.GetDepthRendering();

        if (Input.GetKeyDown(KeyCode.C))
        {
            RenderView(tex);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            AddView(tex);
        }
        
        if (Input.GetKeyDown(KeyCode.X))
        {
            UnityEngine.Debug.Log(tex.GetPixel(256, 256));
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            BuildViewSphere();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            _viewIndex = (_viewIndex + 1) % _views.Count;
            _drm.SetCameraView(_views[_viewIndex]);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            _viewIndex = (_viewIndex + 10) % _views.Count;
            _drm.SetCameraView(_views[_viewIndex]);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            _pcm.BuildPointCloudObject(_meshPosition);
        }

    }

    private void SetupScene()
    {
        //Setup object
        Vector3 boundaries = _studyObject.GetComponent<MeshFilter>().mesh.bounds.size;
        _studyObject.transform.localScale = Vector3.one / (boundaries.magnitude);
        _studyObject.transform.position = Vector3.zero;
        UnityEngine.Debug.Log("Object size:" + _studyObject.GetComponent<MeshFilter>().mesh.bounds.size);

        //Setup views
        _views = ViewSphereGenerator.GenerateViews(_viewGridLayers, _sphereRadius);
        _drm.SetCameraView(_views[_viewIndex]);
    }

    private void RenderView(Texture2D tex)
    {

        Mesh m = MeshCreator.GenerateMeshFromSet(_pcm.GetPointCloud(), Vector3.zero, Vector3.zero, Color.green, 0.005f);
    
        CreateGameObject(m);
        
    }

    private void AddView(Texture2D tex)
    {
        _timer.Reset();
        _timer.Start();
        HashSet<Vector3> pointSet = _pcm.CreatePointSet(tex);
        UnityEngine.Debug.Log("Creating point set -" + _timer.Elapsed);
        _pcm.AddToCloud(pointSet);
        UnityEngine.Debug.Log("Add To Cloud -" + _timer.Elapsed);
        _ogm.AddPoints(pointSet);
        UnityEngine.Debug.Log("Add to occupancy grid - " + _timer.Elapsed);
        _ogm.UpdateGridObject();
        UnityEngine.Debug.Log("Update Grid - " + _timer.Elapsed);
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
         ViewSphereGenerator.BuildSphere(_views, Vector3.zero);
    }

}
