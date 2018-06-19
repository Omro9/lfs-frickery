using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is responsible for translating this object using keyboard input
// for easy testing purposes
public class Navigation : MonoBehaviour {

    public float m_currentSpeed = 0.0f;
    public float m_maxSpeed = 10.0f;

    public Camera m_camera;
    private Rigidbody m_rigidbody;

    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

	void Update () {

        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("w pressed");
            m_currentSpeed += .04f;
            m_rigidbody.AddForceAtPosition(transform.forward * m_currentSpeed, transform.position);
        }

        Debug.Log(m_currentSpeed);
        m_currentSpeed = Mathf.Clamp(m_currentSpeed, 0f, m_maxSpeed);   
	}
}
