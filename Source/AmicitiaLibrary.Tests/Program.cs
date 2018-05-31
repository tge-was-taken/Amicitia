using System;
using System.Linq;
using System.Numerics;
using Assimp.Configs;

namespace AmicitiaLibrary.Tests
{
    using Graphics.RenderWare;
    using Scripting;
    using AmicitiaLibrary.FileSystems.CVM;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    using IO;
    using FileSystems.PAKToolArchive;

    class Program
    {
        static void Main(string[] args)
        {
            var aiCtx = new Assimp.AssimpContext();
            Assimp.Scene aiScene;
            RmdScene rmdScene;
            RwClumpNode rwClumpNode;
            PakToolArchiveFile pak;

            aiCtx.SetConfig(new MaxBoneCountConfig(64));
            //aiScene = aiCtx.ImportFile(@"D:\Users\smart\Desktop\temp\BC001.FBX", Assimp.PostProcessSteps.SplitByBoneCount);
            aiScene = aiCtx.ImportFile(@"D:\Users\smart\Desktop\temp\test.FBX", Assimp.PostProcessSteps.SplitByBoneCount);
            //aiCtx.ExportFile(aiScene, @"D:\Users\smart\Desktop\temp\BC001_out.FBX", "fbx");
            //aiScene = aiCtx.ImportFile(@"D:\Users\smart\Desktop\temp\BC001_out.FBX");

            rmdScene = new RmdScene(@"D:\Modding\Persona 3 & 4\Persona4\CVM_DATA\MODEL\PACK\BC001.RMD");
            rwClumpNode = rmdScene.Clumps[0];
            //aiScene = RWScene.ToAssimpScene(rwClumpNode);

            using (var writer = File.CreateText("boneMap.txt"))
            {
                for (int i = 0; i < rwClumpNode.FrameList.Count; i++)
                {
                    int boneNameId = -1;
                    if (rwClumpNode.FrameList[i].HasHAnimExtension)
                        boneNameId = rwClumpNode.FrameList[i].HAnimFrameExtensionNode.NameId;

                    writer.WriteLine(
                        $"{i:D4} {rwClumpNode.FrameList.GetHierarchyIndexByFrameIndex(i):D4} {boneNameId:D4}");
                }
            }

            //aiCtx.ExportFile(aiScene, @"D:\Modding\Persona 3 & 4\Persona4\CVM_DATA\MODEL\PACK\BC001.DAE", "collada");
            //aiScene = aiCtx.ImportFile(@"D:\Modding\Persona 3 & 4\Persona4\CVM_DATA\MODEL\PACK\BC001.DAE");
            rwClumpNode.ReplaceGeometries(aiScene);

            /*
            var geometry = rwClumpNode.GeometryListNode[4];
            var matrix = rwClumpNode.Nodes[rwClumpNode.Atomics[4].FrameIndex].WorldTransform;
            Matrix4x4.Invert(matrix, out Matrix4x4 matrixInv);
            var matrixLocal = rwClumpNode.Nodes[rwClumpNode.Atomics[4].FrameIndex].Transform;
            Matrix4x4.Invert(matrixLocal, out Matrix4x4 matrixLocalInv);
            Matrix4x4.Invert(rwClumpNode.Nodes[0].Transform, out Matrix4x4 matrixInv2);
            Matrix4x4.Invert(matrix * matrixInv2, out matrixInv2);


            rwClumpNode.Atomics[4].FrameIndex = 0;

            for (int i = 0; i < geometry.Vertices.Length; i++)
                geometry.Vertices[i] = Vector3.Transform(geometry.Vertices[i], matrix);

            var skinPlugin = (RWSkinPlugin)geometry.ExtensionNodes.Find(x => x.Id == RWNodeType.SkinNode);
            var skinMatrices = rwClumpNode.AnimationRootBone.HAnimFrameExtensionNode.Hierarchy.Nodes.Select(x =>
            {
                return rwClumpNode.FrameNode.GetFrameByHierarchyIndex(x.Index).Transform;
            }).ToArray();

            for (int i = 0; i < skinPlugin.SkinToBoneMatrices.Length; i++)
            {
                //skinMatrices[i] = Matrix4x4.Transpose(skinMatrices[i]);

                //skinMatrices[i] = Matrix4x4.Multiply(matrixLocal, skinMatrices[i]);
                //Matrix4x4.Invert(skinMatrices[i], out Matrix4x4 invertedSkinMatrix);
                //skinPlugin.SkinToBoneMatrices[i] = invertedSkinMatrix;

                // works
                Matrix4x4.Invert(skinPlugin.SkinToBoneMatrices[i], out Matrix4x4 boneMatrix);
                boneMatrix *= matrix;
                Matrix4x4.Invert(boneMatrix, out boneMatrix);
                skinPlugin.SkinToBoneMatrices[i] = boneMatrix;
            }
            */

            //geometry.ExtensionNodes.Remove(geometry.ExtensionNodes.Find(x => x.Id == RWNodeType.SkinNode));

            rmdScene.Save(@"D:\Modding\Persona 3 & 4\Persona4\CVM_DATA\MODEL\PACK\BC001_new.RMD");

            pak = new PakToolArchiveFile(@"D:\Modding\Persona 3 & 4\Persona4\CVM_DATA\MODEL\PACK\BC001.PAC");
            pak.Entries.Find(x => x.Name.EndsWith("RMD")).Data = rmdScene.GetBytes();
            pak.Save(@"D:\Modding\Persona 3 & 4\Persona4\CVM_DATA\MODEL\PACK\BC001_new.PAC");


            //PAKToolArchiveFileUnsafe test = new PAKToolArchiveFileUnsafe(@"D:\Modding\Persona34\Persona4\CVM_DATA\MODEL\PACK\BC001.PAC");
            //ACXFile acx = new ACXFile(@"D:\Modding\Persona34\Persona4\bc001.acx");
            //CVMFileRewrite cvm = new CVMFileRewrite();
            //cvm.Load(@"D:\Modding\Persona34\Persona4\DVDROOT\DATA.CVM");
            //ReadMemoryMapped(@"D:\Modding\Persona34\Persona4\DVDROOT\DATA.CVM");

            //SetCulture();

            /*
            BMDFile bmd = new BMDFile(@"C:\Users\TGE\Downloads\Shin Megami Tensei Persona 3 FES [USA - English - PS2DVD]\CVM_DATA\HELP\DATMYTH.BMD");
            BMDDecompiler.Decompile(bmd, @"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\CVM_DATA\HELP\DATMYTH.msg");
            BMDScriptParser.CompileScript(@"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\CVM_DATA\HELP\DATMYTH.msg");
            */

            /*
            RMDScene rmd = new RMDScene(@"C:\Users\TGE\Downloads\Shin_Megami_Tensei_Persona_4_NTSC_PS2DVD-STRiKE.[www.usabit.com]\bc001.RMD");
            var split = new RWGeometryMaterialSplitMeshData(rmd.Clumps[0].GeometryListNode[4], RWPrimitiveType.TriangleStrip);
            var split2 = new RWGeometryMaterialSplitMeshData(rmd.Clumps[0].GeometryListNode[4], RWPrimitiveType.TriangleList);
            */

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
            RMDScene clumpNode = new RMDScene(pac.Entries.Find(entry => entry.Name.Contains(".RMD")).MeshListNode);

            Assimp.AssimpContext aiContext = new Assimp.AssimpContext();
            Assimp.ClumpNode aiScene = new Assimp.ClumpNode();
            aiScene.RootNode = new Assimp.Node("Root");

            foreach (RWClump clumpNode in clumpNode.Clumps)
            {
                int geoIdx = 0;
                foreach (RWGeometry geo in clumpNode.GeometryListNode)
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
                        Assimp.MaterialNode aiMaterial = new Assimp.MaterialNode();
                        aiMaterial.Name = "material_" + idx++;
                        aiScene.Materials.Add(aiMaterial);
                    }

                    aiScene.GeometryListNode.Add(aiMesh);
                }

