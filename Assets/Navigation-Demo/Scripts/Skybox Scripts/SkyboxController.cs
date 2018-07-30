using UnityEngine;
using System.Collections;

public class SkyboxController : MonoBehaviour
{
    public const double earthAngularVelocity = 7.2921159D / 100000D * 3600D * Mathf.Rad2Deg;   // In deg/hr
    public static float gameHoursPerRealSecond = 1 / 2F;
    public static bool isDaytime;

    private kode80.Clouds.kode80Clouds clouds;
    private GameObject player;
    private float playerLat, playerLong;

    private Material skybox;
    private static Transform rotationRef;
    public Transform Transform {
        get { return rotationRef; }
    }
    /// <summary>
    /// Unit vector along Earth's orbital axis pointing North, in world space
    /// </summary>
    /// <value>The North vector.</value>
    public static Vector3 North {
        get { return rotationRef.up.normalized; }
    }

    public static long julianDate;
    public static long JD
    {
        get { return julianDate; }
    }

    private static double gmtTimeOfDay;    // In GMT
    private static int gmtToLocalConversion = 10;  // Guam is GMT + 10
    /// <summary>
    /// Gets the time of day local to Guam.
    /// </summary>
    /// <value>The local time of day.</value>
    public static double LocalTimeOfDay
    {
        get { return (gmtTimeOfDay + gmtToLocalConversion) % 24; }
    }
    private double oldTimeOfDay;
    /// <summary>
    /// Gets the time of day in GMT.
    /// </summary>
    /// <value>The time of day.</value>
    public static double TimeOfDay
    {
        get { return gmtTimeOfDay; }
    }

    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        clouds = GameObject.Find("Clouds").GetComponent<kode80.Clouds.kode80Clouds>();
        skybox = RenderSettings.skybox;
        rotationRef = new GameObject("Skybox Rotation Reference").transform;
        rotationRef.SetParent(transform);

        // Find current julian Date
        System.DateTime nowGMT = System.DateTime.Now.ToUniversalTime();
        julianDate = (long)(367 * nowGMT.Year - 7 * (nowGMT.Year + (nowGMT.Month + 9) / 12) / 4 + 275 * nowGMT.Month / 9 + nowGMT.Day + 1721013.5
                             + nowGMT.ToUniversalTime().Hour / 24 - 0.5 * Mathf.Sign(100 * nowGMT.Year + nowGMT.Month - 190002.5F) + 0.5);
        gmtTimeOfDay = nowGMT.Hour + nowGMT.Minute / 60D + nowGMT.Second / 3600D; // Time of day in hours


        playerLat = 90;    // Initial position of skybox, or, the point on the skydome directly above one's head at the north pole
        playerLong = 0;
        StarInstancer.CreateFromFile("big_dipper.txt"); // Make stars from file
        clouds.coverageOffsetPerFrame = new Vector2(0.01F, 0.07F);  // Arbitrary values
        Update();

        Instantiate(Resources.Load<GameObject>("Sun")).transform.SetParent(transform); // Sun must be instantiated before moon
        Instantiate(Resources.Load<GameObject>("Moon")).transform.SetParent(transform);

