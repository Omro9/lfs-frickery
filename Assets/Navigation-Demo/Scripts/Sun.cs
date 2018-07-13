using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to handle motion of sun relative to Earth. Also contains variables such as time multiplier and 
/// in-game time of day
/// </summary>
public class Sun : MonoBehaviour {
    public static int RADIUS = 200;
    public static double earthAngularVelocity = 7.2921159D / 100000D * 3600D * Mathf.Rad2Deg;   // In deg/hr
    public const float gameHoursPerRealSecond = 1 / 2F;
    private const float dayNightUpdateFreq = 24F / 300F;   // Number of hours per skybox redraw

    private GameObject player;
    private Vector3 playerPosition;

    public long julianDate;
    public long JD {
        get { return julianDate; }
    }

    private float jdSunrise, jdSunset;
    private int daysSinceCalcDayLength;

    private double timeOfDay;
    private double oldTimeOfDay;
    public double TimeOfDay {
        get { return timeOfDay; }
    }


    void Start()
    {
        player = GameObject.Find("Player");
        playerPosition = player.transform.position;

        // Find current julian Date
        System.DateTime now = System.DateTime.Now;
        julianDate = (long) (367 * now.Year - 7 * (now.Year + (now.Month + 9) / 12) / 4 + 275 * now.Month / 9 + now.Day + 1721013.5 
                             + now.ToUniversalTime().Hour / 24 - 0.5 * Mathf.Sign(100 * now.Year + now.Month - 190002.5F) + 0.5);
        timeOfDay = now.Hour + now.Minute / 60D + now.Second / 3600D; // Time of day in hours

        daysSinceCalcDayLength = 31;    // Currently unused, see below comment re:actual daylight times
    }

