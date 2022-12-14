using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloud
{
    public class PointData
    {
        public Vector3 position;
        public Vector3 normal;
        public Color color;
        public int index;
    }

    public class MeshData : MonoBehaviour
    {
        public List<PointData> points = new List<PointData>();

        public float minx = 1000f, miny = 1000f, minz = 1000f, maxx = -1000f, maxy = -1000f, maxz = -1000f;
    }
}