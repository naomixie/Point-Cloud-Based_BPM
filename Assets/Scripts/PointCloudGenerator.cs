using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PointCloud
{
    public class PointCloudGenerator : MonoBehaviour
    {
        [Header("Point Cloud Import")]
        public string AssetDirectory;
        public string AssetName;

        void Start ()
        {
            PointCloudImporter.Instance.Load(AssetDirectory + AssetName);
        }

        void Update ()
        {
        }
    }
}