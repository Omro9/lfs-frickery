using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>Script to handle motion of sun relative to Earth. Also contains variables such as time multiplier and 
/// in-game time of day</para>
/// Many fields are marked static to avoid constantly searching for the "Sun" GameObject; however, this may
/// not be good practice. Perhaps it would be better to break this script up into a sun "controller" script (static)
/// and a sun movement script (extending MonoBehaviour).
/// </summary>
public class Sun : MonoBehaviour {
    private const int RADIUS = 200;
    private const double SCALE = 1.578; // Scale factor calculated from https://en.wikipedia.org/wiki/Angular_diameter

    private GameObject player;
    private Vector3 playerPosition;
    private float initIntensity;

    private void Awake()
    {
        name = "Sun";
        GetComponent<SpriteRenderer>().material.renderQueue = 1500; // Put in back half of background layer
        player = GameObject.Find("Player");
        playerPosition = player.transform.position;
        UpdateSunPosition();
    }

    void Start()
    {
        initIntensity = GetComponent<Light>().intensity;
    }

    void Update()
    {
        playerPosition = player.transform.position;
        transform.LookAt(player.transform);

        float alt = UpdateSunPosition();
        SkyboxController.BlendSkyboxes(alt);   // Update skybox alpha blending
        SkyboxController.SetDayTexTint(alt);   // Tint sky for sunset/-rise 
        float intensityScalar = (alt < 0) ? Mathf.Lerp(0, 1, 1 - alt / -20F): 1F;   // 20 deg below horizon is arbitrary
        GetComponent<Light>().intensity = initIntensity * intensityScalar;
        SkyboxController.isDaytime = alt > 0;
    }


    /// <summary>
    /// <para>Transform sun based on time of day. Does not take into account atmospheric refraction.</para>
    /// Equatorial coordinate calculations taken from https://en.wikipedia.org/wiki/Position_of_the_Sun#Approximate_position,
    /// horizontal calculations taken from https://en.wikipedia.org/wiki/Celestial_coordinate_system#Equatorial_%E2%86%94_horizontal
    /// </summary>
    /// <returns>The new altitude of the sun in degrees.</returns>
    private float UpdateSunPosition()
    {
        // Calculate position of sun relative to Earth (equatorial coordinates)
        long julianDate = SkyboxController.JD;
        double gmtTimeOfDay = SkyboxController.TimeOfDay;

        double n = julianDate - 2451545 - 0.5D + gmtTimeOfDay / 24D;
        float obliquity = (float)((23.439D - 0.0000004D * n) * Mathf.Deg2Rad);

        double meanLong = (280.46D + 0.9856474D * n) % 360D * Mathf.Deg2Rad;
        float g = (float)((357.528D + 0.9856003D * n) % 360D * Mathf.Deg2Rad);
        float elipticLong = (float)(meanLong + 1.915D * Mathf.Deg2Rad * Mathf.Sin(g) + 0.02D * Mathf.Deg2Rad * Mathf.Sin(2F * g));

        float ra = Mathf.Atan2(Mathf.Cos(obliquity) * Mathf.Sin(elipticLong), Mathf.Cos(elipticLong));  // In rads
        float dec = Mathf.Asin(Mathf.Sin(obliquity) * Mathf.Sin(elipticLong));  // In rads
        //Debug.Log("RA: " + ra + "\nDEC: " + dec);

        // Transform equatorial coordinates to horizontal ones
        double T = (((gmtTimeOfDay < 12D) ? -0.5 : 0.5) + (julianDate - 2451545)) / 36525D; // This assumes timeOfDay is in GMT
        double T0 = 6.697374558D + (2400.051336D * T) + (0.000025862D * T * T) + (gmtTimeOfDay * 1.0027379093D);   // I believe this also assumes time is in GMT
        double GST = T0 % 24D;
        double LST = (GST + player.GetComponent<CanoeControls>().Longitude / 15D) % 24D;    // In hours
        //Debug.Log("\nLocal TimeOfDay: " + localTimeOfDay + "\t\tGMT: " + gmtTimeOfDay + "\t\tLST: " + LST);

        float localHourAngle = (((float)(LST * 15F - ra * Mathf.Rad2Deg) + 360F) % 360F) * Mathf.Deg2Rad;   // In rads
        float playerLatRads = player.GetComponent<CanoeControls>().Latitude * Mathf.Deg2Rad;
        float azimuth = -Mathf.Atan2(Mathf.Cos(dec) * Mathf.Sin(localHourAngle),    // Azimuth as measured from N = +z axis opening E
                                     -Mathf.Sin(playerLatRads) * Mathf.Cos(dec) * Mathf.Cos(localHourAngle)
                                     + Mathf.Cos(playerLatRads) * Mathf.Sin(dec));
        float altitude = Mathf.Asin(Mathf.Sin(playerLatRads) * Mathf.Sin(dec)
                                    + Mathf.Cos(dec) * Mathf.Cos(playerLatRads) * Mathf.Cos(localHourAngle));
        //Debug.Log("\nAltitude:\t" + altitude * Mathf.Rad2Deg + "\t\tAzimuth:\t" + azimuth * Mathf.Rad2Deg);

        Vector3 north = Vector3.ProjectOnPlane(SkyboxController.North, Vector3.up); // North vector projected onto xz plane
        azimuth += Vector3.Angle(Vector3.forward, north) * Mathf.Deg2Rad;   // Adjust azimuth to be aligned with current North
        float horiX = RADIUS * Mathf.Sin(azimuth) * Mathf.Cos(altitude);
        float horiY = RADIUS * Mathf.Sin(altitude);
        float horiZ = RADIUS * Mathf.Cos(azimuth) * Mathf.Cos(altitude);
        Vector3 newPosition = new Vector3(horiX, horiY, horiZ);

        transform.position = newPosition + playerPosition;  // Follow player and update position
        return altitude * Mathf.Rad2Deg;
    }
}
