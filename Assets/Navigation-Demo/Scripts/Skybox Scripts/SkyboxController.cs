using UnityEngine;
using System.Collections;

public class SkyboxController : MonoBehaviour
{
    private GameObject player;
    private Material skybox;
    private static Transform rotationRef;
    public Transform Transform {
        get { return rotationRef; }
    }
    /// <summary>
    /// Vector along Earth's orbital axis pointing North, in world space
    /// </summary>
    /// <value>The North vector.</value>
    public static Vector3 North {
        get { return rotationRef.up.normalized; }
    }

    private float playerLat, playerLong;
    private Quaternion playerRotation;
    private Vector3 playerPosition;
    private Vector3 daytimeRotation;


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        skybox = RenderSettings.skybox;
        rotationRef = new GameObject("Skybox Rotation Reference").transform;

        playerLat = 90;    // Initial position of skybox, or, the point on the skydome directly above one's head at the north pole
        playerLong = 0;

        StarInstancer.CreateFromFile("big_dipper.txt");
        Update();

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
        // Update model's rotation on player's lat/long
        float latDelta = player.GetComponent<CanoeControls>().Latitude - playerLat;
        float longDelta = player.GetComponent<CanoeControls>().Longitude - playerLong;
        rotationRef.Rotate(0, -longDelta, -latDelta, Space.Self);   // Rotate around x-axis to change lat, rotate around y-axis to change long
        playerLat = player.GetComponent<CanoeControls>().Latitude;
        playerLong = player.GetComponent<CanoeControls>().Longitude;

        // Update model's rotation on time of day
        rotationRef.Rotate(Vector3.up, (float) (Time.deltaTime * Sun.gameHoursPerRealSecond * Sun.earthAngularVelocity), Space.Self);

        // Apply rotation from model
        Vector3 eulerAngles = -rotationRef.localEulerAngles;
        //Debug.Log(eulerAngles);
        skybox.SetFloat("_XRotation", eulerAngles.x);
        skybox.SetFloat("_YRotation", eulerAngles.y);
        skybox.SetFloat("_ZRotation", eulerAngles.z);
    }

    /// <summary>
    /// Updates skybox's alpha blending of day/night skies based on sun's altitude
    /// </summary>
    /// <param name="altitude">Altitude of sun.</param>
    public static void BlendSkyboxes(float altitude)
    {
        if (altitude >= 0)
            RenderSettings.skybox.SetFloat("_BlendCubemaps", 0.5F + (altitude / 90F * 0.5F));
        else
            RenderSettings.skybox.SetFloat("_BlendCubemaps", 0.5F - (altitude / -70F * 0.5F));
    }

    public static void SetDayTexTint(float altitude) {
        Color eveTint = Color.Lerp(Color.grey, 0.9F * (0.63F * Color.red + 0.37F * Color.yellow), 
                                   Mathf.Pow(1 - Mathf.Abs(altitude) / 85F, 3));
        RenderSettings.skybox.SetColor("_Tint", eveTint);
    }

    public static void SetNightTexTint(Color tint)
    {
        RenderSettings.skybox.SetColor("_Tint2", tint);
    }

    /// <summary>
    /// Blends alpha values of skybox textures based on time of day, uses an easing function defined in
    /// <see cref="Ease(float, float, float, float)"/>.
    /// </summary>
    private void BlendSkyboxes() {
        float alphaBlend;
        float t = (float) Sun.TimeOfDay;

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

    /// <summary>
    /// <para>Finds new player position vector and old position vector, crosses them to find axis of rotation,
    /// then uses Vector3.SignedAngle() to calculate change in degrees of rotation along axis by which to rotate skybox.
    /// </para> 
    /// This can probably be run only on occasion, as lat/long won't change significantly frame-to-frame.
    /// </summary>
    [System.Obsolete("Remnant from usage of camera to render skybox. Function now folded into Update().")]
    private void UpdateOnGlobalPosition()
    {
        float curPlayerLat = player.GetComponent<CanoeControls>().Latitude;
        float curPlayerLong = player.GetComponent<CanoeControls>().Longitude;
        Vector3 v = new Vector3(Mathf.Cos(playerLat) * Mathf.Cos(playerLong),
                                Mathf.Sin(playerLat),
                                Mathf.Cos(playerLat) * Mathf.Sin(playerLong));
        Vector3 w = new Vector3(Mathf.Cos(curPlayerLat) * Mathf.Cos(curPlayerLong),
                                Mathf.Sin(curPlayerLat),
                                Mathf.Cos(curPlayerLat) * Mathf.Sin(curPlayerLong));
        Vector3 axis = Vector3.Cross(v, w);
        float angle = Vector3.SignedAngle(v, w, axis);
        transform.Rotate(axis, angle, Space.World);
        playerLat = curPlayerLat;
        playerLong = curPlayerLong;
    }
}