        // Rotate islands around
        // Or not, because apparently terrain is static
        /*
        Vector3 northProj = Vector3.ProjectOnPlane(SkyboxController.North, Vector3.up).normalized;
        float rotation = Vector3.Angle(Vector3.forward, northProj);
        Transform islands = GameObject.Find("Islands").transform;
        for (int i = 0; i < islands.childCount; ++i) {
            Transform island = islands.GetChild(i);
            island.RotateAround(Vector3.zero, Vector3.up, rotation);   // Islands should be initially oriented as if z-axis were North
        }  
        */
    }

    // Update is called once per frame
    void Update()
    {
        // Increment time of day
        gmtTimeOfDay += Time.deltaTime * gameHoursPerRealSecond;   
        if (gmtTimeOfDay > 24D) {
            gmtTimeOfDay -= 24D;
            ++julianDate;
        }

        // Update model's rotation on player's lat/long
        float latDelta = player.GetComponent<CanoeControls>().Latitude - playerLat;
        float longDelta = player.GetComponent<CanoeControls>().Longitude - playerLong;
        rotationRef.Rotate(0, -longDelta, -latDelta, Space.Self);   // Rotate around x-axis to change lat, rotate around y-axis to change long
        playerLat = player.GetComponent<CanoeControls>().Latitude;
        playerLong = player.GetComponent<CanoeControls>().Longitude;

        // Update model's rotation on time of day
        rotationRef.Rotate(Vector3.up, (float) (Time.deltaTime * SkyboxController.gameHoursPerRealSecond * SkyboxController.earthAngularVelocity), Space.Self);

        // Apply rotation from model
        Vector3 eulerAngles = -rotationRef.localEulerAngles;
        //Debug.Log(eulerAngles);
        skybox.SetFloat("_XRotation", eulerAngles.x);
        skybox.SetFloat("_YRotation", eulerAngles.y);
        skybox.SetFloat("_ZRotation", eulerAngles.z);

    }

    /// <summary>
    /// Updates skybox's alpha blending of day/night skies based on sun's altitude.
    /// </summary>
    /// <param name="altitude">Altitude of sun in degrees.</param>
    public static void BlendSkyboxes(float altitude)
    {
        if (altitude >= 0)
            RenderSettings.skybox.SetFloat("_BlendCubemaps", 0.5F + (altitude / 90F * 0.5F));
        else
            RenderSettings.skybox.SetFloat("_BlendCubemaps", 0.5F - (altitude / -70F * 0.5F));
        AnimateStar.daytimeScaleEffect = 1 - Mathf.Round(RenderSettings.skybox.GetFloat("_BlendCubemaps"));
    }

    public static void SetDayTexTint(float altitude) {
        Color eveTint = Color.Lerp(Color.grey, 0.9F * (0.63F * Color.red + 0.37F * Color.yellow), Mathf.Pow(1 - Mathf.Abs(altitude) / 85F, 3));
        RenderSettings.skybox.SetColor("_Tint", eveTint);
    }

    public static void SetNightTexTint(Color tint)
    {
        RenderSettings.skybox.SetColor("_Tint2", tint);
    }

    /// <summary>
    /// <para>Gets the time of day in a readable format.</para>
    /// Calculations from http://aa.usno.navy.mil/faq/docs/JD_Formula.php
    /// </summary>
    /// <returns>A string of the format "DD/MM/YYYY HH:MM:SS".</returns>
    public static string GetTimeOfDayFormatted()
    {
        int N, L, I, J, K;
        L = (int)(julianDate + 68569);
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

        float hourWithChange = (float)(gmtTimeOfDay);
        float minsWithChange = (hourWithChange - Mathf.Floor(hourWithChange)) * 60;
        int secs = (int)((minsWithChange - Mathf.Floor(minsWithChange)) * 60);

        hourWithChange += gmtToLocalConversion; // Convert to local time
        if (hourWithChange > 24)
        {
            hourWithChange -= 24;
            K += 1;
        }

        string dayMonthYear = K.ToString().PadLeft(2, '0') + "/" + J.ToString().PadLeft(2, '0') + "/" + I.ToString();
        string hourMinSec = ((int)hourWithChange).ToString().PadLeft(2, '0') + ":"
                                                  + ((int)minsWithChange).ToString().PadLeft(2, '0') + ":"
                                                  + secs.ToString().PadLeft(2, '0');
        return dayMonthYear + "\t" + hourMinSec;
    }

    /// <summary>
    /// Blends alpha values of skybox textures based on time of day, uses an easing function defined in
    /// <see cref="Ease(float, float, float, float)"/>.
    /// </summary>
    private void BlendSkyboxes() {
        float alphaBlend;
        float t = (float) SkyboxController.TimeOfDay;

        if (t < 12F)
        {  // These conditionals are a naive way to apply, then reverse the easing function
            alphaBlend = Ease(t, 0F, 1F, 12F);
        }
        else
        {
            alphaBlend = 1 - Ease(t - 12F, 0F, 1F, 12F);
        }

        skybox.SetFloat("_BlendCubemaps", alphaBlend);   // Shader for blending alpha channels of two cubemaps
        AnimateStar.daytimeScaleEffect = 1F - Mathf.Round(alphaBlend);  // Turns stars off if skyboxes are blended more than 0.5
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
}
