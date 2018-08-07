using UnityEngine;
using System.Collections;

public class TerrainWaves : MonoBehaviour
{
    private const float waveSpeed = 0.01F;
    private const float waveAmplitude = 1F;
    private const int numSamples = 100;
    private readonly Vector2Int[] MooreNeighbor =
        {   new Vector2Int(0, 1),   // E
            new Vector2Int(1, 1),   // SE
            new Vector2Int(1, 0),   // S
            new Vector2Int(1, -1),  // SW
            new Vector2Int(0, -1),  // W
            new Vector2Int(-1, -1), // NW
            new Vector2Int(-1, 0),  // N
            new Vector2Int(-1, 1)   // NE 
        };

    private TerrainData terrainData;
    private GameObject player;
    private Mesh waterTileMesh;
    private Vector3[] baseHeights;
    private TerrainBody[] landmasses;
    private float[,] heights;
    private int size;
    public int NumLandmasses {
        get { return size; }
    }
    public TerrainBody[] Landmasses {
        get { return landmasses; }
    }

    // Use this for initialization
    void Start()
    {
        waterTileMesh = GameObject.Find("Tile").GetComponent<MeshFilter>().mesh;
        baseHeights = waterTileMesh.vertices;
        player = GameObject.Find("Player");
        landmasses = LoadBodies();
        heights = null;
    }

    void Update()
    {
        if (GetComponent<TerrainCollider>().bounds.Contains(player.transform.position)) {
            Vector3 wave = GetWave(player.transform.position);
            Debug.Log("\nWave:\t" + wave.ToString());
            Vector3[] waveVertices = new Vector3[baseHeights.Length];
            Quaternion rotation = Quaternion.LookRotation(wave);
            for (int i = 0; i < baseHeights.Length; ++i) {
                Vector3 vertex = baseHeights[i];
                Vector3 direction = rotation * vertex;
                vertex.y += Mathf.Sin(Time.time * waveSpeed + direction.x + direction.z) * wave.magnitude;
                waveVertices[i] = vertex;
            }
            waterTileMesh.vertices = waveVertices;
        }
            
    }

    // It broke. For some reason every heights entry is the same and idk por quois
    private TerrainBody[] LoadBodies() {
        Terrain terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        TerrainBody[] bodies = new TerrainBody[20];

        heights = terrainData.GetHeights(0, 0, numSamples, numSamples);//terrainData.alphamapWidth, terrainData.alphamapHeight);
        //heights = new float[terrainData.size, terrainData.size];
        //for (int i = 0; i < terrainData.size; ++i) {
        //    for (int j = 0; j < terrainData.size; ++j) {
        //        heights[i, j] = terrainData.GetHeight(i, j);
        //    }
        //}
        for (int i = 1; i < heights.GetLength(0) - 1; ++i)
        {
            for (int j = 1; j < heights.GetLength(1) - 1; ++j)
            {
                if (heights[i, j] <= 0.01F) continue;
                if (heights[i, j] == Mathf.Infinity) {
                    while (j < terrainData.alphamapWidth - 1 && heights[i, j++] != Mathf.Infinity) {}
                    continue;
                }

                bodies[size] = new TerrainBody(terrain);
                TraceEdge(bodies[size], i, j);   // Traces edge of body and stores vertices in bodies[bodiesIndex]
                ++size;
            }
        }
                
        return bodies;
    }

    /// <summary>
    /// Moore-Neighbor Tracing algorithm.
    /// </summary>
    /// <param name="body">The landmass.</param>
    /// <param name="i">The row.</param>
    /// <param name="j">The column.</param>
    private void TraceEdge(TerrainBody body, int i, int j)
    {
        Vector2Int start = new Vector2Int(i, j);
        body.AddVertex(ArrayToPosition(i, j));
        Vector2Int boundary = start;
        Vector2Int enteredFrom = new Vector2Int(i, j - 1);
        //Vector2Int neighbor = new Vector2Int(i - 1, j);
        int neighborIndex = 6;   // North in MooreNeighbor array
        Vector2Int neighbor = MooreNeighbor[neighborIndex];
        while (neighbor != start)
        {
            int row = neighbor.x;
            int col = neighbor.y;
            if (heights[row, col] > 0)
            {
                body.AddVertex(ArrayToPosition(row, col));
                enteredFrom = new Vector2Int(row, col);
                boundary = neighbor;
                if (++neighborIndex > 7) neighborIndex = 0;
                neighbor = MooreNeighbor[neighborIndex];
                //neighbor = NextMooreNeighbor(enteredFrom, neighbor);
            }
            else
            {
                if (++neighborIndex > 7) neighborIndex = 0;
                neighbor = MooreNeighbor[neighborIndex];
                //neighbor = NextMooreNeighbor(enteredFrom, neighbor);
            }
        }
        if (!body.IsValid)
            throw new System.Exception("Landmass was not closed!");
    }

    private Vector2Int NextMooreNeighbor(Vector2Int enteredFrom, Vector2Int neighbor) {
        Vector2Int delta = neighbor - enteredFrom;
        if (delta.x == 0)
        {
            if (delta.y == 1)
            {
                return enteredFrom + new Vector2Int(1, 1);
            }
            else if (delta.y == -1)
            {
                return enteredFrom + new Vector2Int(-1, -1);
            }
        }
        else if (delta.x == 1)
        {
            if (delta.y == 0)
            {
                return enteredFrom + new Vector2Int(1, -1);
            }
            else if (delta.y == 1)
            {
                return enteredFrom + new Vector2Int(1, 0);
            }
            else if (delta.y == -1)
            {
                return enteredFrom + new Vector2Int(0, -1);
            }
        }
        else if (delta.x == -1)
        {
            if (delta.y == 0)
            {
                return enteredFrom + new Vector2Int(-1, 1);
            }
            else if (delta.y == 1)
            {
                return enteredFrom + new Vector2Int(0, 1);
            }
            else if (delta.y == -1)
            {
                return enteredFrom + new Vector2Int(-1, 0);
            }
        }
        else throw new System.Exception("Error calculating next Moore neigbor!");
        return Vector2Int.zero;
    }

    private Vector3 ArrayToPosition(int row, int column)
    {
        float percentWidth = (float)(column) / numSamples;   // These may be wrong
        float percentHeight = (float)(numSamples - row) / numSamples;
        float x = percentWidth * terrainData.size.x;
        float z = percentHeight * terrainData.size.z;

        return new Vector3(x, 0, z);
    }

    /// <summary>
    /// Gets the sum of all landmasses' contribution to a wave at the player's current position
    /// </summary>
    /// <returns>The wave.</returns>
    /// <param name="position">Player's global position.</param>
    public Vector3 GetWave(Vector3 position) {
        int x = (int)position.x;
        int z = (int)position.z;
        Vector3 relativeToTerrain = position - GetComponent<Terrain>().transform.position;
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < size; ++i) {
            sum += landmasses[i].GetWave(relativeToTerrain);    // Sum all waves into one
        }
        return sum;
    }

}
