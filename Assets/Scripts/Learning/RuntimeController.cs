using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeController : MonoBehaviour {
    [SerializeField]
    GameObject agent;

    [SerializeField]
    GameObject acadamy;

    [SerializeField]
    bool testWithModel;

    [SerializeField]
    TextAsset model;


    void Awake()
    {
        var nvbAgent = agent.GetComponent<NbvAgent>();
        var testingAgent = agent.GetComponent<TestingAgent>();

        if (testWithModel)
        {
            Debug.Log("Testing environment with model");
            testingAgent.model = model;
            testingAgent.enabled = true;
            nvbAgent.enabled = false;
            acadamy.SetActive(false);
        }
        else
        {
            Debug.Log("Commencing training using mlagents");
            testingAgent.enabled = false;
            nvbAgent.enabled = true;
            acadamy.SetActive(true);
        }
    }
}
