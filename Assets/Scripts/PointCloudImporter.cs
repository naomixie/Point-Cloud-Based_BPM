using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

namespace PointCloud
{
    public class PointCloudImporter
    {
        // Singleton
        private static PointCloudImporter instance;
        private PointCloudImporter ()
        {
        }
        public static PointCloudImporter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PointCloudImporter();
                }
                return instance;
            }
        }

        public MeshData Load (string filePath, int maximumVertex = 65000)
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    Debug.Log(line);
                }
            }
            return null;
        }
    }
}