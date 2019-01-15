using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableScript : MonoBehaviour
{
    private float velocity = 10f;
    private float rotationSpeed = 60.0f;


    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            HandleRotation();
        }
        else
        {
            HandlePosition();
        }
    }

    private void HandlePosition()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * velocity * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += transform.forward * -velocity * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += transform.right * -velocity * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * velocity * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += transform.up * velocity * Time.deltaTime;
        }
    }

    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.eulerAngles -= Vector3.right * rotationSpeed * Time.deltaTime; 
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.eulerAngles += Vector3.right * rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.eulerAngles -= Vector3.up * rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.eulerAngles += Vector3.up * rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position -= transform.up * velocity * Time.deltaTime;
        }

    }
}
