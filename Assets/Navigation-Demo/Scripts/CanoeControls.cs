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

    private const float m_pushForce = 10f;
    private readonly Vector3 m_eulerAngleVelocity = new Vector3(0, 8f, 0);
    private Rigidbody m_rigidbody;

    private Camera eyeCamera;
    private Animator anim;
    private Canvas clockUI;
    private const float uiControllerDistance = 0.25F;

    // Begin skybox variable additions
    private const float globalVelocity = 0.03F; // PLACEHOLDER angular velocity in radians/meter traveled
    private Vector3 globalPosition = new Vector3(144F, 0, 13F);  // z represents latitude, x represents longitude. AKA z is ~North
    public float Latitude
    {
        get { return globalPosition.z; }
    }
    public float Longitude
    {
        get { return globalPosition.x; }
    }
    // End skybox variable additions

    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        eyeCamera = Camera.main;
        clockUI = Instantiate(Resources.Load<Canvas>("Worldspace UI"));
        clockUI.worldCamera = eyeCamera;
        clockUI.enabled = false;
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
            Vector3 deltaPosition = transform.forward * m_pushForce * Time.deltaTime;
            m_rigidbody.MovePosition(transform.position + deltaPosition);

            // Register change in lat/long
            deltaPosition = new Vector3(deltaPosition.x, 0, deltaPosition.z).normalized * globalVelocity;
            globalPosition += deltaPosition;

            // Adjust rotation based on the angular difference between the controller and the canoe
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0f, m_leftController.transform.localRotation.eulerAngles.y, 0f), Time.deltaTime);

            if (Vector3.Angle(transform.eulerAngles, new Vector3(0f, m_leftController.transform.localRotation.eulerAngles.y, 0f)) < 0)
                anim.Play("Forward");
            else anim.Play("Reverse");
        }

        // Show timeofday ui
        if (m_rightController.gripped) {
            string timeFormatted = Sun.GetTimeOfDayFormatted();
            clockUI.GetComponentInChildren<UnityEngine.UI.Text>().text = timeFormatted;

            clockUI.transform.position = m_rightController.transform.position + m_rightController.transform.forward * uiControllerDistance;
            clockUI.transform.LookAt(eyeCamera.transform, Vector3.up);
            clockUI.transform.Rotate(clockUI.transform.up * 180F);
            clockUI.enabled = true;
        }
        else clockUI.enabled = false;
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
            anim.Play("Forward");
            Vector3 deltaPosition = transform.forward * -globalVelocity * Time.deltaTime;
            globalPosition += deltaPosition;
        }
        if (Input.GetKey(KeyCode.S))
        {
            m_rigidbody.MovePosition(transform.position + (transform.forward * m_pushForce * Time.deltaTime));
            anim.Play("Reverse");
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
        if (Input.GetKey(KeyCode.F))
        {
            string timeFormatted = Sun.GetTimeOfDayFormatted();
            clockUI.GetComponentInChildren<UnityEngine.UI.Text>().text = timeFormatted;

            clockUI.transform.position = eyeCamera.transform.position + eyeCamera.transform.forward * uiControllerDistance;//targetCamera.transform.TransformPoint(uiControllerDistance);
            clockUI.transform.LookAt(eyeCamera.transform);
            clockUI.transform.Rotate(Vector3.up * 180F);
            clockUI.enabled = true;
        }
        else clockUI.enabled = false;
    }
}
