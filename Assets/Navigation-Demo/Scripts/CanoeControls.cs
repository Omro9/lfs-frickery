using UnityEngine;

// This class is responsible for player controls *FOR TESTING PURPOSES*
// that includes testing on desktop or in a VR space
public class CanoeControls : MonoBehaviour
{
    private const int SCALE = 2000; // Scale of 2000 brings the canoe to about 20ft long, is that right?
                                    // Feels more realistic to me at around 1200 factor

    // Mode is the type of controls the the player will use
    // Computer - desktop gameplay WASD controls for testing and audio is through the computer
    // VR_Simple - Point and click to go for testing and audio is through the Vive headset
    // VR_Realistic - realism for the actual game with wind forcing the canoe
    public enum Mode { Computer, VR_Simple, VR_Realistic }
    public Mode m_mode;
    
    public SteamVR_TrackedController m_leftController;
    public SteamVR_TrackedController m_rightController;

    private Vector3 m_eulerAngleVelocity = new Vector3(0, 8f, 0);
    private Rigidbody m_rigidbody;

    private float m_pushForce = 10f;

    // Begin skybox variable additions
    private const float globalVelocity = 0.15F; // PLACEHOLDER angular velocity in radians/frame
    private Vector3 globalPosition = new Vector3(13F, 0, 144F);  // x represents latitude, z represents longitude
    public float Latitude
    {
        get { return globalPosition.x; }
    }
    public float Longitude
    {
        get { return globalPosition.z; }
    }
    // End skybox variable additions

    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        switch (m_mode)
        {
            case Mode.Computer:
                KeyboardNavigation();
                break;
            case Mode.VR_Simple:
                VRPointer();
                break;
            case Mode.VR_Realistic:
                break;

        }
    }

    /*
     * VRPointer is a simple "point there go there" controller
     */
    private void VRPointer()
    {
        // Controlled by the left controller
        if (m_leftController.triggerPressed)
        {
            // Adjust position
            m_rigidbody.MovePosition(transform.position + (transform.forward * -m_pushForce * Time.deltaTime));

            // Adjust rotation based on the angular difference between the controller and the canoe
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0f, m_leftController.transform.localRotation.eulerAngles.y, 0f), Time.deltaTime);
        }
    }

    private void KeyboardNavigation()
    {
        // WASD or arrow Keys:
        // W - forward
        // s - backward
        // a - turn left
        // d - turn right
        if (Input.GetKey(KeyCode.W))
        {
            m_rigidbody.MovePosition(transform.position + (transform.forward * -m_pushForce * Time.deltaTime));
            Vector3 deltaPosition = transform.forward * -globalVelocity * Time.deltaTime;
            globalPosition += deltaPosition;
        }
        if (Input.GetKey(KeyCode.S))
        {
            m_rigidbody.MovePosition(transform.position + (transform.forward * m_pushForce * Time.deltaTime));
            Vector3 deltaPosition = transform.forward * globalVelocity * Time.deltaTime;
            globalPosition += deltaPosition;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Quaternion deltaRotation = Quaternion.Euler(m_eulerAngleVelocity * -Time.deltaTime * 10f);
            m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);
        }

        if (Input.GetKey(KeyCode.D))
        {
            Quaternion deltaRotation = Quaternion.Euler(m_eulerAngleVelocity * Time.deltaTime * 10f);
            m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);
        }
    }
}