    void Update()
    {
        timeOfDay += Time.deltaTime * gameHoursPerRealSecond;   // Increment time of day
        if (timeOfDay > 24D) {
            timeOfDay -= 24D;
            ++julianDate;
            //++daysSinceCalcDayLength;
        }

        UpdateSunPosition();
        //AlignWithSkybox();
        FollowPlayer();

        /*
         * This calculates actual daylight times, but pretty computationally heavy imo and not really worth the time
         * 
        if (daysSinceCalcDayLength > 30)
        {
            float meanSolarTime = n - player.GetComponent<CanoeControls>().longitude / 360F;
            int meanSolarAnomaly = ((int) (357.5291F + 0.98560028F * meanSolarTime)) % 360;
            float center = 0.9148F * Mathf.Sin(meanSolarAnomaly * Mathf.Deg2Rad) + 0.02F * Mathf.Sin(2 * meanSolarAnomaly * Mathf.Deg2Rad) + 0.0003F * Mathf.Sin(3 * meanSolarAnomaly * Mathf.Deg2Rad);
            int eclipLong = ((int)(meanSolarAnomaly + center + 180 + 102.9372F)) % 360;
            float solarTransit = 2451545.5F + meanSolarTime + 0.0053F * Mathf.Sin(meanSolarAnomaly * Mathf.Deg2Rad) - 0.0069F * Mathf.Sin(2 * eclipLong * Mathf.Deg2Rad);
            float sinDec = Mathf.Sin(eclipLong * Mathf.Deg2Rad) * Mathf.Sin(23.44F * Mathf.Deg2Rad);
            float hourAngle = Mathf.Acos(-Mathf.Tan(player.GetComponent<CanoeControls>().latitude) * Mathf.Tan(Mathf.Asin(sinDec)));
            jdSunset = solarTransit + hourAngle / (2F * Mathf.PI);
            jdSunrise = solarTransit - hourAngle / (2F * Mathf.PI);

            daysSinceCalcDayLength = 0;
        }
        float lengthOfDaylight = jdSunset - jdSunrise;
        */
       
    }

   
    /// <summary>
    /// <para>Transform sun based on time of day.</para>
    /// Equatorial coordinate calculations taken from https://en.wikipedia.org/wiki/Position_of_the_Sun#Approximate_position,
    /// horizontal calculations taken from https://en.wikipedia.org/wiki/Celestial_coordinate_system#Equatorial_%E2%86%94_horizontal
    /// </summary>
    private void UpdateSunPosition() {
        // Calculate position of sun relative to Earth (equatorial coordinates)
        double n = julianDate - 2451545 + (timeOfDay - 12D) / 24D;
        float obliquity = (float)((23.439D - 0.0000004D * n) * Mathf.Deg2Rad);

        double meanLong = (280.46D + 0.9856474D * n) % 360D * Mathf.Deg2Rad;
        float g = (float) ((357.528D + 0.9856003D * n) % 360D * Mathf.Deg2Rad);
        float elipticLong = (float) (meanLong + 1.915D * Mathf.Deg2Rad * Mathf.Sin(g) + 0.02D * Mathf.Deg2Rad * Mathf.Sin(2F * g));// + Mathf.PI / 2F;

        float ra = Mathf.Atan2(Mathf.Cos(obliquity) * Mathf.Sin(elipticLong), Mathf.Cos(elipticLong));
        float dec = Mathf.Asin(Mathf.Sin(obliquity) * Mathf.Sin(elipticLong));
        //Debug.Log("RA: " + ra + "\nDEC: " + dec);

        // Transform equatorial coordinates to horizontal ones
        double T = (((timeOfDay < 12D) ? -0.5 : 0.5) + (julianDate - 2451545)) / 36525D; // This assumes timeOfDay is in GMT
        double T0 = 6.697374558D + (2400.051336D * T) + (0.000025862D * T * T) + (timeOfDay * 1.0027379093D);
        double GST = T0 % 24D;
        double LST = (GST + player.GetComponent<CanoeControls>().longitude / 15D) % 24D;    // In hours
        //Debug.Log(timeOfDay + "\t" + GST + "\t" + LST);

        float localHourAngle = (float)(LST - ra * Mathf.Rad2Deg / 15D) * 15F * Mathf.Deg2Rad;   // In rads
        float playerLatRads = player.GetComponent<CanoeControls>().latitude * Mathf.Deg2Rad;
        float azimuth = -Mathf.Atan2(Mathf.Cos(dec) * Mathf.Sin(localHourAngle),
                                    -Mathf.Sin(playerLatRads) * Mathf.Cos(dec) * Mathf.Cos(localHourAngle)
                                        + Mathf.Cos(playerLatRads) * Mathf.Sin(dec));
        float altitude = Mathf.Asin(Mathf.Sin(playerLatRads) * Mathf.Sin(dec)
                                    + Mathf.Cos(dec) * Mathf.Cos(playerLatRads) * Mathf.Cos(localHourAngle));
        //Debug.Log("\nAltitude:\t" + altitude * Mathf.Rad2Deg + "\t\tAzimuth:\t" + azimuth * Mathf.Rad2Deg);

        float horiZ = RADIUS * Mathf.Cos(azimuth) * Mathf.Cos(altitude);
        float horiY = RADIUS * Mathf.Sin(altitude);
        float horiX = RADIUS * Mathf.Sin(azimuth) * Mathf.Cos(altitude);
        Vector3 newPosition = new Vector3(horiX, horiY, horiZ);

        transform.position = newPosition + playerPosition;  // This line seems kind of redundant with FollowPlayer(), can organize better
    }

    /// <summary>
    /// Updates skybox's alpha blending of day/night skies.
    /// </summary>
    private void UpdateSkybox(){
        float output;
        float t = (float) timeOfDay;
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
    /// <para>Have GameObject follow position of player. Object is not made a child of player to preserve the object's rotation.</para>
    /// Probably should make parent class, as this method is shared among Moon.cs, Sun.cs, and(to an extent) in Orient.cs.
    /// </summary>
    private void FollowPlayer() {
        Vector3 delta = player.transform.position - playerPosition;
        transform.Translate(delta);
        playerPosition = player.transform.position;
        transform.LookAt(player.transform);
    }
}
