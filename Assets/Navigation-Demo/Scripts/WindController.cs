using UnityEngine;
using System.Collections;

public class WindController : MonoBehaviour
{
    private const float maxMagnitude = 60F; // In mph?
    private const float perlinSpeed = 0.03F;
    private const float angularRandoMagnitude = 15F;
    private const float timeScaleRandoMagnitude = 0.3F;
    private const float timeAngularVelocity = 1; // In deg/sec

    private static Vector3 wind;

    /// <summary>
    /// A vector representing the wind's direction and speed at the given instant.
    /// </summary>
    /// <value>The wind.</value>
    public static Vector3 Wind {
        get { return wind; }
    }

    private static Vector3 start, dest;
    private static float timeScale;
    private static float t;

    // Use this for initialization
    void Start()
    {
        wind = new Vector3(1, 0, 1);
        start = wind;
        dest = GenerateDestination();
        timeScale = 1;
        t = 1;
    }

    // Update is called once per frame
    void Update()
    {
        wind = Vector3.Lerp(start, dest, t / timeScale).normalized * GenerateMagnitude(t, wind.x);
        //wind = wind.normalized * GenerateMagnitude(t, timeScale);
        t += Time.deltaTime * timeAngularVelocity;
        //Debug.Log("\nWind:\t" + wind.normalized + "\t\tDest:\t" + dest + "\t\tPercent:\t" + t / timeScale);
        Debug.Log("\nWind vector:\t" + wind + "\t\tWind speed:\t" + wind.magnitude);
        if (t / timeScale >= 1) {
            start = dest;
            dest = GenerateDestination();
            timeScale = Mathf.Abs(Vector3.Angle(start, dest));
            t = 0;
        }
    }

    /// <summary>
    /// Generates a unit vector representing the direction the wind vector will lerp to.
    /// </summary>
    /// <returns>The destination with a magnitude of 1.</returns>
    private Vector3 GenerateDestination() {
        float angularDisplacement = Random.Range(-1, 1) * angularRandoMagnitude;
        Quaternion rotation = Quaternion.AngleAxis(angularDisplacement, Vector3.up);
        return (rotation * wind).normalized;
    }

    /// <summary>
    /// Generates wind speed based on Perlin noise with a maximum value of <c>WindController.magnitude</c>.
    /// </summary>
    /// <returns>The magnitude as a float.</returns>
    /// <param name="modx">First parameter of Perlin noise function.</param>
    /// <param name="mody">Second parameter of Perlin noise function.</param>
    private float GenerateMagnitude(float modx, float mody)
    {
        float result = maxMagnitude * Mathf.Pow(Mathf.PerlinNoise(modx * perlinSpeed, mody * perlinSpeed), 1);
        //Debug.Log("\nMagnitude:\t" + result);
        return result;
    }
}
