using System.Collections.Generic;
using UnityEngine;

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

    public class PointCloudGenerator : MonoBehaviour
    {
        [Header("Point Cloud Import")]
        public List<AssetInfo> AssetInfos = new List<AssetInfo>();

        private MeshData tempMesh = null;

        public GameObject prefab;

        public GameObject GenerateParent;

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
            tempMesh = PointCloudImporter.Instance.Load(AssetInfos);
        }

        public void renderMeshData ()
        {
            for (int i = 0 ; i < tempMesh.vertices.Count ; i++)
            {
                Instantiate(prefab, tempMesh.vertices[i], Quaternion.identity, GenerateParent.transform);
            }
        }
    }
}