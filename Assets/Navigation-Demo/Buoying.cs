using UnityEngine;

// This script sustain's a buoyancy for an object with a rigid body.
// If the object passed througha a certain threshold on the y-axis, a certain force will be applied giving
// the illusion that there is buoyancy
public class Buoying : MonoBehaviour {

    public float m_waterLevel = 0.0f;
    public float m_waterDensity = 2.0f;    // Changing factor when observing object buoyancy
    public float m_downForce = 1.0f;

    private float m_height;
    private Vector3 m_floatForce;
    private Vector3 m_forceGravity;
    private Rigidbody m_rigidbody;

    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_forceGravity = -Physics.gravity * m_rigidbody.mass;
    }
	
	void Update () {
        m_height = transform.position.y - m_waterLevel;

        if(m_height < 0.0f)
        {
            // Force = mass * acceleration * buoyancy force
            m_floatForce = -m_forceGravity * m_height * m_waterDensity;
            m_floatForce += new Vector3(0, -m_downForce, 0);
            m_rigidbody.AddForceAtPosition(m_floatForce, transform.position);
        }
       
	}
}
