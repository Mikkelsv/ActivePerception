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
    private int _gridSize = 5;

    [SerializeField]
    private float _sphereRadius = 3f;

    [SerializeField]
    GameObject _studyObject;

    private Stopwatch _timer;

    private PointCloudManager _pcm;

    private HashSet<Vector3> _pc = new HashSet<Vector3>();

    private OccupancyGridManager _ogm;

    private DepthRenderingManager _drm;

    private List<Vector3> _views;

    private void Start()
    {
        RenderTexture rTex = _depthCamera.targetTexture;
        _depthCamera.SetReplacementShader(_shader, null);
        _timer = new Stopwatch();
        _drm = new DepthRenderingManager(_depthCamera);
        _pcm = new PointCloudManager(rTex);
        _ogm = new OccupancyGridManager(32);
        
        Vector3 boundaries = _studyObject.GetComponent<MeshFilter>().mesh.bounds.size;
        UnityEngine.Debug.Log(boundaries);
        float s = Mathf.Max(boundaries.x, boundaries.y, boundaries.z);
        _studyObject.transform.localScale = Vector3.one / s;

        _views = ViewSphereGenerator.GenerateViews(_gridSize, _sphereRadius);
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

    }

    private void RenderView(Texture2D tex)
    {
        _timer.Reset();
        _timer.Start();
        HashSet<Vector3> pointSet = _pcm.CreatePointSet(tex, _depthCamera);
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
        HashSet<Vector3> pointSet = _pcm.CreatePointSet(tex, _depthCamera);
        _timer.Stop();
        Mesh m = MeshCreator.GenerateMeshFromSet(pointSet, Vector3.zero, Vector3.zero, Color.green, 0.005f);
        //timer.Stop();
        UnityEngine.Debug.Log(_timer.Elapsed);

        CreateGameObject(m);


        _ogm.AddPoints(pointSet);
        _ogm.BuildGridObject();
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
         ViewSphereGenerator.BuildSphere(_views);
    }

}
