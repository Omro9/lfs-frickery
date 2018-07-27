using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Short script to make instantiated stars rotate and scale to simulate twinkling
 */
public class AnimateStar : MonoBehaviour {
    public static int RADIUS = 200;
    public static float daytimeScaleEffect = 1F;
   
    public float luminance;
    public Vector3 position;

    private static float rotationSpeed = 0.01F;
    private static float growthSpeed = 0.75F;
    private static float growthAmplitude = 1.5F;
    //public static Vector3 initScale;
    public Vector3 initScale;

    private GameObject player;
    private Vector3 playerPosition;
    private float t;

	// Use this for initialization
	void Start () {
        player = GameObject.Find("Player");
        playerPosition = player.transform.position;
        transform.position = position;
        transform.localScale *= luminance;
        initScale = transform.localScale;
        t = Random.Range(0F, 1000F);

        transform.LookAt(player.transform);
	}
	
	// Update is called once per frame
	void Update () {
        t += Time.deltaTime * growthSpeed;    // Adjust speed of animation
        float noise = Mathf.PerlinNoise(t, transform.localScale.magnitude);   // Perlin noise to add smooth randomness
        float scaleFactor = (growthAmplitude - 1F) * noise + 0.5F;
        transform.localScale = initScale * scaleFactor * daytimeScaleEffect;

        transform.RotateAround(playerPosition,
                               SkyboxController.North,
                               (float) (Time.deltaTime * SkyboxController.gameHoursPerRealSecond * SkyboxController.earthAngularVelocity));
        
        FollowPlayer();
	}

    private void FollowPlayer() {
        Vector3 delta = player.transform.position - playerPosition;
        transform.Translate(delta);
        playerPosition = player.transform.position;
    }
}
