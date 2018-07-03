using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script to handle rotation of moon around Earth
 */
public class Moon : MonoBehaviour {
    public const float RADIUS = 100F;
    private const float angularVelocity = 360F / 27.322F / 24F; // In deg/hour, using sidereal orbit of moon
    private const float orbitalInclinationFromEquator = (23.4F + 5.145F) * Mathf.Deg2Rad;
    private readonly Vector3 axisOfRotation = new Vector3(Mathf.Cos(orbitalInclinationFromEquator),
                                                 Mathf.Sin(orbitalInclinationFromEquator),
                                                 0);

    private float gamma;    // Angle of rotation around Earth in degrees.

	// Use this for initialization
	void Start () {
        transform.position = new Vector3(RADIUS, 0, 0); // This is not the actual position of the moon at the given JD
        transform.RotateAround(new Vector3(0, 0, 0), //GameObject.Find("Skybox Camera").transform.position,
                               new Vector3(0, 0, 1), 
                               Vector3.Angle(new Vector3(0, 1, 0), axisOfRotation));
        gamma = 0;
	}
	
	// Update is called once per frame
	void Update () {
        gamma += Time.deltaTime * Sun.gameHoursPerRealSecond * angularVelocity;
        transform.RotateAround(new Vector3(0, 0, 0), //GameObject.Find("Skybox Camera").transform.position,
                               axisOfRotation,
                               Time.deltaTime * Sun.gameHoursPerRealSecond * angularVelocity);
	}
}
