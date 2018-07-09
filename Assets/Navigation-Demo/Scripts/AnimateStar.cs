﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Short script to make instantiated stars rotate and scale to simulate twinkling
 */
public class AnimateStar : MonoBehaviour {
    public static int RADIUS = 700;
    public static float daytimeScaleEffect = 1F;
   
    public float luminance;
    public Vector3 position;

    private static float rotationSpeed = 0.01F;
    private static float growthSpeed = 0.75F;
    private static float growthAmplitude = 1.5F;
    //public static Vector3 initScale;
    public static float initScale;

    private GameObject skyboxCamera;
    private Vector3 skyboxCameraPosition;
    private float t = 0;

	// Use this for initialization
	void Start () {
        skyboxCamera = GameObject.Find("Skybox Camera");
        skyboxCameraPosition = skyboxCamera.transform.position;
        transform.position = position;
        //transform.localScale *= luminance;
        GetComponent<LensFlare>().brightness *= luminance;
        transform.LookAt(skyboxCameraPosition);
        transform.Rotate(0F, 45F, 45F);
        t = Random.Range(0F, 1000F);
        //t = Mathf.PerlinNoise(transform.position.x, transform.position.y) * 100F;
        //initScale = transform.localScale;
        initScale = GetComponent<LensFlare>().brightness;
	}
	
	// Update is called once per frame
	void Update () {
        t += Time.deltaTime * growthSpeed;    // Adjust speed of animation
        float noise = Mathf.PerlinNoise(t, transform.position.x);   // Perlin noise to add smooth randomness
        float scaleFactor = (growthAmplitude - 1F) * noise + 0.5F;
        //transform.localScale = initScale * scaleFactor * daytimeScaleEffect;
        GetComponent<LensFlare>().brightness = initScale * scaleFactor * daytimeScaleEffect;

        followCamera();
	}

    private void followCamera() {
        Vector3 delta = skyboxCamera.transform.position - skyboxCameraPosition;
        transform.Translate(delta);
        skyboxCameraPosition = skyboxCamera.transform.position;
    }
}
