using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager{

    int _currentStep = 0;
    Vector3 virtualPosition = Vector3.zero;
    Vector3 lastPositon = Vector3.zero;
    Vector3 cV = new Vector3(0, 0, -10f);

    float speed = 0.1f;
    GameObject agent;


    public BallManager(GameObject agent)
    {
        this.agent = agent;
    }


    public void Reset()
    {
        _currentStep = 0;
        virtualPosition = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        UpdatePosition();
    }

    public float[] CollectObservations()
    {
        float[] obs = new float[2];
        obs[0] = agent.transform.position.x;
        obs[1] = agent.transform.position.z + 10f;
        return obs;
    }

    public System.Tuple<float, bool> Action(float[] vectorAction)
    {
        ApplyAction(vectorAction);
        return ComputeReward();
    }

    private void ApplyAction(float[] vectorAction)
    {
        float pX = (vectorAction[0] - vectorAction[1]) * speed;
        float pZ = (vectorAction[2] - vectorAction[3]) * speed;
        
        virtualPosition += new Vector3(pX, 0, pZ);
        UpdatePosition();
        _currentStep++;

    }

    private System.Tuple<float, bool> ComputeReward()
    {
        //Compute the reward of the chosen view
        //Determine if done based on accuracy or other measures

        
        bool done = false;
        float reward = lastPositon.magnitude - virtualPosition.magnitude;
        reward *= 5f;
        lastPositon = virtualPosition;

        if ( virtualPosition.magnitude < 0.5f){
            reward = 1f;
            done = true;
        }
        else if (_currentStep > 75)
        {
            done = true;
        }

        return System.Tuple.Create(reward, done);
    }

    private void UpdatePosition()
    {
        agent.transform.position = virtualPosition + cV;
    }

}
