using UnityEngine;

// This class is responsible for player controls *FOR TESTING PURPOSES*
// that includes testing on desktop or in a VR space
public class NavigationControls : MonoBehaviour {

    private Vector3 m_eulerAngleVelocity = new Vector3(0, 8f, 0);
    private Rigidbody m_rigidbody;

    private float m_pushForce = 10000f;
    

    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

	void Update()
    {
        KeyboardNavigation();
    }

    private void KeyboardNavigation()
    {
        // WASD or arrow Keys:
        // W - forward
        // s - backward
        // a - turn left
        // d - turn right
        if (Input.GetKey(KeyCode.W))
            m_rigidbody.AddForce(-transform.forward * m_pushForce);

        if (Input.GetKey(KeyCode.S))
            m_rigidbody.AddForce(transform.forward * m_pushForce);

        if (Input.GetKey(KeyCode.A))
        {
            Quaternion deltaRotation = Quaternion.Euler(m_eulerAngleVelocity * -Time.deltaTime);
            m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);
        }

        if (Input.GetKey(KeyCode.D))
        {
            Quaternion deltaRotation = Quaternion.Euler(m_eulerAngleVelocity * Time.deltaTime);
            m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);
        }
    }
}
