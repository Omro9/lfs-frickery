using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to handle rotation of moon around Earth. Calculating the position of the moon is really difficult, as it turns out.
/// Right now, this script just defines the plane of the moon's orbit and rotates it around that based on the time. This is 
/// really innaccurate, as is (either that, or I screwed up the implementation). I'm considering trying to pull some ephemeride 
/// data from online and just using the moon's positions from one day on repeat.
/// <para>-- Owen</para>
/// </summary>
public class Moon : MonoBehaviour {
    public static int RADIUS = 100;
    private const float earthCenterOffsetMeters = 1.658F; // At radius of 100, Earth's center is this many units below the player (pretty negligable)
    private const double SCALE = 0.873;   // Scale factor calculated from https://en.wikipedia.org/wiki/Angular_diameter
    private const float angularVelocity = 360F / 29.53F / 24F; // In deg/hour, using synodic lunar month
    private const float orbitalInclinationFromEquator = (23.4F + 5.145F) * Mathf.Deg2Rad;
    private readonly Vector3 earthCenterOffset = new Vector3(0, -earthCenterOffsetMeters, 0);

    // Vector defining orbital plane
    private Vector3 orbitalPlane = Vector3.Cross(new Vector3(Mathf.Cos(orbitalInclinationFromEquator),
                                                               Mathf.Sin(orbitalInclinationFromEquator),
                                                               0),
                                                   Vector3.forward).normalized;

    private float initIntensity;
    private float gamma;    // Angle of rotation around Earth in degrees (measured from new moon).
    private float deltaGamma;
    private Vector3 playerPosition;
    private GameObject player;
    private GameObject sun;
    private SpriteRenderer spRend;

    private void Awake() {
        name = "Moon";
        spRend = GetComponent<SpriteRenderer>();
        spRend.material.renderQueue = 1501; // Put in back half of background (but rendered after sun)

    }

    // Use this for initialization
    void Start () {
        player = GameObject.Find("Player");
        sun = GameObject.Find("Sun");
        initIntensity = GetComponent<Light>().intensity;
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
        float numNewMoons = (float)((SkyboxController.JD - 2458136) / 29.53D);  // Time since last new moon in Guam 
                                                                                //  (Jan 17, 2018) divided by synodic month
        float initRotation = (numNewMoons - Mathf.Floor(numNewMoons)) * 360F; // Rotation in deg from new moon position
        transform.RotateAround(playerPosition + earthCenterOffset, orbitalPlane, initRotation); // Rotate moon to be at correct rotation dictated by JD
        gamma = initRotation;
    }
	
	// Update is called once per frame
	void Update () {
        // Rotate by moon's sydonic orbit
        //Debug.Log("\nNorth: " + sun.GetComponent<SkyboxController>().North);
        Quaternion rotateToNorth = Quaternion.FromToRotation(Vector3.up, SkyboxController.North);
        Vector3 orbitalPlaneRelativeToNorth = rotateToNorth * orbitalPlane;
        float rotation = Time.deltaTime * SkyboxController.gameHoursPerRealSecond * angularVelocity;
        transform.RotateAround(playerPosition + earthCenterOffset, orbitalPlaneRelativeToNorth, rotation);
        gamma = (gamma + rotation) % 360F;
        deltaGamma += rotation;

        // Rotate by time of day
        transform.RotateAround(playerPosition + earthCenterOffset,
                               SkyboxController.North,
                               (float) (Time.deltaTime * SkyboxController.gameHoursPerRealSecond * SkyboxController.earthAngularVelocity));
        if (deltaGamma > 1F)
            UpdatePhase();
        
        if (SkyboxController.IsDaytime) {   // If it's day, tint the moon to be bluish
            spRend.color = Color.Lerp(Color.cyan, Color.white, gamma / 180F);
        }
        float intensityScalar = (transform.position.y < 0) ? Mathf.Lerp(0, 1, 1 - transform.position.y / -15F) : 1F;   // 15 deg below horizon is arbitrary
        float phaseIntensityScalar = Mathf.Lerp(0.25F, 1.3F, gamma / 180);
        GetComponent<Light>().intensity = initIntensity * intensityScalar * phaseIntensityScalar;
        FollowPlayer();
	}

    /// <summary>
    /// Update sprite of moon to reflect correct phase. Sprites taken from http://www.neoprogrammics.com/lunar_phase_images/
    /// </summary>
    private void UpdatePhase() {
        string spriteName = ((int)gamma).ToString();
        Sprite newPhase = Resources.Load<Sprite>("trans_images/trans_" + spriteName);
        spRend.sprite = newPhase;
        //Debug.Log(spriteName + "\t" + newPhase);
        deltaGamma = 0F;
    }

    private void FollowPlayer()
    {
        Vector3 delta = player.transform.position - playerPosition;
        transform.Translate(delta);
        playerPosition = player.transform.position;
        transform.LookAt(playerPosition, transform.position.normalized - SkyboxController.North);
    }
}
