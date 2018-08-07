using System;
using UnityEngine;

namespace UnityEngine
{
    public class TerrainBody
    {
        private Terrain terrain;
        private TerrainData terrainData;
        private float[, ] vertices;
        private int initSize = 100;
        private int size = 0;
        private bool locked = false;
        public bool IsValid {
            get { return locked; }
        }
        public Vector3 Center {
            get { return center; }
        }
        private Vector3 center;
       
        public TerrainBody(Terrain tr)
        {
            vertices = new float[2, initSize];
            terrain = tr;
            TerrainData td = tr.terrainData;
            terrainData = td;
        }

        /// <summary>
        /// Adds the vertex given by row/col indexing in an alphamap to the current landmass.
        /// The vertices are stored as world coordinates relative to the terrain's position.
        /// </summary>
        /// <returns><c>true</c>, if added vertex completes the landmass contour, <c>false</c> otherwise.</returns>
        /// <param name="row">Row.</param>
        /// <param name="col">Col.</param>
        public bool AddVertex(Vector3 vertex) {
            if (Mathf.Abs(vertices[0, 0] - vertex.x) < 0.1F && Mathf.Abs(vertices[1, 0] - vertex.z) < 0.1F)
            {
                locked = true;
                float sumx = 0;
                float sumz = 0;
                for (int i = 0; i < size; ++i) {
                    sumx += vertices[0, i];
                    sumz += vertices[1, i];
                }
                center = new Vector3((float)sumx / size, 0, (float)sumz / size);
                return locked;
            }
            else if (locked == true)
                throw new Exception("Cannot add vertex, terrain body already locked!");

            vertices[0, size] = vertex.x;
            vertices[1, size] = vertex.z;
            ++size;
            return locked;
        }

        public Vector3 GetWave(Vector3 position) {
            float distance = (center - position).sqrMagnitude;
            float magnitude = 1 / distance;
            Vector3 direction = (center - position).normalized;
            Debug.Log(magnitude * direction);
            return magnitude * direction;
        }

        public int Size {
            get { return size; }
        }
    }
}
