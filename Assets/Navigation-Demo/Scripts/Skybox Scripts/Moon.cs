using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to handle rotation of moon around Earth
/// </summary>
public class Moon : MonoBehaviour {
    public static int RADIUS = 100;
    private const double SCALE = 0.87;   // Scale factor calculated from https://en.wikipedia.org/wiki/Angular_diameter
    private const float angularVelocity = 360F / 29.53F / 24F; // In deg/hour, using synodic lunar month
    private const float orbitalInclinationFromEquator = (23.4F + 5.145F) * Mathf.Deg2Rad;

    // Vector defining orbital plane
    private Vector3 orbitalPlane = Vector3.Cross(new Vector3(Mathf.Cos(orbitalInclinationFromEquator),
                                                               Mathf.Sin(orbitalInclinationFromEquator),
                                                               0),
                                                   Vector3.forward).normalized;

    private float gamma;    // Angle of rotation around Earth in degrees.
    private float deltaGamma;
    private Vector3 playerPosition;
    private GameObject player;
    private GameObject sun;
    private SpriteRenderer spRend;

	// Use this for initialization
	void Start () {
        player = GameObject.Find("Player");
        sun = GameObject.Find("Sun");
        spRend = GetComponent<SpriteRenderer>();
        playerPosition = player.transform.position;
        transform.position = RADIUS * new Vector3(Mathf.Cos(orbitalInclinationFromEquator),
                                                  Mathf.Sin(orbitalInclinationFromEquator),
                                                  0) + playerPosition; // This is not the actual position of the moon at the given JD

        // Rotate moon to be between Earth and Sun to make it a new moon
        Vector3 sunRelativeToPlayer = sun.transform.position - playerPosition;
        Vector3 sunProj = Vector3.ProjectOnPlane(sunRelativeToPlayer, orbitalPlane);
        //Vector3 moonPos = transform.position;
        //transform.Rotate(axisOfRotation, Vector3.Angle(moonPos, sunProj));
        transform.position = sunProj.normalized * RADIUS;

        // Find progress of moon in orbit. Calculations from https://www.subsystems.us/uploads/9/8/9/4/98948044/moonphase.pdf
        float numNewMoons = (float) ((sun.GetComponent<Sun>().JD - 2458136) / 29.53D);  // Time since last new moon in Guam 
                                                                                        //  (Jan 17, 2018) divided by synodic month
        float initRotation = (numNewMoons - Mathf.Floor(numNewMoons)) * 360; // Rotation in deg from new moon position
        transform.Rotate(orbitalPlane, initRotation); // Rotate moon to be at correct rotation dictated by JD
        gamma = initRotation;
    }
	
	// Update is called once per frame
	void Update () {
        // Rotate by moon's sydonic orbit
        //Debug.Log("\nNorth: " + sun.GetComponent<SkyboxController>().North);
        Quaternion rotateToNorth = Quaternion.FromToRotation(Vector3.up, sun.GetComponent<SkyboxController>().North);
        Vector3 orbitalPlaneRelativeToNorth = rotateToNorth * orbitalPlane;
        float rotation = Time.deltaTime * sun.GetComponent<Sun>().gameHoursPerRealSecond * angularVelocity;
        transform.RotateAround(playerPosition, orbitalPlaneRelativeToNorth, rotation);
        gamma = (gamma + rotation) % 360F;
        deltaGamma += rotation;

        // Rotate by time of day
        transform.RotateAround(playerPosition,
                               sun.GetComponent<SkyboxController>().North,
                               (float) (Time.deltaTime * sun.GetComponent<Sun>().gameHoursPerRealSecond * Sun.earthAngularVelocity));
        if (deltaGamma > 1F)
            UpdatePhase();
        FollowPlayer();
	}

    private void UpdatePhase() {
        string spriteName = ((int) gamma).ToString().PadLeft(3).Replace(' ', '0');
        Sprite newPhase = Resources.Load<Sprite>("Lunar Phases/" + spriteName);
        spRend.sprite = newPhase;
        Debug.Log(spriteName + "\t" + newPhase);
        deltaGamma = 0F;
    }

    /// <summary>
    /// <para>Have GameObject follow position of player. Object is not made a child of player to preserve the object's rotation.</para>
    /// Probably should make parent class, as this method is shared among Moon.cs, Sun.cs, and(to an extent) in Orient.cs.
    /// </summary>
    private void FollowPlayer()
    {
        Vector3 delta = player.transform.position - playerPosition;
        transform.Translate(delta);
        playerPosition = player.transform.position;
        transform.LookAt(player.transform);
    }
}
