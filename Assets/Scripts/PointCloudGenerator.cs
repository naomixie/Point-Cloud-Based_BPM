using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace PointCloud
{
    [System.Serializable]
    public struct AssetInfo
    {
        public string AssetDirectory;
        public string AssetName;
        [HideInInspector]
        public string filePath;
        [HideInInspector]
        public int endHeaderLine;
    }

    public struct DistancePair
    {
        public Vector3Int VoxelIndex;

        public int pointIndex;

        public float distance;
    }

    public class DistanceComparer : IComparer<DistancePair>
    {

        int IComparer<DistancePair>.Compare (DistancePair x, DistancePair y)
        {
            if (x.distance == 0 || y.distance == 0)
            {
                return 0;
            }
            return x.distance.CompareTo(y.distance);
        }
    }

    public class PointCloudGenerator : MonoBehaviour
    {
        [Header("Point Cloud Import")]
        public List<AssetInfo> AssetInfos = new List<AssetInfo>();

        private MeshData tempMesh = null;

        public GameObject prefab;

        public GameObject GenerateParent;

        public GameObject boundsObject;

        public GameObject Ball;

        public MeshFilter outputMesh;

        public List<float> radius = new List<float>();

        private List<PointData>[,,] voxels;

        // Mesh triangles
        private List<int> meshTriangles = new List<int>();

        private Vector3Int voxelsSize;

        private Front front = new Front();

        private static PointCloudGenerator instance;

        public static PointCloudGenerator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PointCloudGenerator();
                }
                return instance;
            }
        }

        void Start ()
        {
            tempMesh = PointCloudImporter.Instance.Load(AssetInfos);
        }

        public void renderMeshData ()
        {
            SpatialPartitioning(1.0f, tempMesh);
            BPA();
            generateMesh();
        }

        // Step 1 Partition the space
        private void SpatialPartitioning (float radius, MeshData mesh)
        {
            float voxelSize = radius * 2;

            // TODO: Delete after testing
            Instantiate(boundsObject, new Vector3(tempMesh.minx, tempMesh.miny, tempMesh.minz), Quaternion.identity);
            Instantiate(boundsObject, new Vector3(tempMesh.minx, tempMesh.miny, tempMesh.maxz), Quaternion.identity);
            Instantiate(boundsObject, new Vector3(tempMesh.minx, tempMesh.maxy, tempMesh.minz), Quaternion.identity);
            Instantiate(boundsObject, new Vector3(tempMesh.minx, tempMesh.maxy, tempMesh.maxz), Quaternion.identity);
            Instantiate(boundsObject, new Vector3(tempMesh.maxx, tempMesh.miny, tempMesh.minz), Quaternion.identity);
            Instantiate(boundsObject, new Vector3(tempMesh.maxx, tempMesh.maxy, tempMesh.minz), Quaternion.identity);
            Instantiate(boundsObject, new Vector3(tempMesh.maxx, tempMesh.miny, tempMesh.maxz), Quaternion.identity);
            Instantiate(boundsObject, new Vector3(tempMesh.maxx, tempMesh.maxy, tempMesh.maxz), Quaternion.identity);

            // Calculating the number of voxels for each axis
            voxelsSize = new Vector3Int((int) Math.Ceiling(mesh.maxx / voxelSize) - (int) Math.Floor(mesh.minx / voxelSize),
                (int) Math.Ceiling(mesh.maxy / voxelSize) - (int) Math.Floor(mesh.miny / voxelSize),
                (int) Math.Ceiling(mesh.maxz / voxelSize) - (int) Math.Floor(mesh.minz / voxelSize));

            voxels = new List<PointData>[voxelsSize.x, voxelsSize.y, voxelsSize.z];

            // Initializing voxels
            for (int i = 0 ; i < voxelsSize.x ; ++i)
                for (int j = 0 ; j < voxelsSize.y ; ++j)
                    for (int k = 0 ; k < voxelsSize.z ; ++k)
                        voxels[i, j, k] = new List<PointData>();

            // TODO: Delete after Testing
            Debug.Log(voxelsSize);

            // Inserting points to voxels
            foreach (PointData point in mesh.points)
            {
                Vector3Int offset = findVoxelOffset(point.position, mesh, voxelSize);

                voxels[offset.x, offset.y, offset.z].Add(point);
            }

            for (int i = 0 ; i < voxelsSize.x ; ++i)
                for (int j = 0 ; j < voxelsSize.y ; ++j)
                    for (int k = 0 ; k < voxelsSize.z ; ++k)
                    {
                        // Only Instantiate voxel if it is not empty
                        if (voxels[i, j, k].Count == 0)
                            continue;
                        GameObject voxel = new GameObject("voxel " + i + j + k);
                        voxel.transform.SetParent(GenerateParent.transform);
                        foreach (PointData point in voxels[i, j, k])
                            Instantiate(prefab, point.position, Quaternion.identity, voxel.transform);
                    }
        }

        private Vector3Int findVoxelOffset (Vector3 position, MeshData mesh, float voxelSize)
        {
            int voxelX = (int) Math.Floor(position.x / voxelSize) - (int) Math.Floor(mesh.minx / voxelSize);
            int voxelY = (int) Math.Floor(position.y / voxelSize) - (int) Math.Floor(mesh.miny / voxelSize);
            int voxelZ = (int) Math.Floor(position.z / voxelSize) - (int) Math.Floor(mesh.minz / voxelSize);
            return new Vector3Int(voxelX, voxelY, voxelZ);
        }


        private bool findSeedTriangle ()
        {
            // For each vertex in each voxel
            for (int i = 0 ; i < voxelsSize.x ; ++i)
                for (int j = 0 ; j < voxelsSize.y ; ++j)
                    for (int k = 0 ; k < voxelsSize.z ; ++k)
                    {
                        if (voxels[i, j, k].Count == 0)
                            continue;

                        Debug.Log("Finding vertex in voxel" + i + j + k);
                        // Pick any point not yet used by the reconstructed triangulation
                        for (int p = 0 ; p < 1 ; p++)
                        {
                            List<DistancePair> pairDistances = new List<DistancePair>();
                            // Get the queue of points in order of distance 
                            for (int q = 0 ; q < voxels[i, j, k].Count ; ++q)
                            {
                                if (p == q)
                                    continue;
                                DistancePair pair = new DistancePair();
                                pair.VoxelIndex = new Vector3Int(i, j, k);
                                pair.pointIndex = q;
                                pair.distance = Vector3.Distance(voxels[i, j, k][p].position, voxels[i, j, k][q].position);
                                pairDistances.Add(pair);
                            }
                            pairDistances.Sort(new DistanceComparer());

                            // Build potential seed triangles
                            for (int first = 0 ; first < pairDistances.Count ; first++)
                            {
                                for (int second = 1 ; second < pairDistances.Count ; second++)
                                {
                                    PointData f = voxels[pairDistances[first].VoxelIndex.x, pairDistances[first].VoxelIndex.y, pairDistances[first].VoxelIndex.z][pairDistances[first].pointIndex];
                                    PointData s = voxels[pairDistances[second].VoxelIndex.x, pairDistances[second].VoxelIndex.y, pairDistances[second].VoxelIndex.z][pairDistances[second].pointIndex];
                                    PointData po = voxels[i, j, k][p];
                                    Vector3 triangleNormal = getTriangleNormal(po, f, s);

                                    // Check that the triangle normal is consistent with the vertex normals, i.e., pointing outward
                                    if (!checkConsistency(triangleNormal, po.normal))
                                    {
                                        PointData temp = f;
                                        f = s;
                                        s = temp;
                                        triangleNormal = getTriangleNormal(po, f, s);
                                    }

                                    // Test that a p-ball with center in the outward halfspace touches all three vertices and contains no other data point
                                    Vector3 ballCentre = getBallCentre(po, f, s, 1.0f, triangleNormal);
                                    GameObject b = Instantiate(Ball, ballCentre, Quaternion.identity);
                                    b.name = "voxel023 point" + p + pairDistances[first].pointIndex + pairDistances[second].pointIndex;
                                    b.SetActive(false);

                                    // Check neighbor 27 voxels for collision
                                    if (!checkSeedCollision(i, j, k, ballCentre, 1.0f, po, f, s))
                                    {
                                        outputTriangle(po, f, s, ballCentre);
                                        // Stop when a valid seed triangle has been found
                                        return true;
                                    }
                                }
                            }
                        }
                    }
            return false;
        }

        private void outputTriangle (PointData first, PointData second, PointData third, Vector3 ballCenter)
        {
            meshTriangles.Add(first.index);
            meshTriangles.Add(second.index);
            meshTriangles.Add(third.index);

            Edge a = new Edge(first, second, third, ballCenter, null, null, EdgeStatus.active);
            Edge b = new Edge(second, third, first, ballCenter, null, null, EdgeStatus.active);
            Edge c = new Edge(third, first, second, ballCenter, null, null, EdgeStatus.active);

            a.next = b;
            a.prev = c;

            b.next = c;
            b.prev = a;

            c.next = a;
            c.prev = b;

            front.edges.Add(a);
            front.edges.Add(b);
            front.edges.Add(c);
        }


        private void join ()
        {
        }

        private void glue ()
        {
        }

        private void generateMesh ()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            foreach (PointData point in tempMesh.points)
            {
                vertices.Add(point.position);
                normals.Add(point.normal);
            }
            Mesh mesh = new Mesh();
            mesh.name = "test";
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.SetIndices(meshTriangles.ToArray(), MeshTopology.Triangles, 0);
            outputMesh.mesh = mesh;
        }

        private void BPA ()
        {
            while (true)
            {
                Edge edge = front.getActiveEdge();
                while (edge != null)
                {
                    Vector3 midPoint = (edge.StartPoint.position + edge.EndPoint.position) / 2;
                    Vector3 ballCentre = getBallCentre(edge.StartPoint, edge.EndPoint, edge.OppsPoint, 1.0f, getTriangleNormal(edge.StartPoint, edge.EndPoint, edge.OppsPoint));
                    float trajectoryRadius = Vector3.Distance(midPoint, ballCentre);
                    
                    Vector3Int voxelIndex = findVoxelOffset(midPoint, tempMesh, 2.0f);
                    for (int a = voxelIndex.x > 0 ? voxelIndex.x - 1 : 0 ; a < (voxelIndex.x < voxelsSize.x - 1 ? voxelIndex.x + 1 : voxelsSize.x - 1) ; ++a)
                        for (int b = voxelIndex.y > 0 ? voxelIndex.y - 1 : 0 ; b < (voxelIndex.y < voxelsSize.y - 1 ? voxelIndex.y + 1 : voxelsSize.y - 1) ; ++b)
                            for (int c = voxelIndex.z > 0 ? voxelIndex.z - 1 : 0 ; c < (voxelIndex.z < voxelsSize.z - 1 ? voxelIndex.z + 1 : voxelsSize.z - 1) ; ++c)
                                foreach (PointData data in voxels[a, b, c])
                                {
                                }
                }

                if (!findSeedTriangle())
                {
                    break;
                }

            }

        }


        private bool checkConsistency (Vector3 triangleNormal, Vector3 poNormal)
        {
            if (Vector3.Angle(triangleNormal, poNormal) < 90f)
                return true;
            return false;
        }

        private Vector3 getTriangleNormal (PointData po, PointData b, PointData c)
        {
            Vector3 edge1 = b.position - po.position;
            Vector3 edge2 = c.position - po.position;

            return Vector3.Cross(edge1, edge2).normalized;
        }

        private Vector3 getBallCentre (PointData po, PointData f, PointData s, float radius, Vector3 triangleNormal)
        {
            Vector3 centre = new Vector3((po.position.x + f.position.x + s.position.x) / 3, (po.position.y + f.position.y + s.position.y) / 3, (po.position.z + f.position.z + s.position.z) / 3);
            float cenDistance = Vector3.Distance(centre, po.position);
            float alpha = Mathf.Acos(cenDistance / radius);
            float distance = radius * Mathf.Sin(alpha);
            return centre + triangleNormal * distance;
        }

        private bool checkSeedCollision (int i, int j, int k, Vector3 ballCentre, float radius, PointData p, PointData f, PointData s)
        {
            for (int a = i > 0 ? i - 1 : 0 ; a < (i < voxelsSize.x - 1 ? i + 1 : voxelsSize.x - 1) ; ++a)
                for (int b = j > 0 ? j - 1 : 0 ; b < (j < voxelsSize.y - 1 ? j + 1 : voxelsSize.y - 1) ; ++b)
                    for (int c = k > 0 ? k - 1 : 0 ; c < (k < voxelsSize.z - 1 ? k + 1 : voxelsSize.z - 1) ; ++c)
                        foreach (PointData data in voxels[a, b, c])
                        {
                            // Skip selected vertices
                            if (data == p || data == f || data == s)
                                continue;
                            if (Vector3.Distance(data.position, ballCentre) <= radius)
                                return true;
                        }
            return false;
        }
    }
}