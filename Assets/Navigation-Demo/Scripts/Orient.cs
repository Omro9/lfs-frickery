using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script to orient the skybox camera based upon player's latitude and longitude as well as time of day.
 * Also rotates sun around to simulate day passing
 */
public class Orient : MonoBehaviour {
    private GameObject player;
    private GameObject sun;

    private const double earthAngularVelocity = 7.2921159D / 100000D * 3600D * Mathf.Rad2Deg;   // In deg/hr

    private float oldLat, oldLong;
    private Quaternion oldCameraRotation;
    private Vector3 playerPosition;

    // Use this for initialization
    void Start () {
        player = GameObject.Find("Player");
        oldLat = 90;    // Initial position of skybox
        oldLong = 0;    // ^
        oldCameraRotation = player.transform.localRotation;
        //sun = GameObject.Find("Sun");
        playerPosition = player.transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        // Need to take into account initial displacement (i.e. initial lat/long) of starmap_16k
        // initial displacement:
        // uses equatorial coordinate system: right ascention given in hours, declination given in degrees
        // geocentric, so no need to worry about rotation to align with equator
        // so, initial position of skydome is where north pole is directly above you (aka offset in lat/long of like 90N, 0E)

        /*
         * ROTATE BASED ON GLOBAL POSITION
         * 
         * This is a vector approach to rotation:
         *     Find original position vector, find new position vector.
         *     Cross them to find axis of rotation
         *     Use Vector3.SignedAngle() to calculate change in degrees of rotation along axis by which to rotate skybox
         * 
         * This can probably be run only occasionally, as lat/long won't change significantly frame-to-frame.
         */
        float curLat = player.GetComponent<CanoeControls>().latitude;
        float curLong = player.GetComponent<CanoeControls>().longitude;
        Quaternion curCameraRotation = player.transform.localRotation;
        Vector3 v = new Vector3(Mathf.Cos(oldLat) * Mathf.Cos(oldLong),
                                Mathf.Sin(oldLat), 
                                Mathf.Cos(oldLat) * Mathf.Sin(oldLong));
        Vector3 w = new Vector3(Mathf.Cos(curLat) * Mathf.Cos(curLong),
                                Mathf.Sin(curLat),
                                Mathf.Cos(curLat) * Mathf.Sin(curLong));
        Vector3 axis = Vector3.Cross(v, w);
        float angle = Vector3.SignedAngle(v, w, axis);
        transform.Rotate(axis, angle, Space.World);
        transform.Rotate(curCameraRotation.eulerAngles - oldCameraRotation.eulerAngles);
        oldLat = curLat;
        oldLong = curLong;
        oldCameraRotation = curCameraRotation;

        /*
         * ROTATE BASED ON TIME OF DAY
         * 
         * One thing to note: because of the similar radii of our stars and sun, will not get parallax effect
         * of sun passing over stars
         */
        transform.Rotate(Vector3.up, Time.deltaTime * Sun.gameHoursPerRealSecond * (float) earthAngularVelocity, Space.World);

        // Rotate sun object
        // JANK LINES, PROLLY DON'T WORK GOOD
        //sun.transform.RotateAround(GameObject.Find("Skybox Camera").transform.position, axis, angle);
        //sun.transform.RotateAround(GameObject.Find("Skybox Camera").transform.position, new Vector3(0, 1, 0), Time.deltaTime * Sun.gameHoursPerRealSecond * (float)earthAngularVelocity);

        followPlayer();
    }

    private void followPlayer() {
        Vector3 delta = player.transform.position - playerPosition;
        transform.Translate(delta);
        playerPosition = player.transform.position;
    }
}
