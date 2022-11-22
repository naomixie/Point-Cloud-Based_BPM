using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloud
{
    public class MeshData : MonoBehaviour
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Color[] colors;
        public int vertexCount;
        public Bounds bounds;
    }
}