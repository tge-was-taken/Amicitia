namespace AtlusLibSharp.Test
{
    using System.IO;
    using SMT3.ChunkResources.Modeling;

    class Program
    {
        static void Main(string[] args)
        {
            SetCulture();
        }

        private static void SetCulture()
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        }

        private static void WriteObj(string path, MDChunk model)
        {
            int fIdx = 0;
            int bIdx = -1;
            using (StreamWriter writer = new StreamWriter(File.Create(path)))
            {
                foreach (MDNode node in model.Nodes)
                {
                    if (node.Mesh != null)
                    {
                        MDMesh mesh = node.Mesh;
                        if (mesh.SubMeshes == null)
                            continue;
                        foreach (IMDSubMesh subMesh in mesh.SubMeshes)
                        {
                            if (subMesh == null)
                                continue;
                            switch (subMesh.Type)
                            {
                                case 1:
                                    {
                                        MDSubMeshType1 sm = subMesh as MDSubMeshType1;
                                        foreach (MDSubMeshVifBatch batch in sm.Batches)
                                        {
                                            bIdx++;
                                            writer.WriteLine("g batch{0}", bIdx);
                                            for (int i = 0; i < batch.VertexCount; i++)
                                            {
                                                writer.WriteLine("v {0} {1} {2}", batch.TransformedPositions[i].X, batch.TransformedPositions[i].Y, batch.TransformedPositions[i].Z);
                                            }
                                            if (batch.TransformedNormals != null)
                                            {
                                                for (int i = 0; i < batch.VertexCount; i++)
                                                {
                                                    writer.WriteLine("vn {0} {1} {2}", batch.TransformedNormals[i].X, batch.TransformedNormals[i].Y, batch.TransformedNormals[i].Z);
                                                }
                                            }
                                            if (batch.TextureCoords != null)
                                            {
                                                for (int i = 0; i < batch.VertexCount; i++)
                                                {
                                                    writer.WriteLine("vt {0} {1}", batch.TextureCoords[i].X, batch.TextureCoords[i].Y);
                                                }
                                            }
                                            for (int i = 0; i < batch.FaceCount; i++)
                                            {
                                                writer.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", batch.FaceIndices[i][0] + fIdx + 1, batch.FaceIndices[i][1] + fIdx + 1, batch.FaceIndices[i][2] + fIdx + 1);
                                            }
                                            fIdx += batch.VertexCount;
                                        }

                                        break;
                                    }
                                case 2:
                                    {
                                        MDSubMeshType2 sm = subMesh as MDSubMeshType2;
                                        foreach (MDSubMeshVifBatch batch in sm.Batches)
                                        {
                                            if (batch.FaceIndices == null)
                                                continue;
                                            bIdx++;
                                            writer.WriteLine("g batch{0}", bIdx);
                                            for (int i = 0; i < batch.VertexCount; i++)
                                            {
                                                writer.WriteLine("v {0} {1} {2}", batch.TransformedPositions[i].X, batch.TransformedPositions[i].Y, batch.TransformedPositions[i].Z);
                                            }
                                            if (batch.TransformedNormals != null)
                                            {
                                                for (int i = 0; i < batch.VertexCount; i++)
                                                {
                                                    writer.WriteLine("vn {0} {1} {2}", batch.TransformedNormals[i].X, batch.TransformedNormals[i].Y, batch.TransformedNormals[i].Z);
                                                }
                                            }
                                            /*
                                            if (batch.TextureCoords != null)
                                            {
                                                for (int i = 0; i < batch.VertexCount; i++)
                                                {
                                                    writer.WriteLine("vt {0} {1}", batch.TextureCoords[i].X, batch.TextureCoords[i].Y);
                                                }
                                            }
                                            */
                                            for (int i = 0; i < batch.FaceCount; i++)
                                            {
                                                writer.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", batch.FaceIndices[i][0] + fIdx + 1, batch.FaceIndices[i][1] + fIdx + 1, batch.FaceIndices[i][2] + fIdx + 1);
                                            }
                                            fIdx += batch.VertexCount;
                                        }

                                        break;
                                    }
                            }
                        }
                    }
                }
            }
        }
    }
}
