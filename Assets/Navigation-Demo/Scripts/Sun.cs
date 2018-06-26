using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour {

    public Transform m_player;

    private float m_sunRadius = 100f;
    private float m_timeInOneDay = 10f;

    private float time;

    void Start()
    {
        transform.position = new Vector3(0f, m_sunRadius, 0f);
    }

    void Update()
    {
        transform.RotateAround(m_player.position, Vector3.right, m_timeInOneDay * Time.deltaTime);
        transform.LookAt(m_player.position);
    }
}
