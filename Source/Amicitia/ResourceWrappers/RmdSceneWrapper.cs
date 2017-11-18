using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using AtlusLibSharp.Graphics.RenderWare;
using AtlusLibSharp.PS2.Graphics;

namespace Amicitia.ResourceWrappers
{
    interface IRwNodeWrapper : IResourceWrapper
    {
        RwNodeId RwNodeId { get; }
    }

    public class RwNodeWrapperBase<TNode> : ResourceWrapper<TNode>, IRwNodeWrapper
        where TNode : RwNode
    {
        [Category("RenderWare Node Info")]
        public RwNodeId RwNodeId => Resource.Id;

        public RwNodeWrapperBase(string text, RwNode resource) : base(text, (TNode)resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.RwNode, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.RwNode, (res, path) => (TNode)RwNode.Load(path));
        }

        protected override void PopulateView()
        {
        }
    }

    public class RwNodeWrapper : RwNodeWrapperBase<RwNode>
    {
        public RwNodeWrapper(string text, RwNode resource) : base(text, resource)
        {
        }
    }

    public class RmdSceneWrapper : RwNodeWrapperBase<RmdScene>
    {
        [Browsable(false)]
        public RwTextureDictionaryNodeWrapper TextureDictionaryWrapper { get; private set; }

        [Browsable(false)]
        public RwClumpNodeListWrapper ClumpsWrapper { get; private set; }

        [Browsable(false)]
        public RmdNodeLinkListNodeWrapper NodeLinksWrapper { get; private set; }

        [Browsable(false)]
        public RmdAnimationsWrapper AnimationsWrapper { get; private set; }

        [Browsable(false)]
        public GenericListWrapper<RwNode> MiscNodesWrapper { get; private set; }

        public RmdSceneWrapper(string text, RmdScene resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.RmdScene, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.RmdScene, (res, path) => new RmdScene(path));
            RegisterRebuildAction((wrap) =>
            {
                var scene = new RmdScene();

                scene.TextureDictionary = TextureDictionaryWrapper.Resource;

                foreach (var node in ClumpsWrapper.Resource)
                {
                    scene.Clumps.Add(node);
                }

                foreach (var nodeLink in NodeLinksWrapper.Resource)
                {
                    scene.NodeLinks.Add(nodeLink);
                }

                foreach (var animationNodeList in AnimationsWrapper.Resource)
                {
                    scene.Animations.Add(animationNodeList);
                }

                foreach (var node in MiscNodesWrapper.Resource)
                {
                    scene.MiscNodes.Add(node);
                }
                
                return scene;
            });
        }

        protected override void PopulateView()
        {
            TextureDictionaryWrapper = new RwTextureDictionaryNodeWrapper("Textures", Resource.TextureDictionary ?? new RwTextureDictionaryNode());
            ClumpsWrapper = new RwClumpNodeListWrapper("Clumps", Resource.Clumps ?? new List<RwClumpNode>());
            NodeLinksWrapper = new RmdNodeLinkListNodeWrapper("NodeLinks", Resource.NodeLinks ?? new RmdNodeLinkListNode());
            AnimationsWrapper = new RmdAnimationsWrapper("Animations", Resource.Animations ?? new List<RmdAnimation>());
            MiscNodesWrapper = new GenericListWrapper<RwNode>("MiscNodes", Resource.MiscNodes ?? new List<RwNode>(), (e, i) => e.Id.ToString());

            Nodes.Add(TextureDictionaryWrapper);
            Nodes.Add(ClumpsWrapper);
            Nodes.Add(NodeLinksWrapper);
            Nodes.Add(AnimationsWrapper);
            Nodes.Add(MiscNodesWrapper);
        }
    }

    public class RwTextureDictionaryNodeWrapper : RwNodeWrapperBase<RwTextureDictionaryNode>
    {
        public RwDeviceId DeviceId => Resource.DeviceId;

        public int TextureCount => Resource.TextureCount;

        public RwTextureDictionaryNodeWrapper(string text, RwTextureDictionaryNode resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.RwTextureDictionaryNode, (res, path) => res.Save(path));
            RegisterCustomAction( "Export All", ( o, s ) =>
            {
                using ( FolderBrowserDialog dialog = new FolderBrowserDialog() )
                {
                    dialog.ShowNewFolderButton = true;

                    if ( dialog.ShowDialog() != DialogResult.OK )
                    {
                        return;
                    }

                    foreach ( var texture in Resource.Textures )
                    {
                        var path = Path.Combine( dialog.SelectedPath, Path.ChangeExtension( texture.Name, "png" ) );
                        var bitmap = texture.GetBitmap();

                        bitmap.Save( path );
                    }
                }
            },
            0 );
            RegisterFileReplaceAction(SupportedFileType.RwTextureDictionaryNode, (res, path) => (RwTextureDictionaryNode)RwNode.Load(path));
            RegisterFileAddAction(SupportedFileType.RwTextureNativeNode, DefaultFileAddAction);
            RegisterFileAddAction(SupportedFileType.Bitmap, (path, wrap) =>
            {
                var resWrap = (BitmapWrapper)ResourceWrapperFactory.GetResourceWrapper(path);
                var name = Path.GetFileNameWithoutExtension(path);
                wrap.Nodes.Add(new RwTextureNativeNodeWrapper(new RwTextureNativeNode(name, resWrap.Resource)));
            });
            RegisterRebuildAction(wrap =>
            {
                var textureDictionary = new RwTextureDictionaryNode();

                foreach (RwTextureNativeNodeWrapper textureNodeWrapper in Nodes)
                {
                    textureDictionary.Textures.Add(textureNodeWrapper.Resource);
                }

                return textureDictionary;
            });
        }

        protected override void PopulateView()
        {
            foreach (var texture in Resource.Textures)
            {
                Nodes.Add(new RwTextureNativeNodeWrapper(texture));
            }
        }
    }

    public class RwTextureNativeNodeWrapper : RwNodeWrapperBase<RwTextureNativeNode>
    {
        public bool IsIndexed => Resource.IsIndexed;

        public RwPlatformId PlatformId => Resource.PlatformId;

        public PS2FilterMode FilterMode
        {
            get => Resource.FilterMode;
            set => SetProperty(Resource, value);
        }

        public PS2AddressingMode HorrizontalAddressingMode
        {
            get => Resource.HorrizontalAddressingMode;
            set => SetProperty(Resource, value);
        }

        public PS2AddressingMode VerticalAddressingMode
        {
            get => Resource.VerticalAddressingMode;
            set => SetProperty(Resource, value);
        }

        public string TextureName
        {
            get => Resource.Name;
            set => SetProperty(Resource, value, false, nameof( Resource.Name ) );
        }

        public string TextureMaskName
        {
            get => Resource.MaskName;
            set => SetProperty(Resource, value, false, nameof( Resource.MaskName ) );
        }

        public int Width => Resource.Width;

        public int Height => Resource.Height;

        public int Depth => Resource.Depth;

        public RwRasterFormats Format => Resource.Format;

        public RwTextureNativeNodeWrapper(string text, RwTextureNativeNode resource) : base(resource.Name, resource)
        {
        }

        public RwTextureNativeNodeWrapper(RwTextureNativeNode resource) : base(resource.Name, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.RwTextureNativeNode, (res, path) => res.Save(path));
            RegisterFileExportAction(SupportedFileType.Bitmap, (res, path) => res.GetBitmap().Save(path, ImageFormat.Png));
            RegisterFileReplaceAction(SupportedFileType.RwTextureNativeNode, (res, path) => (RwTextureNativeNode)RwNode.Load(path));
            RegisterFileReplaceAction(SupportedFileType.Bitmap, (res, path) => new RwTextureNativeNode(res.Name, new Bitmap(path)));
            RegisterRebuildAction((wrap) =>
            {
                wrap.Resource.Name = wrap.Text;
                return wrap.Resource;
            });
        }
    }

    public class RwClumpNodeListWrapper : GenericListWrapper<RwClumpNode>
    {
        public RwClumpNodeListWrapper(string text, List<RwClumpNode> resource) 
            : base(text, resource, (e, i) => $"Clump{i:0}")
        {
        }

        protected override void PostInitialize()
        {
            RegisterCustomAction( "Export All", ( o, s ) =>
            {
                using ( FolderBrowserDialog dialog = new FolderBrowserDialog() )
                {
                    dialog.ShowNewFolderButton = true;

                    if ( dialog.ShowDialog() != DialogResult.OK )
                    {
                        return;
                    }   

                    foreach ( RwClumpNodeWrapper clumpNode in Nodes )
                    {
                        var path = Path.Combine( dialog.SelectedPath, Path.ChangeExtension( clumpNode.Text, "dae" ) );
                        clumpNode.Export( path, SupportedFileType.AssimpModelFile );
                    }
                }
            },
            0 );
        }
    }

    public class RwClumpNodeWrapper : RwNodeWrapperBase<RwClumpNode>
    {
        [Browsable(false)]
        public GenericListWrapper<RwFrame> FrameListWrapper { get; private set; }

        [Browsable(false)]
        public GenericListWrapper<RwGeometryNode> GeometryListWrapper { get; private set; }

        [Browsable(false)]
        public GenericListWrapper<RwAtomicNode> AtomicsWrapper { get; private set; }

        [Browsable(false)]
        public GenericListWrapper<RwNode> ExtensionsWrapper { get; private set; }

        public int AtomicCount   => Resource.AtomicCount;

        public int LightCount    => Resource.LightCount;

        public int CameraCount   => Resource.CameraCount;

        public int GeometryCount => Resource.GeometryCount;

        public RwClumpNodeWrapper(string text, RwClumpNode resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.RwClumpNode, (res, path) => res.Save(path));
            RegisterFileExportAction(SupportedFileType.AssimpModelFile, (res, path) =>
            {
                var scene = RwClumpNode.ToAssimpScene(res);

                using ( var ctx = new Assimp.AssimpContext() )
                {
                    var exportFormats = ctx.GetSupportedExportFormats();
                    var extension = Path.GetExtension( path ).TrimStart('.');
                    var exportFormat = exportFormats.SingleOrDefault( x => x.FileExtension.Equals( extension, System.StringComparison.InvariantCultureIgnoreCase ) );

#if !DEBUG
                    if ( exportFormat == null )
                        throw new System.Exception( "Unsupported Assimp Export Format" );
#else
                    if ( exportFormat == null )
                    {
                        System.Console.WriteLine( "Unsupported Assimp Export Format" );
                        return;
                    }
#endif

                    ctx.ExportFile( scene, path, exportFormat.FormatId, Assimp.PostProcessSteps.FlipUVs | Assimp.PostProcessSteps.JoinIdenticalVertices);
                }
            });
            RegisterFileReplaceAction(SupportedFileType.RwClumpNode, (res, path) => (RwClumpNode) RwNode.Load(path));
            RegisterFileReplaceAction(SupportedFileType.AssimpModelFile, (res, path) =>
            {
                using ( var ctx = new Assimp.AssimpContext() )
                {
                    ctx.SetConfig( new Assimp.Configs.MaxBoneCountConfig( 64 ) );
                    var scene = ctx.ImportFile( path, Assimp.PostProcessSteps.SplitByBoneCount | Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.FlipUVs | 
                                                      Assimp.PostProcessSteps.FindInvalidData | Assimp.PostProcessSteps.GenerateSmoothNormals | Assimp.PostProcessSteps.GenerateUVCoords | Assimp.PostProcessSteps.ImproveCacheLocality |
                                                      Assimp.PostProcessSteps.JoinIdenticalVertices | Assimp.PostProcessSteps.LimitBoneWeights | Assimp.PostProcessSteps.OptimizeMeshes );
                    res.ReplaceGeometries( scene );
                    return res;
                }
            });
            RegisterRebuildAction((wrap) =>
            {
                var resource = new RwClumpNode(Resource.Parent);

                for (var i = 0; i < FrameListWrapper.Nodes.Count; i++)
                {
                    var node = (RwFrameWrapper) FrameListWrapper.Nodes[i];
                    resource.FrameList.Add(node.Resource);
                    resource.FrameList.Extensions[i] = Resource.FrameList.Extensions[i];
                }

                foreach (RwGeometryNodeWrapper node in GeometryListWrapper.Nodes)
                {
                    resource.GeometryList.Add(node.Resource);
                }

                foreach (RwAtomicNodeWrapper node in AtomicsWrapper.Nodes)
                {
                    resource.Atomics.Add(node.Resource);
                }

                foreach (IResourceWrapper node in ExtensionsWrapper.Nodes)
                {
                    resource.Extensions.Add((RwNode)node.Resource);
                }

                return resource;
            });
        }

        protected override void PopulateView()
        {
            FrameListWrapper = new GenericListWrapper<RwFrame>("Frames", Resource.FrameList, (e, i) => e.HAnimFrameExtensionNode?.NameId.ToString() ?? "Root");

            GeometryListWrapper =
                new GenericListWrapper<RwGeometryNode>("Geometries", Resource.GeometryList,
                    (e, i) => $"Geometry{i:00}");

            AtomicsWrapper = new GenericListWrapper<RwAtomicNode>("Atomics", Resource.Atomics, (e, i) => $"Atomic{i:00}");

            ExtensionsWrapper = new GenericListWrapper<RwNode>("Extensions", Resource.Extensions ?? new List<RwNode>(), (e, i) => $"Extension{i:00}");

            Nodes.Add(FrameListWrapper);
            Nodes.Add(GeometryListWrapper);
            Nodes.Add(AtomicsWrapper);
            Nodes.Add(ExtensionsWrapper);
        }
    }

    public class RwFrameWrapper : ResourceWrapper<RwFrame>
    {
        public Matrix4x4 Transform
        {
            get => Resource.Transform;
            set => SetProperty(Resource, value);
        }

        public int UserFlags
        {
            get => Resource.UserFlags;
            set => SetProperty(Resource, value);
        }

        public RwFrameWrapper(string text, RwFrame resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;
        }

        protected override void PopulateView()
        {
        }
    }

    public class RwGeometryNodeWrapper : RwNodeWrapperBase<RwGeometryNode>
    {
        [Browsable(false)]
        public GenericListWrapper<Vector3> VerticesWrapper { get; }

        [Browsable(false)]
        public GenericListWrapper<Vector3> NormalsWrapper { get; }

        [Browsable(false)]
        public GenericListWrapper<Color> ColorsWrapper { get; }

        [Browsable(false)]
        public GenericListWrapper<GenericListWrapper<Vector2>> TextureCoordinateChannelsWrapper { get; }

        [Browsable(false)]
        public GenericListWrapper<RwTriangle> TrianglesWrapper { get; }

        [Browsable(false)]
        public GenericListWrapper<RwNode> ExtensionWrapper { get; }

        public RwGeometryFlags Flags => Resource.Flags;

        public int TextureCoordinateChannelCount => Resource.TextureCoordinateChannelCount;

        public RwGeometryNativeFlag NativeFlag => Resource.NativeFlag;

        public int TriangleCount => Resource.TriangleCount;

        public int VertexCount => Resource.VertexCount;

        public Vector3 BoundingSphereCenter => Resource.BoundingSphere.Center;

        public float BoundingSphereRadius => Resource.BoundingSphere.Radius;

        public RwGeometryNodeWrapper(string text, RwGeometryNode resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;
        }
    }

    public class RwAtomicNodeWrapper : RwNodeWrapperBase<RwAtomicNode>
    {
        public int FrameIndex
        {
            get => Resource.FrameIndex;
            set => SetProperty(Resource, value);
        }

        public int GeometryIndex
        {
            get => Resource.GeometryIndex;
            set => SetProperty(Resource, value);
        }

        public int Flag1
        {
            get => Resource.Flag1;
            set => SetProperty(Resource, value);
        }

        public int Flag2
        {
            get => Resource.Flag2;
            set => SetProperty(Resource, value);
        }

        [Browsable(false)]
        public GenericListWrapper<RwNode> ExtensionsWrapper { get; private set; }

        public RwAtomicNodeWrapper(string text, RwAtomicNode resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Move | CommonContextMenuOptions.Delete;

            RegisterRebuildAction(wrap =>
            {
                var atomic = wrap.Resource;
                atomic.Extensions.Clear();
                atomic.Extensions.AddRange(ExtensionsWrapper.Resource);
                return atomic;
            });
        }

        protected override void PopulateView()
        {
            ExtensionsWrapper = new GenericListWrapper<RwNode>("Extensions", Resource.Extensions ?? new List<RwNode>(), (e, i) => $"Extension{i:00}");

            Nodes.Add(ExtensionsWrapper);
        }
    }

    public class RmdNodeLinkListNodeWrapper : GenericListWrapper<RmdNodeLink>
    {
        public RmdNodeLinkListNodeWrapper(string text, IList<RmdNodeLink> resource)
            : base(text, resource, (e, i) => $"NodeLink{i:0}")
        {
        }
    }

    public class RmdNodeLinkWrapper : ResourceWrapper<RmdNodeLink>
    {
        public int SourceNodeId
        {
            get => Resource.SourceNodeId;
            set => SetProperty(Resource, value);
        }

        public int TargetNodeId
        {
            get => Resource.TargetNodeId;
            set => SetProperty(Resource, value);
        }

        public Matrix4x4 Matrix
        {
            get => Resource.Matrix;
            set => SetProperty(Resource, value);
        }

        public RmdNodeLinkWrapper(string text, RmdNodeLink resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.RmdNodeLink, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.RmdNodeLink, (res, path) => new RmdNodeLink(path));
        }

        protected override void PopulateView()
        {
        }
    }

    public class RmdAnimationsWrapper : GenericListWrapper<RmdAnimation>
    {
        public RmdAnimationsWrapper(string text, IList<RmdAnimation> resource) 
            : base(text, resource, (e, i) => $"Animation{i:00}")
        {
        }
    }

    public class RmdAnimationWrapper : ResourceWrapper<RmdAnimation>
    {
        public RmdAnimationWrapper(string text, RmdAnimation resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.RmdAnimation, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.RmdAnimation, (res, path) => (RmdAnimation)RwNode.Load(path));
            RegisterFileAddAction(SupportedFileType.RmdAnimation, DefaultFileAddAction);
            RegisterRebuildAction(wrap =>
            {
                var nodes = new List<RwNode>();
                foreach (IResourceWrapper node in Nodes)
                {
                    nodes.Add((RwNode)node.Resource);
                }

                return new RmdAnimation(nodes, wrap.Resource.Parent);
            });          
        }

        protected override void PopulateView()
        {
            for (var i = 0; i < Resource.Count; i++)
            {
                var node = Resource[i];

                if ( node is RwAnimationNode animationNode )
                {
                    Nodes.Add( new RwAnimationNodeWrapper( $"AnimationNode{i:00}", animationNode ) );
                }
                else
                {
                    Nodes.Add( new RwNodeWrapperBase<RwNode>( $"Node{i:00}", node ) );
                }
            }
        }
    }

    public class RwAnimationNodeWrapper : ResourceWrapper<RwAnimationNode>
    {
        public int Version
        {
            get => Resource.Version;
            set => SetProperty( Resource, value );
        }

        public RwKeyFrameType KeyFrameType
        {
            get => Resource.KeyFrameType;
            set => SetProperty( Resource, value );
        }

        public int Flags
        {
            get => Resource.Flags;
            set => SetProperty( Resource, value );
        }

        public float Duration
        {
            get => Resource.Duration;
            set => SetProperty( Resource, value );
        }

        public RwAnimationNodeWrapper( string text, RwAnimationNode resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterCustomAction( "Change speed", ( o, s ) =>
            {
                using ( var dialog = new SimpleValueInputDialog( "Enter a speed value", "Speed multiplier", "1.0" ) )
                {
                    if ( dialog.ShowDialog() != DialogResult.OK )
                        return;

                    if ( !float.TryParse( dialog.ValueText, out var multiplier ) )
                        return;

                    Duration *= multiplier;
                    foreach ( var keyFrame in Resource.KeyFrames )
                    {
                        keyFrame.Time *= multiplier;
                    }

                }
            }, Keys.None );
            RegisterFileExportAction( SupportedFileType.RwAnimationNode, ( res, path ) => res.Save( path ) );
            RegisterFileExportAction( SupportedFileType.AssimpModelFile, ( res, path ) =>
            {
                var scene = ( RmdScene )res.FindParentNode( RwNodeId.RmdSceneNode );
                var clump = scene.Clumps.FirstOrDefault();
                if ( clump == null )
                    return;

                RwAnimationNode.SaveToCollada( res, clump.FrameList, path );
            } );
            RegisterFileReplaceAction( SupportedFileType.RwAnimationNode, ( res, path ) => ( RwAnimationNode ) RwNode.Load( path ) );
            RegisterFileReplaceAction( SupportedFileType.AssimpModelFile, ( res, path ) =>
            {
                var scene = (RmdScene)res.FindParentNode( RwNodeId.RmdSceneNode );
                var clump = scene.Clumps.FirstOrDefault();
                if ( clump == null )
                    return res;

                return RwAnimationNode.FromAssimpScene( res.Parent, clump.FrameList, path );
            });
        }

        protected override void PopulateView()
        {
        }
    }
}
