namespace AtlusLibSharp.Sample
{
    using System;
    using System.IO;

    using SMT3;
    using SMT3.ChunkResources;
    using SMT3.ChunkResources.Graphics;
    using SMT3.ChunkResources.Scripting;
    using SMT3.ChunkResources.Modeling;
    using Persona3.Archives;
    using Generic.Archives;

    class Program
    {
        static void Main(string[] args)
        {
            SetCulture();

            //RunARCTool(args);
            //RunBFTool(args);
            //RunMessageTool(args);
        }

        private static void SetCulture()
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        }

        private static void RunBFTool(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No input specified.");
                Console.WriteLine("Usage:");
                Console.WriteLine(" Enter path to BF file to extract it to an XML and a folder of the same name.");
                Console.WriteLine(" Enter path to XML file to pack into a BF.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            if (args[0].EndsWith(".BF", StringComparison.InvariantCultureIgnoreCase))
            {
                BFChunk bf = ChunkFactory.Get<BFChunk>(args[0]);
                string baseName = Path.GetFileNameWithoutExtension(args[0]);
                bf.Extract(Path.GetDirectoryName(baseName) + baseName + "\\" + baseName);
            }
            else if (args[0].EndsWith(".XML", StringComparison.InvariantCultureIgnoreCase))
            {
                BFChunk bf;

                try
                {
                    bf = new BFChunk(args[0]);
                }
                catch (InvalidDataException)
                {
                    Console.WriteLine("Xml root element name mismatch.\nAre you sure it was exported by this tool?");
                    Console.ReadKey();
                    return;
                }

                bf.Save(args[0].Replace(".XML", ".BF"));
            }
        }

        private static void RunBVPTool(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No input specified.");
                Console.WriteLine("Usage:");
                Console.WriteLine(" Enter path to BVP file to extract it to a folder of the same name.");
                Console.WriteLine(" Enter path to directory to pack into a BVP.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            if (args[0].EndsWith(".BVP", StringComparison.InvariantCultureIgnoreCase))
            {
                BVPArchive bvp = new BVPArchive(args[0]);
                bvp.Extract(Path.GetFileNameWithoutExtension(args[0]));
            }
            else if (!Path.HasExtension(args[0]))
            {
                BVPArchive bvp = BVPArchive.Create(args[0]);
                bvp.Save(Path.GetFileName(args[0]) + ".BVP");
            }
        }

        private static void RunARCTool(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No input specified.");
                Console.WriteLine("Usage:");
                Console.WriteLine(" Enter path to ARC file to extract it to a folder of the same name.");
                Console.WriteLine(" Enter path to directory to pack into a ARC.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            if (Path.HasExtension(args[0]))
            {
                if (!GenericVitaArchive.VerifyFileType(args[0]))
                {
                    Console.WriteLine("This is not a proper arc file!");
                    if (GenericPAK.VerifyFileType(args[0]))
                    {
                        Console.WriteLine("Detected format: regular .bin/.pac/.pak archive.");
                    }
                }

                GenericVitaArchive arc = new GenericVitaArchive(args[0]);
                string path = Path.GetFileNameWithoutExtension(args[0]);
                Directory.CreateDirectory(path);
                for (int i = 0; i < arc.EntryCount; i++)
                {
                    File.WriteAllBytes(path + "//" + arc.Entries[i].Name, arc.Entries[i].Data);
                }
            }
            else if (!Path.HasExtension(args[0]))
            {
                GenericVitaArchive arc = GenericVitaArchive.Create(args[0]);
                arc.Save(Path.GetFileName(args[0]) + ".arc");
            }
        }

        private static void RunMessageTool(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No input specified.");
                Console.WriteLine("Usage:");
                Console.WriteLine(" Enter path to BMD file to convert BMD file to XML.");
                Console.WriteLine(" Enter path to XML file to convert XML file to BMD.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            if (args[0].EndsWith(".BMD", StringComparison.InvariantCultureIgnoreCase))
            {
                MSGChunk msg = ChunkFactory.Get<MSGChunk>(args[0]);

                if (msg == null)
                {
                    Console.WriteLine("Could not read BMD.");
                    Console.ReadKey();
                    return;
                }

                msg.SaveXml(args[0] + ".XML");
            }
            else if (args[0].EndsWith(".XML", StringComparison.InvariantCultureIgnoreCase))
            {
                MSGChunk msg;
                try
                {
                    msg = new MSGChunk(args[0]);
                }
                catch (InvalidDataException)
                {
                    Console.WriteLine("Xml header element name mismatch.\nAre you sure the xml was exported by this tool?");
                    Console.ReadKey();
                    return;
                }

                msg.Save(args[0].Replace(".XML", ".BMD"));
            }
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
