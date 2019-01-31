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
    GameObject _prefabStudyObject;

    [SerializeField]
    GameObject _pointCloudVisualizer;

    private GameObject _studyObject;

    private float _objectSize = 2f;
    private Vector3 _objectPosition = new Vector3(0, -1f, 0);

    //Depth Camera Settings
    private float _nearClipPlane = 0.4f;
    private float _farClipPlane = 10f;
    private float _depthSawOff = 0.4f;
    private int _textureResolution = 128;
    private float _studyGridSize = 2f;


    //View Sphere Settings
    private int _viewGridLayers = 6;
    private float _sphereRadius = 2f;   

    //Occupancy Grid Settings
    private int _occupancyGridCount = 16;
    
    private Vector3 _gridPosition = new Vector3(12, 0, 0);

    private Vector3 _referenceGridPosition = new Vector3(14, 0, 0);
    //Mesh Creatonr
    private Vector3 _meshPosition = new Vector3(1,0,0);
    private Vector3 _pointCloudScale = new Vector3(1, 1, 1);
    

    
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
        _pcm = new PointCloudManager(rTex, _depthSawOff, _depthCamera, _pointCloudVisualizer, _studyGridSize);
        _ogm = new OccupancyGridManager(_occupancyGridCount, _studyGridSize, _gridPosition);
        SetupScene(_prefabStudyObject);
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
            _pcm.BuildPointCloudObject(_meshPosition, _pointCloudScale);
        }

    }

    private void SetupScene(GameObject prefabObject)
    {
        //Setup object
        _studyObject = Instantiate(prefabObject);
        Vector3 boundaries = _studyObject.GetComponent<MeshFilter>().mesh.bounds.size;

        //_studyObject.transform.localScale = Vector3.one / (boundaries.) * _objectSize;
        _studyObject.transform.localScale = Vector3.one / GetMaxElement(boundaries);
        _studyObject.transform.position = _objectPosition;
        UnityEngine.Debug.Log("Object size:" + _studyObject.GetComponent<MeshFilter>().mesh.bounds.size);

        //Setup views
        _views = ViewSphereGenerator.GenerateViews(_viewGridLayers, _sphereRadius);
        _drm.SetCameraView(_views[_viewIndex]);

        _ogm.GenerateExampleGrid(_referenceGridPosition);
    }

    private void RenderView(Texture2D tex)
    {

        HashSet<Vector3> pointCloud = _pcm.CreatePointSet(tex);
        _pcm.BuildPointCloudObjectFromCloud(_meshPosition, pointCloud, _pointCloudScale);
        // Mesh m = MeshCreator.GenerateMeshFromSet(_pcm.GetPointCloud(), Vector3.zero, Vector3.zero, Color.green, 0.005f);
        //CreateGameObject(m);
        
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

    private float GetMaxElement(Vector3 v)
    {
        return Mathf.Max(v.x,v.y,v.z);
    }

}
