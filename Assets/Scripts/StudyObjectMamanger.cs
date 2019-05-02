using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StudyObjectMamanger
{

    private GameObject[] _studyObjects;
    private GameObject _studyObject;
    private int _studyObjectIndex = 0;
    private GameObject _studyObjectsManager;

    private Vector3 _position = new Vector3(0, 0, 0);
    private Vector3 _objectPosition;

    private static System.Random _rnd = new System.Random();
    private int _countObjects;
    private bool _randomOrientation;

    public int RotationVariations { get; }

    public StudyObjectMamanger(Vector3 objectPosition, int rotationVariations=12)
    {
        _objectPosition = objectPosition;
        RotationVariations = rotationVariations;
        LoadStudyObjects();
        _countObjects = _studyObjects.Count();
        Debug.Log(_countObjects +" prepared");
        PrepareStudyObject(0);
    }

    public void PrepareRandomStudyObject()
    {
        int i = _rnd.Next(_countObjects);
        PrepareStudyObject(i);
    }

    public void PrepareNextStudyObject()
    {
        PrepareStudyObject((_studyObjectIndex + 1) % _countObjects);
    }

    public void PrepareStudyObject(int i)
    {
        RemoveStudyObject();
        _studyObjectIndex = i;
        _studyObject = _studyObjects[i];
        _studyObject.SetActive(true);
        _studyObject.transform.position = _objectPosition;
    }

    public int Count()
    {
        return _countObjects;
    }

    public int CurrentObject()
    {
        return _studyObjectIndex;
    }

    private void RemoveStudyObject()
    {
        if (_studyObject)
        {
            _studyObject.transform.position = _position;
            _studyObject.SetActive(false);
        }
    }

    private void LoadStudyObjects()
    {

        _studyObjectsManager = new GameObject("StudyObjectsManager");
        _studyObjectsManager.transform.position = _position;
        GameObject[] modelPrefabs = Resources.LoadAll("Models", typeof(GameObject)).Cast<GameObject>().ToArray();
        _studyObjects = new GameObject[modelPrefabs.Length * RotationVariations];
        int c = 0;
        foreach (GameObject g in modelPrefabs)
        {
            for(int i=0; i<RotationVariations; i++)
            {
                float rot = 360f / RotationVariations * i;

                GameObject studyObject = GameObject.Instantiate(g, _studyObjectsManager.transform);

                Vector3 boundaries = studyObject.GetComponentInChildren<MeshFilter>().mesh.bounds.size;
                studyObject.transform.localScale = Vector3.one / (GetMaxElement(boundaries) * 1.224f);
                studyObject.transform.rotation = Quaternion.Euler(new Vector3(0, rot, 0));
                studyObject.name = g.name + i.ToString();

                _studyObjects[c] = studyObject;
                studyObject.SetActive(false);
                c++;
            }
         
        }
    }

    private float GetMaxElement(Vector3 v)
    {
        return Mathf.Max(v.x, v.y, v.z);
    }
}
