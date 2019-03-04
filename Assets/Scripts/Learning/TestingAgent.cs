using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlow;
using System.Linq;
using System;

public class TestingAgent : MonoBehaviour {

    
    public TextAsset model;

    [SerializeField]
    private int _maxStep;

    [SerializeField]
    private int _decisionFrequency;

    [SerializeField]
    readonly int step;

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

        _ball = new NbvManager(this.gameObject);

        _currentReward = 0f;

        graph = new TFGraph();
        graph.Import(model.bytes);


    }

    // Update is called once per frame
    void Update()
    {
        _currentStep++;

        if (_currentStep % _decisionFrequency == 0)
        {
            float[] actions = ComputeAction(_ball.CollectObservations());
            Evaluate(actions);
        }
        if (_currentStep > _maxStep)
        {
            Debug.Log("Accumulated Reward: " + _currentReward);
            _currentReward = 0;
            _ball.Reset();
            _currentStep = 0;
        }
    }

    private void Evaluate(float[] actions)
    {
        var eval = _ball.Action(actions);
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
