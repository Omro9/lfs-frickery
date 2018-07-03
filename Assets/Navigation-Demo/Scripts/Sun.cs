using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script to handle motion of sun relative to Earth. Also defines constants such as time multiplier and in-game time
 */
public class Sun : MonoBehaviour {
    public const float gameHoursPerRealSecond = 2F;
    private const float dayNightUpdateFreq = 24F / 300F;   // Number of hours per skybox redraw

    public Camera skyboxCamera;
    private GameObject player;

    private float m_sunRadius = 200f;
    private Vector3 oldPosition;

    public double julianDate = 2458297F;    // 12:00 AM June 28, 2018

    private float jdSunrise, jdSunset;
    private int daysSinceCalcDayLength;

    private float timeOfDay;
    public float TimeOfDay {
        get { return timeOfDay; }
    }
    private float oldTimeOfDay;


    void Start()
    {
        player = GameObject.Find("Player");
        timeOfDay = 0F; // Time of day in hours
        transform.position = new Vector3(0f, m_sunRadius, 0f);
        oldPosition = transform.position;
        daysSinceCalcDayLength = 31;
    }

    void Update()
    {
        timeOfDay += Time.deltaTime * gameHoursPerRealSecond;   // Increment time of day
        if (timeOfDay > 24F) {
            timeOfDay -= 24F;
            //++daysSinceCalcDayLength;
        }
        julianDate += Time.deltaTime * gameHoursPerRealSecond / 24F;

        // Transform sun based on time of day
        // Calculations taken from https://en.wikipedia.org/wiki/Position_of_the_Sun#Approximate_position
        float n = (float) (julianDate - 2451545);
        float epsilon = 23.439F - 0.0000004F * n * Mathf.Deg2Rad;

        float meanLong = (280.46F + 0.9856474F * n) % 360F;
        float g = (357.528F + 0.9856003F * n) % 360F;
        float sunLong = (meanLong + 1.915F * Mathf.Sin(g) + 0.02F * Mathf.Sin(2F * g)) * Mathf.Deg2Rad + Mathf.PI / 2F;

        float x = m_sunRadius * Mathf.Cos(sunLong);
        float z = m_sunRadius * Mathf.Cos(epsilon) * Mathf.Sin(sunLong);
        float y = m_sunRadius * Mathf.Sin(epsilon) * Mathf.Sin(sunLong);

        transform.position = new Vector3(x, y, z);
        //transform.Translate(new Vector3(x, y, z) - oldPosition);    // This and the above line should be exchanged if not rotating sun
        transform.LookAt(skyboxCamera.transform);
        //oldPosition = new Vector3(x, y, z);                         // re:above - also this line
        /*
         * If the lines above are not commented out, sun rotates around as an object, rather than being fixed like the 
         * stars. However this doesn't seem to work with the water reflections.
         */ 


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

        if (Mathf.Abs(timeOfDay - oldTimeOfDay) > dayNightUpdateFreq) // Updates day/night cycle every few frames
        {
            float t = timeOfDay;
            float output;
            if (timeOfDay < 12F) {  // These conditionals are a naive way to apply then reverse the easing function
                output = ease(t, 0F, 1F, 12F);
            }
            else {
                output = 1 - ease(t - 12F, 0F, 1F, 12F);
            }

            RenderSettings.skybox.SetFloat("_BlendCubemaps", output);   // Shader for blending alpha channels of two cubemaps
            oldTimeOfDay = timeOfDay;
        }
       
    }

    /*
     * A quintic in/out easing function taken from http://www.timotheegroleau.com/
     */
    private float ease(float t, float b, float c, float d) {
        float ts = (t /= d) * t;    // aka t = t / d; ts = t^2; ts=="t squared"
        float tc = ts * t;          // aka tc = t^3;            tc=="t cubed"
        return b + c * (6F * tc * ts + -15F * ts * ts + 10F * tc); // aka begin + delta*(6*t^5-15*t^4+10t^3)
    }

}
