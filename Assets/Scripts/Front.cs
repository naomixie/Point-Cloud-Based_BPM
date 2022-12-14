using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloud
{
    public enum EdgeStatus
    {
        active, boundary, frozen
    }

    [System.Serializable]
    public class Edge : MonoBehaviour
    {
        public PointData StartPoint, EndPoint, OppsPoint;

        // the center of the ball that touches all three points
        public Vector3 BallCenter;

        public Edge prev, next;

        public EdgeStatus status;

        public Edge(PointData startPoint, PointData endPoint, PointData oppsPoint, Vector3 ballCenter, Edge prev, Edge next, EdgeStatus status = EdgeStatus.active)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            OppsPoint = oppsPoint;
            BallCenter = ballCenter;
            this.prev = prev;
            this.next = next;
            this.status = status;
        }
    }

    public class Front : MonoBehaviour
    {
        public List<Edge> edges = new List<Edge>();


    }
}