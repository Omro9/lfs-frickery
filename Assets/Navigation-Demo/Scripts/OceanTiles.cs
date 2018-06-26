using UnityEngine;

// This class is responsible for procedurally generating ocean tiles that are in range of the player's
// sight. Because it is not intuitive to render an entire ocean, it is more applicable to create and 
// destroy tiles of 50 by 50 that are closest to the player giving the illusion that the entire
// ocean is being rendered.
public class OceanTiles : MonoBehaviour {

    public Transform m_canoe;
    public Transform m_tile;
    
    private float m_distanceThreshold = 10f ;    // Cannot be lower than the size of the tiles

    void Update()
    {
        UpdateDistances();
    }

    /*
     * Check all surrounding tile's distances if a tile change needs to occur
     */
    private void UpdateDistances()
    {
        double m_distance = Vector3.Distance(m_canoe.position, m_tile.position);

        if(m_distance > m_distanceThreshold)
        {
            m_tile.position = m_canoe.position;
        }
        
    }
}
