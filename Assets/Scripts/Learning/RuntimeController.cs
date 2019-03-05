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
        if (testWithModel)
        {
            Debug.Log("Testing environment with model");

            TestingAgent ta = agent.GetComponent<TestingAgent>();
            ta.model = model;
            ta.enabled = true;
            agent.GetComponent<NbvAgent>().enabled = false;
            agent.SetActive(true);
            this.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Commencing training using mlagents");

            agent.GetComponent<TestingAgent>().enabled = false;
            agent.GetComponent<NbvAgent>().enabled = true;
            acadamy.SetActive(false);
        }

    }
}
