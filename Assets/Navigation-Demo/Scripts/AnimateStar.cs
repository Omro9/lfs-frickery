using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateStar : MonoBehaviour {

    private const float rotationSpeed = 0.01F;
    private const float growthSpeed = 0.1F;
    private const float growthAmplitude = 1.5F;
    private float initScale;
    private float t = 0;

	// Use this for initialization
	void Start () {
        transform.Rotate(0F, 45F, 45F);
        t = Random.Range(0F, 1000F);
        //t = Mathf.PerlinNoise(transform.position.x, transform.position.y) * 100F;
        initScale = transform.localScale.x;     // All components should be the same, so arbitrarily choosing x
	}
	
	// Update is called once per frame
	void Update () {
        t += Time.deltaTime * 0.75F;
        float noise = Mathf.PerlinNoise(t, transform.position.x);
        //transform.Rotate(0, Mathf.Sin(rotationSpeed * t), -Mathf.Sin(rotationSpeed * t), Space.World);
        float scaleFactor = (growthAmplitude - 1F) * noise + 0.5F;
        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
	}
}
