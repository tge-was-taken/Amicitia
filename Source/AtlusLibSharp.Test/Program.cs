namespace AtlusLibSharp.Test
{
    using Graphics.RenderWare;
    using Scripting;

    class Program
    {
        static void Main(string[] args)
        {
            SetCulture();

            /*
            BMDFile bmd = new BMDFile(@"C:\Users\TGE\Downloads\Shin Megami Tensei Persona 3 FES [USA - English - PS2DVD]\CVM_DATA\HELP\DATMYTH.BMD");
            BMDDecompiler.Decompile(bmd, @"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\CVM_DATA\HELP\DATMYTH.msg");
            BMDScriptParser.CompileScript(@"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\CVM_DATA\HELP\DATMYTH.msg");
            */

            RMDScene rmd = new RMDScene(@"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\bc001.RMD");
            var split = new RWMeshMaterialSplitData(rmd.Scenes[0].Meshes[4], RWPrimitiveType.TriangleStrip);
            var split2 = new RWMeshMaterialSplitData(rmd.Scenes[0].Meshes[4], RWPrimitiveType.TriangleList);

            //            BFFile bf;
            //#if !DEBUG
            //            try
            //            {
            //#endif
            //            bf = BFFile.AssembleFromBFASM(args[0] /*, new BMDFile(Path.GetFileNameWithoutExtension(args[0])+".bmd")*/ );
            //#if !DEBUG
            //            }
            //            catch (BFASMParserException e)
            //            {
            //                Console.WriteLine("Couldn't parse file. {0}", e.Message);
            //                Console.ReadKey();
            //                return;
            //            }
            //#endif

            //            bf.Save(Path.GetFileNameWithoutExtension(args[0]) + "_new.bf");


            /*
            var bf = new BFFile(args[0]);
            bf.ExportDisassembly(Path.ChangeExtension(args[0], "bfasm"));
            */

            /*
            var messageTable = new MessageTable(@"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\CVM_BTL\BATTLE\MSG.TBL");
            messageTable.WriteToTextFile(@"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\CVM_BTL\BATTLE\MSG.TXT");
            */

            /*
            var bf2 = new BFFile(@"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\CVM_BTL\BATTLE\AICALC.BF");

            bf2.DisassembleToText(@"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\CVM_BTL\BATTLE\AICALC.scrasm");
            */

            /*
            TGAFile tga = new TGAFile(new Bitmap(@"C:\Users\TGE\Desktop\ponyloaf_punished.png"), TGAEncoding.IndexedRLE);
            tga.Save(@"C:\Users\TGE\Desktop\ponyloaf_punished.tga");
            */

            /*
            PAKToolFile pac = new PAKToolFile(@"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\CVM_DATA\MODEL\PACK\BC001.PAC");
            RMDScene scene = new RMDScene(pac.Entries.Find(entry => entry.Name.Contains(".RMD")).Data);

            Assimp.AssimpContext aiContext = new Assimp.AssimpContext();
            Assimp.Scene aiScene = new Assimp.Scene();
            aiScene.RootNode = new Assimp.Node("Root");

            foreach (RWClump clump in scene.Clumps)
            {
                int geoIdx = 0;
                foreach (RWGeometry geo in clump.GeometryList)
                {
                    Assimp.Mesh aiMesh = new Assimp.Mesh("mesh_" + geoIdx++, Assimp.PrimitiveType.Triangle);

                    aiMesh.MaterialIndex = 0;

                    foreach (RWTriangle tri in geo.Triangles)
                    {
                        aiMesh.Faces.Add(new Assimp.Face(new int[] { tri.A, tri.B, tri.C }));
                    }

                    foreach (OpenTK.Vector3 vertex in geo.Vertices)
                    {
                        aiMesh.Vertices.Add(new Assimp.Vector3D(vertex.X, vertex.Y, vertex.Z));
                    }

                    if (geo.HasNormals)
                    {
                        foreach (OpenTK.Vector3 normal in geo.Normals)
                        {
                            aiMesh.Normals.Add(new Assimp.Vector3D(normal.X, normal.Y, normal.Z));
                        }
                    }

                    for (int i = 0; i < geo.TextureCoordinateChannelCount; i++)
                    {
                        List<Assimp.Vector3D> texCoordChannel = new List<Assimp.Vector3D>();
                        foreach (OpenTK.Vector2 texCoord in geo.TextureCoordinateChannels[i])
                        {
                            texCoordChannel.Add(new Assimp.Vector3D(texCoord.X, texCoord.Y, 0));
                        }

                        aiMesh.TextureCoordinateChannels[i] = texCoordChannel;
                    }

                    if (geo.HasColors)
                    {
                        List<Assimp.Color4D> vertColorChannel = new List<Assimp.Color4D>();

                        foreach (Color color in geo.Colors)
                        {
                            vertColorChannel.Add(new Assimp.Color4D(color.R * 255, color.G * 255, color.B * 255, color.A * 255));
                        }

                        aiMesh.VertexColorChannels.Add(vertColorChannel);
                    }

                    int idx = 0;
                    foreach (RWMaterial mat in geo.Materials)
                    {
                        Assimp.Material aiMaterial = new Assimp.Material();
                        aiMaterial.Name = "material_" + idx++;
                        aiScene.Materials.Add(aiMaterial);
                    }

                    aiScene.Meshes.Add(aiMesh);
                }

                foreach (RWAtomic atomic in clump.Atomics)
                {
                    Assimp.Node node = new Assimp.Node();
                    node.Name = aiScene.Meshes[atomic.GeometryIndex].Name;
                    node.MeshIndices.Add(atomic.GeometryIndex);
                    node.Transform = clump.FrameListNode.Frames[atomic.FrameIndex].WorldMatrix.ToAssimpMatrix4x4();
                    aiScene.RootNode.Children.Add(node);
                }
            }

            aiContext.ExportFile(aiScene, "test.dae", "collada");
            
            /*
            for (int i = 0; i < scene.TextureDictionary.TextureCount; i++)
            {
                Bitmap bitmap = scene.TextureDictionary.Textures[i].GetBitmap();
                string name = scene.TextureDictionary.Textures[i].Name;

                //scene.TextureDictionary.Textures[i] = new RWTextureNative(name, bitmap, PS2.Graphics.PixelFormat.PSMT8);
            }

            scene.TextureDictionary.Save("_test.txd");

            PAKToolFile newPac = new PAKToolFile();
            newPac.Entries.Add(new PAKToolFileEntry("bc001.RMD", scene.GetBytes()));
            newPac.Save("BC001.PAC");
            */
        }

        private static void SetCulture()
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        }

        //private static void WriteObj(string path, MDChunk model)
        //{
        //    int fIdx = 0;
        //    int bIdx = -1;
        //    using (StreamWriter writer = new StreamWriter(File.Create(path)))
        //    {
        //        foreach (MDNode node in model.Nodes)
        //        {
        //            if (node.Mesh != null)
        //            {
        //                MDMesh mesh = node.Mesh;
        //                if (mesh.SubMeshes == null)
        //                    continue;
        //                foreach (IMDSubMesh subMesh in mesh.SubMeshes)
        //                {
        //                    if (subMesh == null)
        //                        continue;
        //                    switch (subMesh.Type)
        //                    {
        //                        case 1:
        //                            {
        //                                MDSubMeshType1 sm = subMesh as MDSubMeshType1;
        //                                foreach (MDSubMeshVifBatch batch in sm.Batches)
        //                                {
        //                                    bIdx++;
        //                                    writer.WriteLine("g batch{0}", bIdx);
        //                                    for (int i = 0; i < batch.VertexCount; i++)
        //                                    {
        //                                        writer.WriteLine("v {0} {1} {2}", batch.TransformedPositions[i].X, batch.TransformedPositions[i].Y, batch.TransformedPositions[i].Z);
        //                                    }
        //                                    if (batch.TransformedNormals != null)
        //                                    {
        //                                        for (int i = 0; i < batch.VertexCount; i++)
        //                                        {
        //                                            writer.WriteLine("vn {0} {1} {2}", batch.TransformedNormals[i].X, batch.TransformedNormals[i].Y, batch.TransformedNormals[i].Z);
        //                                        }
        //                                    }
        //                                    if (batch.TextureCoords != null)
        //                                    {
        //                                        for (int i = 0; i < batch.VertexCount; i++)
        //                                        {
        //                                            writer.WriteLine("vt {0} {1}", batch.TextureCoords[i].X, batch.TextureCoords[i].Y);
        //                                        }
        //                                    }
        //                                    for (int i = 0; i < batch.FaceCount; i++)
        //                                    {
        //                                        writer.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", batch.FaceIndices[i][0] + fIdx + 1, batch.FaceIndices[i][1] + fIdx + 1, batch.FaceIndices[i][2] + fIdx + 1);
        //                                    }
        //                                    fIdx += batch.VertexCount;
        //                                }

        //                                break;
        //                            }
        //                        case 2:
        //                            {
        //                                MDSubMeshType2 sm = subMesh as MDSubMeshType2;
        //                                foreach (MDSubMeshVifBatch batch in sm.Batches)
        //                                {
        //                                    if (batch.FaceIndices == null)
        //                                        continue;
        //                                    bIdx++;
        //                                    writer.WriteLine("g batch{0}", bIdx);
        //                                    for (int i = 0; i < batch.VertexCount; i++)
        //                                    {
        //                                        writer.WriteLine("v {0} {1} {2}", batch.TransformedPositions[i].X, batch.TransformedPositions[i].Y, batch.TransformedPositions[i].Z);
        //                                    }
        //                                    if (batch.TransformedNormals != null)
        //                                    {
        //                                        for (int i = 0; i < batch.VertexCount; i++)
        //                                        {
        //                                            writer.WriteLine("vn {0} {1} {2}", batch.TransformedNormals[i].X, batch.TransformedNormals[i].Y, batch.TransformedNormals[i].Z);
        //                                        }
        //                                    }
        //                                    /*
        //                                    if (batch.TextureCoords != null)
        //                                    {
        //                                        for (int i = 0; i < batch.VertexCount; i++)
        //                                        {
        //                                            writer.WriteLine("vt {0} {1}", batch.TextureCoords[i].X, batch.TextureCoords[i].Y);
        //                                        }
        //                                    }
        //                                    */
        //                                    for (int i = 0; i < batch.FaceCount; i++)
        //                                    {
        //                                        writer.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", batch.FaceIndices[i][0] + fIdx + 1, batch.FaceIndices[i][1] + fIdx + 1, batch.FaceIndices[i][2] + fIdx + 1);
        //                                    }
        //                                    fIdx += batch.VertexCount;
        //                                }

        //                                break;
        //                            }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
