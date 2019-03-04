using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearningController : MonoBehaviour {
    [SerializeField]
    GameObject agent;

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
            Debug.Log("Commencing training with model");

            agent.GetComponent<TestingAgent>().enabled = false;
            agent.GetComponent<NbvAgent>().enabled = true;
        }

    }
}
