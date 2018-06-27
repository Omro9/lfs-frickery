using UnityEngine;

// This class is responsible for swapping the camera views of a desired game object and facing them at the canoe.
// This offers a differentpersective outside of what is possible being some distance away from the canoe
// in the middle of the ocean
public class CameraViews : MonoBehaviour {

    public Transform m_camera;
    public Transform[] m_cameraViews;

    private int m_currentView;

	void Start ()
    {
        m_currentView = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchView();
        }
    }

    private void SwitchView()
    {
        m_camera.transform.localPosition = m_cameraViews[m_currentView].localPosition;
        m_camera.transform.localRotation = m_cameraViews[m_currentView].localRotation;
        
        // Switch the view for next time
        m_currentView++;
        m_currentView = m_currentView % m_cameraViews.Length;
    }
	
}
