using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloud
{
    public class MeshData : MonoBehaviour
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public Color[] colors;
        public Bounds bounds;
    }
}