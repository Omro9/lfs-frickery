using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orient : MonoBehaviour {
    private GameObject player;

    private float oldLat, oldLong;
    private Quaternion oldCameraRotation;

    // Use this for initialization
    void Start () {
        player = GameObject.Find("Player");
        oldLat = 90;    // Initial position of skybox
        oldLong = 0;    // ^
        oldCameraRotation = player.transform.localRotation;
    }
	
	// Update is called once per frame
	void Update () {
        // Need to take into account initial displacement (i.e. initial lat/long) of starmap_16k
        // initial displacement:
        // uses equatorial coordinate system: right ascention given in hours, declination given in degrees
        // geocentric, so no need to worry about rotation to align with equator
        // so, initial position of skydome is where north pole is directly above you (aka offset in lat/long of like 90N, 0E)

        // This is a vector approach to rotation:
        //      Find original position vector, find new position vector.
        //      Cross them to find axis of rotation
        //      Use Vector3.SignedAngle() to calculate change in degrees of rotation along axis by which to rotate skybox

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
    }
}
