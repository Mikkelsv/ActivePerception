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

    private RenderTexture _rTex;

    private Stopwatch _timer;

    private PointCloudManager _pcm;

    private HashSet<Vector3> _pc = new HashSet<Vector3>();

    private void Start()
    {
        _rTex = _cam.targetTexture;
        _timer = new Stopwatch();
        _pcm = new PointCloudManager(_rTex);
        
        _cam.SetReplacementShader(_shader, null);

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
        int[] grid = OccupancyGrid.GenerateGrid(pointSet, _gridSize);

        GameObject structure = OccupancyGrid.GenerateOccupancyStructure(grid, _gridSize);
        UnityEngine.Debug.Log(structure.transform.childCount);
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
        int[] grid = OccupancyGrid.GenerateGrid(pointSet, _gridSize);

        GameObject structure = OccupancyGrid.GenerateOccupancyStructure(grid, _gridSize);
        UnityEngine.Debug.Log(structure.transform.childCount);
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
