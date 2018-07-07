using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script to handle rotation of moon around Earth
 */
public class Moon : MonoBehaviour {
    public static int RADIUS = 100;
    private const float angularVelocity = 360F / 27.322F / 24F; // In deg/hour, using sidereal orbit of moon
    private const float orbitalInclinationFromEquator = (23.4F + 5.145F) * Mathf.Deg2Rad;
    private readonly Vector3 axisOfRotation = Vector3.Cross(new Vector3(Mathf.Cos(orbitalInclinationFromEquator),
                                                                        Mathf.Sin(orbitalInclinationFromEquator),
                                                                        0),
                                                            Vector3.forward).normalized;

    private float gamma;    // Angle of rotation around Earth in degrees.
    private GameObject skyboxCamera;
    private Vector3 skyboxCameraPosition;

	// Use this for initialization
	void Start () {
        skyboxCamera = GameObject.Find("Skybox Camera");
        skyboxCameraPosition = skyboxCamera.transform.position;
        transform.position = RADIUS * new Vector3(Mathf.Cos(orbitalInclinationFromEquator),
                                                  Mathf.Sin(orbitalInclinationFromEquator),
                                                  0) + skyboxCameraPosition; // This is not the actual position of the moon at the given JD
        //transform.RotateAround(new Vector3(0, 0, 0), //GameObject.Find("Skybox Camera").transform.position,
        //                       new Vector3(0, 0, 1), 
        //                       Vector3.Angle(new Vector3(0, 1, 0), axisOfRotation));
        gamma = 0;
	}
	
	// Update is called once per frame
	void Update () {
        gamma += Time.deltaTime * Sun.gameHoursPerRealSecond * angularVelocity;
        transform.RotateAround(skyboxCamera.transform.position, //GameObject.Find("Skybox Camera").transform.position,
                               axisOfRotation,
                               Time.deltaTime * Sun.gameHoursPerRealSecond * angularVelocity);
        transform.LookAt(skyboxCamera.transform);   // Hopefully will provided directed light to scene/moon's front

        followCamera();
	}

    private void followCamera()
    {
        Vector3 delta = skyboxCamera.transform.position - skyboxCameraPosition;
        transform.Translate(delta);
        skyboxCameraPosition = skyboxCamera.transform.position;
    }
}
