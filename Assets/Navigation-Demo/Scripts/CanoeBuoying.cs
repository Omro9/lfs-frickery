using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is responsible for varying buoying speed and altitude that is designed
// to vary over time and altitude for a realistic floating effect
public class CanoeBuoying : MonoBehaviour {

    private float m_maxAngle = 5f;
    private float m_speed = 2f;

    private float m_time;

    void Start()
    {
        m_time = 0;
    }

    void Update()
    {
        // Rotate the canoe
        m_time += m_speed * Time.deltaTime;
        float angle = m_maxAngle * Mathf.Sin(m_time);
        transform.eulerAngles = new Vector3(angle, transform.eulerAngles.y, transform.eulerAngles.z);

        // Vary the time and max angle between buoying for more variation
        m_maxAngle = 2f * Mathf.Sin(m_time) * Time.deltaTime + m_maxAngle;
        m_speed = 2f * Mathf.Sin(m_time)* Time.deltaTime + m_speed;

        m_maxAngle = Mathf.Clamp(m_maxAngle, 1f, 10f);
        m_speed = Mathf.Clamp(m_speed, 1f, 3f);


    }
}
