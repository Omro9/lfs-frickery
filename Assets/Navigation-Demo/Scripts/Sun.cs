using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour {

    public Camera skyboxCamera;
    private GameObject player;

    private float m_sunRadius = 200f;
    private float m_timeInOneDay = 10f;

    public double julianDate = 2458297.5F;
    private float timeOfDay;
    private const float gameHoursPerRealSecond = 100F;

    void Start()
    {
        player = GameObject.Find("Player");
        timeOfDay = 0F; // Time of day in hours
        transform.position = new Vector3(0f, m_sunRadius, 0f);
    }

    void Update()
    {
        //transform.RotateAround(skyboxCamera.transform.position, Vector3.right, 360 * (Time.deltaTime * gameHoursPerRealSecond) / 24F);
        //transform.LookAt(skyboxCamera.transform.position);

        timeOfDay += Time.deltaTime * gameHoursPerRealSecond;   // Increment time of day
        julianDate += Time.deltaTime * gameHoursPerRealSecond / 24F;

        // Transform sun based around time of day
        // Calculations taken from https://en.wikipedia.org/wiki/Position_of_the_Sun#Approximate_position
        float n = (float) (julianDate - 2451545);
        float epsilon = 23.439F - 0.0000004F * n * Mathf.Deg2Rad;

        float meanLong = (280.46F + 0.9856474F * n) % 360F * Mathf.Deg2Rad;
        float g = (357.528F + 0.9856003F * n) % 360F * Mathf.Deg2Rad;
        float sunLong = meanLong + 1.915F * Mathf.Sin(g) + 0.02F * Mathf.Sin(2F * g);
        //float R = 1.00014F - 0.01671F * Mathf.Cos(g) - 0.00014F * Mathf.Cos(2F * g);

        float ra = Mathf.Atan2(Mathf.Cos(epsilon) * Mathf.Sin(sunLong), Mathf.Cos(sunLong));
        float dec = Mathf.Asin(Mathf.Sin(epsilon) * Mathf.Sin(sunLong));

        float x = m_sunRadius * Mathf.Cos(sunLong);
        float z = m_sunRadius * Mathf.Cos(epsilon) * Mathf.Sin(sunLong);
        float y = m_sunRadius * Mathf.Sin(epsilon) * Mathf.Sin(sunLong);

        transform.position = new Vector3(x, y, z);
        transform.LookAt(skyboxCamera.transform);
    }

}
