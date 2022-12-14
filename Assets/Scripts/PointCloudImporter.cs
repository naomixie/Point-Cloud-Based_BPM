using System.Collections.Generic;
using System.IO;
using UnityEditor.VersionControl;
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

        public MeshData Load (List<AssetInfo> assets, int maximumVertex = 65000)
        {
            MeshData meshData = new MeshData();
            foreach (AssetInfo asset in assets)
            {
                string filePath = asset.AssetDirectory + asset.AssetName;
                int assetVertices = 0;

                if (File.Exists(filePath))
                {
                    // Reading the header
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
                                        assetVertices = int.Parse(info_str[2]);
                                        //meshData.vertices = new Vector3[meshData.vertexCount];
                                        Debug.Log(filePath + "\t" + assetVertices);
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
                            if (vertexCount < assetVertices)
                            {
                                string[] info_str = line.Split(' ');
                                PointData data = new PointData();
                                data.index = vertexCount;
                                data.position = new Vector3(float.Parse(info_str[0]) * 100, float.Parse(info_str[1]) * 100, float.Parse(info_str[2]) * 100);
                                data.normal = new Vector3(float.Parse(info_str[3]), float.Parse(info_str[4]), float.Parse(info_str[5]));
                                meshData.points.Add(data);
                                vertexCount++;


                                // Get the bounds of the mesh
                                if (float.Parse(info_str[0]) * 100 > meshData.maxx)
                                    meshData.maxx = float.Parse(info_str[0]) * 100;
                                if (float.Parse(info_str[1]) * 100 > meshData.maxy)
                                    meshData.maxy = float.Parse(info_str[1]) * 100;
                                if (float.Parse(info_str[2]) * 100 > meshData.maxz)
                                    meshData.maxz = float.Parse(info_str[2]) * 100;

                                if (float.Parse(info_str[0]) * 100 < meshData.minx)
                                    meshData.minx = float.Parse(info_str[0]) * 100;
                                if (float.Parse(info_str[1]) * 100 < meshData.miny)
                                    meshData.miny = float.Parse(info_str[1]) * 100;
                                if (float.Parse(info_str[2]) * 100 < meshData.minz)
                                    meshData.minz = float.Parse(info_str[2]) * 100;
                            }
                            else
                            {
                                // TODO: consider other variables
                                break;
                            }
                        }

                    }
                }
            }
            return meshData;
        }

    }
}