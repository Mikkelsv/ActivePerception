using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlow;
using System.Linq;
using System;

public class TestingAgent : MonoBehaviour {

    [SerializeField]
    Camera depthcamera;

    [SerializeField]
    float actionFrequency;

    [SerializeField]
    int maxEpisodes;
        
    float nextAction = 0;
    bool nvb = true;
    int episode = 0;

    public TextAsset model;
    
    private int _maxStep;

    private NbvManager _ball;
    private int _currentStep = 0;
    private float _currentReward;

    bool log = true;

    TFGraph graph;

    // Use this for initialization
    void Start()
    {

#if UNITY_ANDROID
                TensorFlowSharp.Android.NativeBinding.Init();
#endif

        _ball = new NbvManager(depthcamera);
        _maxStep = _ball.maxStep;

        _currentReward = 0f;

        graph = new TFGraph();
        graph.Import(model.bytes);


    }
    
    void Update()
    {
        if((Time.time > nextAction) && nvb)
        {
            _currentStep++;

            float[] actions = ComputeAction(_ball.CollectObservations());
            Evaluate(actions);

            if (_currentStep > 2)
            {
                Debug.Log("Accumulated Reward: " + _currentReward);
                _currentReward = 0;
                _ball.Reset();
                _currentStep = 0;
                episode++;
                if (true)
                {
                    nvb = false;
                    Debug.Log("done");
                }

            }
        }
        
    }

    private void Evaluate(float[] actions)
    {
        float maxValue = actions.Max();
        int maxIndex = actions.ToList().IndexOf(maxValue);
        float[] action = { maxIndex };
        var eval = _ball.Action(action);
        _currentReward += eval.Item1;
        if (eval.Item2)
        {
            Debug.Log("Accumulated Reward: " + _currentReward);
            _currentReward = 0;
            _ball.Reset();
            _currentStep = 0;
        }

    }


    private float[] ComputeAction(float[] observations)
    {
        float[,] obs = ExpandArray(observations);
        float[] actions;

        if (log)
        {
            Debug.Log(graph);
            foreach (var x in graph.GetEnumerator())
            {
                Debug.Log(x.Name);
            }
            log = false;
        }

        var session = new TFSession(graph);
        var runner = session.GetRunner();

        TFTensor input = obs;
        TFOutput observation_node = graph["observations"][0];
        runner.AddInput(observation_node, input);

        TFOutput action_node = graph["actions/Sigmoid"][0];

        runner.Fetch(action_node);
        var a = runner.Run()[0];
        actions = FlattenArray(a.GetValue() as float[,]);

        float m = actions.Max();
        int i = Array.IndexOf(actions, m);
        actions = new float[actions.Length];
        actions[i] = 1;

        return actions;
    }

    private float[] FlattenArray(float[,] a)
    {
        float[] b = new float[a.Length];
        int i = 0;
        foreach (float x in a)
        {
            b[i] = x;
            i++;
        }
        return b;
    }

    private float[,] ExpandArray(float[] a)
    {
        float[,] b = new float[1, a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            b[0, i] = a[i];
        }
        return b;
    }
}
