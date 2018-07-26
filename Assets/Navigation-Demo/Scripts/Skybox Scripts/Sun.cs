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
    public const double earthAngularVelocity = 7.2921159D / 100000D * 3600D * Mathf.Rad2Deg;   // In deg/hr
    public static float gameHoursPerRealSecond = 1 / 2F;

    private GameObject player;
    private Vector3 playerPosition;

    public static long julianDate;
    public static long JD {
        get { return julianDate; }
    }

    private static double gmtTimeOfDay;    // In GMT
    private static int gmtToLocalConversion = 10;  // Guam is GMT + 10
    public static double LocalTimeOfDay {   
        get { return (gmtTimeOfDay + gmtToLocalConversion) % 24; }
    }
    private double oldTimeOfDay;
    public static double TimeOfDay {
        get { return gmtTimeOfDay; }
    }


    void Start()
    {
        GetComponent<SpriteRenderer>().material.renderQueue = 1500; // Put in back half of background layer
        player = GameObject.Find("Player");
        playerPosition = player.transform.position;

        // Find current julian Date
        System.DateTime nowGMT = System.DateTime.Now.ToUniversalTime();
        gmtToLocalConversion = (nowGMT - nowGMT.ToLocalTime()).Hours;
        julianDate = (long) (367 * nowGMT.Year - 7 * (nowGMT.Year + (nowGMT.Month + 9) / 12) / 4 + 275 * nowGMT.Month / 9 + nowGMT.Day + 1721013.5 
                             + nowGMT.ToUniversalTime().Hour / 24 - 0.5 * Mathf.Sign(100 * nowGMT.Year + nowGMT.Month - 190002.5F) + 0.5);
        gmtTimeOfDay = nowGMT.Hour + nowGMT.Minute / 60D + nowGMT.Second / 3600D; // Time of day in hours
    }

    void Update()
    {
        gmtTimeOfDay += Time.deltaTime * gameHoursPerRealSecond;    // Increment time of day
        if (gmtTimeOfDay > 24D) {
            gmtTimeOfDay -= 24D;
            ++julianDate;
        }

        playerPosition = player.transform.position;
        transform.LookAt(player.transform);

        UpdateSunPosition();
    }


    /// <summary>
    /// <para>Transform sun based on time of day.</para>
    /// Equatorial coordinate calculations taken from https://en.wikipedia.org/wiki/Position_of_the_Sun#Approximate_position,
    /// horizontal calculations taken from https://en.wikipedia.org/wiki/Celestial_coordinate_system#Equatorial_%E2%86%94_horizontal
    /// </summary>
    private void UpdateSunPosition()
    {
        // Calculate position of sun relative to Earth (equatorial coordinates)
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

        SkyboxController.BlendSkyboxes(altitude * Mathf.Rad2Deg);   // Update skybox alpha blending
        SkyboxController.SetDayTexTint(altitude * Mathf.Rad2Deg);   // Tint sky for sunset/-rise
    }

    /// <summary>
    /// Updates skybox's alpha blending of day/night skies.
    /// </summary>
    [System.Obsolete("Skybox alpha blending is handled exclusively by SkyboxController.cs")]
    private void UpdateSkybox(){
        float output;
        float t = (float) gmtTimeOfDay;
        if (t < 12F)
        {  // These conditionals are a naive way to apply then reverse the easing function
            output = Ease(t, 0F, 1F, 12F);
        }
        else
        {
            output = 1 - Ease(t - 12F, 0F, 1F, 12F);
        }

        RenderSettings.skybox.SetFloat("_BlendCubemaps", output);   // Shader for blending alpha channels of two cubemaps
        AnimateStar.daytimeScaleEffect = 1F - Mathf.Round(output);
        //GameObject.Find("Star Parent Object").transform.localScale = AnimateStar.initScale * Mathf.Round(1F - output);
    }

    /// <summary>
    /// A quintic in/out easing function taken from http://www.timotheegroleau.com/.
    /// </summary>
    /// <returns>The ease.</returns>
    /// <param name="t">Time variable.</param>
    /// <param name="b">Beginning value of parameter to be eased.</param>
    /// <param name="c">Change in value of parameter to be eased.</param>
    /// <param name="d">Final value of parameter to be eased. <example><code>t == d</code> returns 1.</example></param>
    private float Ease(float t, float b, float c, float d)
    {
        float ts = (t /= d) * t;    // aka t = t / d; ts = t^2; (ts=="t squared")
        float tc = ts * t;          // aka tc = t^3;            (tc=="t cubed")
        return b + c * (6F * tc * ts + -15F * ts * ts + 10F * tc); // aka begin + delta*(6*t^5-15*t^4+10t^3)
    }

    /// <summary>
    /// <para>Gets the time of day in a readable format.</para>
    /// Calculations from http://aa.usno.navy.mil/faq/docs/JD_Formula.php
    /// </summary>
    /// <returns>A string of the format "DD/MM/YYYY HOUR:MINUTE:SECONDS".</returns>
    public static string GetTimeOfDayFormatted() {
        int N, L, I, J, K;
        L = (int) (julianDate + 68569);
        N = 4 * L / 146097;
        L = L - (146097 * N + 3) / 4;
        I = 4000 * (L + 1) / 1461001;
        L = L - 1461 * I / 4 + 31;
        J = 80 * L / 2447;
        K = L - 2447 * J / 80;
        L = J / 11;
        J = J + 2 - 12 * L;
        I = 100 * (N - 49) + I + L;

        //++K; // Not a part of calculations, but due to int rounding, seems to always be one day behind

        float hourWithChange = (float) (gmtTimeOfDay);
        float minsWithChange = (hourWithChange - Mathf.Floor(hourWithChange)) * 60;
        int secs = (int) ((minsWithChange - Mathf.Floor(minsWithChange)) * 60);

        hourWithChange += gmtToLocalConversion; // Convert to local time
        if (hourWithChange > 24) {
            hourWithChange -= 24;
            K += 1;
        }

        string dayMonthYear = K.ToString().PadLeft(2, '0') + "/" 
                                      + J.ToString().PadLeft(2, '0') 
                                      + "/" + I.ToString();
        
        string hourMinSec = ((int) hourWithChange).ToString().PadLeft(2, '0') + ":"
                                                  + ((int) minsWithChange).ToString().PadLeft(2, '0') + ":"
                                                  + secs.ToString().PadLeft(2, '0');

        return dayMonthYear + "\t" + hourMinSec;
    }
}
