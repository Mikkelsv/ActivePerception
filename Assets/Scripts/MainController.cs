using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

public class MainController : MonoBehaviour
{

    [SerializeField]
    private Camera _cam;

    [SerializeField]
    Shader _shader;

    [SerializeField]
    int _gridSize = 32;

    [SerializeField]
    GameObject _studyObject;

    private RenderTexture _rTex;

    private Stopwatch _timer;

    private PointCloudManager _pcm;

    private HashSet<Vector3> _pc = new HashSet<Vector3>();

    private OccupancyGridManager _ogm;

    private void Start()
    {
        _rTex = _cam.targetTexture;
        _timer = new Stopwatch();
        _pcm = new PointCloudManager(_rTex);
        _ogm = new OccupancyGridManager(32);
        
        _cam.SetReplacementShader(_shader, null);

        Vector3 boundaries = _studyObject.GetComponent<MeshFilter>().mesh.bounds.size;
        UnityEngine.Debug.Log(boundaries);
        float s = Mathf.Max(boundaries.x, boundaries.y, boundaries.z);
        _studyObject.transform.localScale = Vector3.one / s;



    }

    void Update()
    {

        Texture2D tex = new Texture2D(_rTex.width, _rTex.height, TextureFormat.RGB24, false);

        RenderTexture.active = _cam.targetTexture;
        _cam.Render();
        tex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        tex.Apply();

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
            GenerateViewSphere();
        }

    }

    private void RenderView(Texture2D tex)
    {
        _timer.Reset();
        _timer.Start();
        HashSet<Vector3> pointSet = _pcm.CreatePointSet(tex, _cam);
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
        HashSet<Vector3> pointSet = _pcm.CreatePointSet(tex, _cam);
        _timer.Stop();
        Mesh m = MeshCreator.GenerateMeshFromSet(pointSet, Vector3.zero, Vector3.zero, Color.green, 0.005f);
        //timer.Stop();
        UnityEngine.Debug.Log(_timer.Elapsed);

        CreateGameObject(m);


        _ogm.AddPoints(pointSet);
        _ogm.BuildGridObject();
    }


    void CreateGameObject(Mesh m)
    {
        var go = new GameObject("Empty");

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshFilter>().mesh = m;
        go.transform.position = Vector3.zero;
        go.transform.position = new Vector3(5, 0, 0);
    }

    private void GenerateViewSphere()
    {
        HashSet<Vector3> views = ViewSphereGenerator.GenerateViewSphere();
        ViewSphereGenerator.BuildSphere(views);
    }

}
