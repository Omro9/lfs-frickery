using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to orient the skybox camera based upon player's latitude and longitude as well as time of day.
/// Also rotates sun around to simulate day passing
/// </summary>
public class Orient : MonoBehaviour {
    private GameObject player;
    private GameObject sun;

    public static double earthAngularVelocity = 7.2921159D / 100000D * 3600D * Mathf.Rad2Deg;   // In deg/hr

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
        /*
         * ROTATE BASED ON GLOBAL POSITION:
         *     Find original position vector, find new position vector.
         *     Cross them to find axis of rotation
         *     Use Vector3.SignedAngle() to calculate change in degrees of rotation along axis by which to rotate skybox
         * 
         * This can probably be run only on occasion, as lat/long won't change significantly frame-to-frame.
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
         * ROTATE BASED ON TIME OF DAY:
         *
         * It's pretty self-explanatory.
         */
        transform.Rotate(Vector3.up, Time.deltaTime * Sun.gameHoursPerRealSecond * (float) earthAngularVelocity, Space.World);

        // Rotate sun object
        // JANK LINES, PROLLY DON'T WORK GOOD
        //sun.transform.RotateAround(GameObject.Find("Skybox Camera").transform.position, axis, angle);
        //sun.transform.RotateAround(GameObject.Find("Skybox Camera").transform.position, new Vector3(0, 1, 0), Time.deltaTime * Sun.gameHoursPerRealSecond * (float)earthAngularVelocity);

        FollowPlayer();
    }

    /// <summary>
    /// <para>Have GameObject follow position of player. Object is not made a child of player to preserve the object's rotation.</para>
    /// Probably should make parent class, as this method is shared among Moon.cs, Sun.cs, and(to an extent) in Orient.cs.
    /// </summary>
    private void FollowPlayer() {
        Vector3 delta = player.transform.position - playerPosition;
        transform.Translate(delta);
        playerPosition = player.transform.position;
    }
}
