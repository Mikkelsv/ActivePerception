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


    //Depth Camera Settings
    private float _nearClipPlane = 0.3f;
    private float _farClipPlane = 10f;
    private float _depthSawOff = 0.75f;

    //View Grid Settings
    private int _gridSize = 5;
    private float _sphereRadius = 1.5f;





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
        _depthCamera.SetReplacementShader(_shader, null);
        _timer = new Stopwatch();
        _drm = new DepthRenderingManager(_depthCamera, _nearClipPlane, _farClipPlane);
        _pcm = new PointCloudManager(rTex, _depthSawOff, _depthCamera);
        _ogm = new OccupancyGridManager(32);
        
        Vector3 boundaries = _studyObject.GetComponent<MeshFilter>().mesh.bounds.size;
        UnityEngine.Debug.Log(boundaries);
        float s = Mathf.Max(boundaries.x, boundaries.y, boundaries.z);
        _studyObject.transform.localScale = Vector3.one / s;

        _views = ViewSphereGenerator.GenerateViews(_gridSize, _sphereRadius);
        _drm.SetCameraView(_views[_viewIndex]);
    }

    void Update()
    {
        
        Texture2D tex = _drm.GetDepthRendering();
       
        UnityEngine.Debug.Log(_timer.Elapsed);

        if (Input.GetKeyDown(KeyCode.C))
        {
            RenderView(tex);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            BuildView(tex);
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

    }

    private void RenderView(Texture2D tex)
    {
        _timer.Reset();
        _timer.Start();
        HashSet<Vector3> pointSet = _pcm.CreatePointSet(tex);
        _timer.Stop();
        Mesh m = MeshCreator.GenerateMeshFromSet(pointSet, Vector3.zero, Vector3.zero, Color.green, 0.005f);
        //timer.Stop();
        UnityEngine.Debug.Log(_timer.Elapsed);

        CreateGameObject(m);
        
    }

    private void BuildView(Texture2D tex)
    {
        _timer.Reset();
        _timer.Start();
        HashSet<Vector3> pointSet = _pcm.CreatePointSet(tex);
        _timer.Stop();
        Mesh m = MeshCreator.GenerateMeshFromSet(pointSet, Vector3.zero, Vector3.zero, Color.green, 0.005f);
        //timer.Stop();
        UnityEngine.Debug.Log(_timer.Elapsed);

        CreateGameObject(m);


        _ogm.AddPoints(pointSet);
        _ogm.BuildGridObject(new Vector3(1,0,0));
    }


    private void CreateGameObject(Mesh m)
    {
        var go = new GameObject("Empty");

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshFilter>().mesh = m;
        go.transform.position = Vector3.zero;
        go.transform.position = new Vector3(5, 0, 0);
    }

    private void BuildViewSphere()
    {
         ViewSphereGenerator.BuildSphere(_views, Vector3.zero);
    }

}
