using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StudyObjectMamanger{

    private GameObject[] _studyObjects;
    private GameObject _studyObject;
    private GameObject _studyObjectsManager;

    private Vector3 _position = new Vector3(1000, 0, 0);
    private Vector3 _objectPosition;

    private static System.Random _rnd = new System.Random();
    private int _countObjects;

    public StudyObjectMamanger(Vector3 objectPosition)
    {
        _objectPosition = objectPosition;
        LoadStudyObjects();
        _countObjects = _studyObjects.Count();
        SetRandomStudyObject();
    }

    public void PrepareNewStudyObject()
    {
        RemoveStudyObject();
        SetRandomStudyObject();
    }

    private void SetRandomStudyObject()
    {
        //Set new study object randomly
        int i = _rnd.Next(_countObjects);
        _studyObject = _studyObjects[i];

        _studyObject.SetActive(true);
        _studyObject.transform.position = _objectPosition;
    }

    private void RemoveStudyObject()
    {
        _studyObject.transform.position = _position;
        _studyObject.SetActive(false);
    }

    private void LoadStudyObjects()
    {
        
        _studyObjectsManager = new GameObject("StudyObjectsManager");
        _studyObjectsManager.transform.position = _position;
        GameObject[] modelPrefabs = Resources.LoadAll("Models", typeof(GameObject)).Cast<GameObject>().ToArray();
        _studyObjects = new GameObject[modelPrefabs.Length];

        int i = 0;
        foreach(GameObject g in modelPrefabs)
        {
            GameObject studyObject = GameObject.Instantiate(g,_studyObjectsManager.transform);

            Vector3 boundaries = studyObject.GetComponentInChildren<MeshFilter>().mesh.bounds.size;
            studyObject.transform.localScale = Vector3.one / GetMaxElement(boundaries);
            studyObject.name = "StudyObject_" + i.ToString();

            _studyObjects[i] = studyObject;
            studyObject.SetActive(false);
            i++;
        }
    }

    private float GetMaxElement(Vector3 v)
    {
        return Mathf.Max(v.x, v.y, v.z);
    }
}
