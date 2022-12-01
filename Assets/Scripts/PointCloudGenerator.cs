using UnityEngine;

namespace PointCloud
{
    public class PointCloudGenerator : MonoBehaviour
    {
        [Header("Point Cloud Import")]
        public string AssetDirectory;
        public string AssetName;

        private MeshData tempMesh = null;

        public GameObject prefab;

        private static PointCloudGenerator instance;

        private PointCloudGenerator ()
        {
        }

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
            tempMesh = PointCloudImporter.Instance.Load(AssetDirectory + AssetName);
        }

        public void renderMeshData ()
        {
            for (int i = 0 ; i < tempMesh.vertexCount ; i++)
            {
                Instantiate(prefab, tempMesh.vertices[i], Quaternion.identity);
            }
        }
    }
}