                foreach (RWAtomic atomic in clumpNode.Atomics)
                {
                    Assimp.Node node = new Assimp.Node();
                    node.Name = aiScene.GeometryListNode[atomic.GeometryIndex].Name;
                    node.MeshIndices.Add(atomic.GeometryIndex);
                    node.Transform = clumpNode.FrameNode.FrameListNode[atomic.FrameIndex].WorldMatrix.ToAssimpMatrix4x4();
                    aiScene.RootNode.Children.Add(node);
                }
            }

            aiContext.ExportFile(aiScene, "test.dae", "collada");
            
            /*
            for (int i = 0; i < clumpNode.TextureDictionary.TextureCount; i++)
            {
                Bitmap bitmap = clumpNode.TextureDictionary.Textures[i].GetBitmap();
                string name = clumpNode.TextureDictionary.Textures[i].Name;

                //clumpNode.TextureDictionary.Textures[i] = new RWTextureNative(name, bitmap, PS2.Graphics.PixelFormat.PSMT8);
            }

            clumpNode.TextureDictionary.Save("_test.txd");

            PAKToolFile newPac = new PAKToolFile();
            newPac.Entries.Add(new PAKToolFileEntry("bc001.RMD", clumpNode.GetBytes()));
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
        //                    switch (subMesh.Id)
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

        private static void ReadMemoryMapped(string path)
        {
            FileInfo info = new FileInfo(path);
            long fileLength = info.Length;

            using (MemoryMappedFile mmapFile = MemoryMappedFile.CreateFromFile(path))
            {
                const int BLOCK_SIZE = 2048;
                int numBlocks = (int)(fileLength / BLOCK_SIZE);
                MemoryMappedViewAccessor accessor = null;

                for (int blockIndex = 0; blockIndex < numBlocks; blockIndex++)
                {
                    using (accessor = mmapFile.CreateViewAccessor(blockIndex * BLOCK_SIZE, BLOCK_SIZE))
                    {
                        //System.Console.WriteLine("Block index: {0}", blockIndex);
                    }
                }
            }
        }
    }
}
