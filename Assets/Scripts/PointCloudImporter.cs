using System.IO;
using UnityEngine;

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
            MeshData meshData = new MeshData();
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                bool inHeader = true;
                int vertexCount = 0;
                foreach (string line in lines)
                {
                    if (inHeader)
                    {
                        /* Reading object information */
                        if (line.StartsWith("obj_info"))
                        {
                            // TODO: consider other object information
                            string[] info_str = line.Split(" ");
                        }
                        /* Read element */
                        else if (line.StartsWith("element"))
                        {
                            string[] info_str = line.Split(" ");
                            switch (info_str[1])
                            {
                                case "vertex":
                                    meshData.vertexCount = int.Parse(info_str[2]);
                                    meshData.vertices = new Vector3[meshData.vertexCount];
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (line.StartsWith("property"))
                        {
                            // TODO: consider properties
                            string[] info_str = line.Split(" ");
                        }
                        else if (line == "end_header")
                        {
                            inHeader = false;
                        }
                    }
                    else
                    {
                        if (vertexCount < meshData.vertexCount)
                        {
                            string[] info_str = line.Split(' ');
                            meshData.vertices[vertexCount] = new Vector3(float.Parse(info_str[0]), float.Parse(info_str[1]), float.Parse(info_str[2]));
                            vertexCount++;
                        }
                        else
                        {
                            // TODO: consider other variables
                            return meshData;
                        }
                    }
                    
                }
            }
            return meshData;
        }        
    }
}