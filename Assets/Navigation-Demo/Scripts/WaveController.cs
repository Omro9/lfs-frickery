using UnityEngine;
using System.Collections;

public class WaveController : MonoBehaviour
{


    private Material mat;
    Vector4 defaultAmp, defaultFreq, defaultSteep, defaultBumpDirection;

    // Use this for initialization
    void Start()
    {
        mat = GameObject.Find("Tile").GetComponent<Renderer>().material;

        defaultAmp = mat.GetVector("_GAmplitude");
        defaultFreq = mat.GetVector("_GFrequency");
        defaultSteep = mat.GetVector("_GSteepness");
        defaultBumpDirection = mat.GetVector("_BumpDirection");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 wind = WindController.Wind;
        float mag = wind.magnitude / 60F + 1;   // Wind magnitude as a percent of max magnitude, offset so default shader settings are 0%
        mag *= mag; // Square it because why not

        // The water tile uses the FXWater4Advanced shader. These shader properties make it really easy to implement waves
        // NOTE: I had to change the properties off of 'uniform' in-shader, so that may break something down the line

        Vector4 wdir = new Vector4(wind.x, wind.z, 1F, 1F);
        Vector4 wamp = defaultAmp * mag;
        //wamp = new Vector4(0.5F, 0, 0, 0);
        Vector4 wfreq = defaultFreq * (1F / mag) / 2.2F;
        //wfreq = new Vector4(1, 0, 0, 0);
        Vector4 wsteep = defaultSteep * (1F / wind.magnitude) * 5F;
        //wsteep = new Vector4(1F, 0, 0, 0);
        Vector4 wbumpdir = new Vector4(wind.x, 1F, wind.z, 1F) * mag;


        mat.SetVector("_GFrequency", wfreq);
        mat.SetVector("_GAmplitude", wamp);
        mat.SetVector("_GDirectionAB", wdir);
        //mat.SetVector("_GDirectionCD", Vector4.zero);
        mat.SetVector("_BumpDirection", wbumpdir);
        mat.SetVector("_GSteepness", wsteep);

    }
}
