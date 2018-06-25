using UnityEngine;

// This class is responsible for player controls *FOR TESTING PURPOSES*
// that includes testing on desktop or in a VR space
public class CanoeControls : MonoBehaviour
{
    public bool m_isVR;
    public SteamVR_ControllerManager m_steamVRManager;
    
    private Vector3 m_eulerAngleVelocity = new Vector3(0, 8f, 0);
    private SteamVR_TrackedController m_rightController;
    private SteamVR_TrackedController m_leftController;
    private Rigidbody m_rigidbody;

    private float m_pushForce = 10f;

    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        if (m_steamVRManager == null)
        {
            m_isVR = false;
        } else
        {
            m_leftController = m_steamVRManager.left.GetComponent<SteamVR_TrackedController>();
            m_rightController = m_steamVRManager.right.GetComponent<SteamVR_TrackedController>();
        }
    }

    void Update()
    {
        if (!m_isVR)
            KeyboardNavigation();
        else
            VRNavigation();
    }

    private void VRNavigation()
    {
        Debug.Log("Left Controller Trigger: " + m_leftController.triggerPressed);
    }

    private void KeyboardNavigation()
    {
        // WASD or arrow Keys:
        // W - forward
        // s - backward
        // a - turn left
        // d - turn right
        if (Input.GetKey(KeyCode.W))
            transform.Translate(Vector3.forward * -m_pushForce * Time.deltaTime, Space.Self);

        if (Input.GetKey(KeyCode.S))
            transform.Translate(Vector3.forward * m_pushForce * Time.deltaTime, Space.Self);

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
