using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using AtlusLibSharp.Utilities;

namespace AtlusLibSharp.Graphics.RenderWare
{
    public class RwAnimationNode : RwNode
    {
        public const int VERSION = 0x100;
        public const float FPS = 30f;

        private const int UNCOMPRESSED_KEYFRAME_SIZE = 36;
        private const int COMPRESSED_KEYFRAME_SIZE = 24;

        public int Version { get; set; }

        public RwKeyFrameType KeyFrameType { get; set; }

        public List<RwKeyFrame> KeyFrames { get; private set; }

        public int Flags { get; set; }

        public float Duration { get; set; }

        private RwCompressedKeyFrameData mCachedCompressedKeyframeData;

        public RwAnimationNode( RwNode parent = null ) : base( RwNodeId.RwAnimationNode, parent )
        {
            Version = VERSION;
            KeyFrameType = RwKeyFrameType.Uncompressed;
            KeyFrames = new List<RwKeyFrame>();
            Flags = 0;
        }

        public RwAnimationNode(RwNode parent, RwKeyFrameType keyFrameType, float duration) : 
            base(RwNodeId.RwAnimationNode, parent)
        {
            Version = VERSION;
            KeyFrameType = keyFrameType;
            KeyFrames = new List<RwKeyFrame>();
            Flags = 0;
            Duration = duration;
        }

        /// <summary>
        /// Initialize a new <see cref="RwAnimationNode"/> instance with a <see cref="Stream"/> containing <see cref="RwAnimationNode"/> data.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing <see cref="RwAnimationNode"/> data.</param>
        /// <param name="leaveOpen">Option to leave the <see cref="Stream"/> open or dispose it after loading the <see cref="RwAnimationNode"/>.</param>
        public RwAnimationNode( Stream stream, bool leaveOpen = false ) : base(RwNodeId.RwAnimationNode)
        {
            stream.Position = 12;
            using ( var reader = new BinaryReader( stream, Encoding.Default, leaveOpen ) )
                ReadBody( reader );
        }

        internal RwAnimationNode( RwNodeFactory.RwNodeHeader header, BinaryReader reader ) : base(header)
        {
            ReadBody( reader );
        }

        private static Quaternion ToQuaternion( Assimp.Quaternion quaternion )
        {
            return Unsafe.As< Assimp.Quaternion, Quaternion >( ref quaternion );
        }

        private static Vector3 ToVector3( Assimp.Vector3D vector )
        {
            return Unsafe.As< Assimp.Vector3D, Vector3 >( ref vector );
        }

        public static RwAnimationNode FromAssimpScene( RwNode parent, RwFrameListNode frameList, string path )
        {
            var aiContext = new Assimp.AssimpContext();
            var aiScene = aiContext.ImportFile( path );
            var aiAnimation = aiScene.Animations.FirstOrDefault();
            RwAnimationNode animationNode;

            if ( aiAnimation != null )
            {
                animationNode = new RwAnimationNode( parent, RwKeyFrameType.Uncompressed, ( float ) ( aiAnimation.DurationInTicks / aiAnimation.TicksPerSecond ) );
                var nodeNameToHAnimId = aiAnimation.NodeAnimationChannels.ToDictionary( x => x.NodeName, x => frameList.GetNameIdByName( x.NodeName ) );
                var nodeKeyframeTimes = aiAnimation.NodeAnimationChannels.SelectMany( x => x.PositionKeys )
                                                   .Select( x => x.Time )
                                                   .Concat( aiAnimation.NodeAnimationChannels.SelectMany( x => x.RotationKeys.Select( y => y.Time ) ) )
                                                   .Distinct()
                                                   .OrderBy( x => x )
                                                   .ToList();

                var previousKeyFrames = new Dictionary< int, RwKeyFrame >();
                var nodeKeyFrames = new Dictionary< int, List< RwKeyFrame > >();

                // Add initial pose
                foreach ( var hierarchyNode in frameList.AnimationRootNode.HAnimFrameExtensionNode.Hierarchy.Nodes )
                {
                    var frame = frameList[ frameList.GetFrameIndexByNameId( hierarchyNode.NodeId ) ];
                    var firstRotation = Quaternion.CreateFromRotationMatrix( frame.Transform );
                    var firstTranslation = frame.Transform.Translation;

                    var channel = aiAnimation.NodeAnimationChannels.FirstOrDefault( x => nodeNameToHAnimId[ x.NodeName ] == hierarchyNode.NodeId );
                    if ( channel != null )
                    {
                        if ( channel.HasRotationKeys )
                        {
                            firstRotation = ToQuaternion( channel.RotationKeys.First().Value );
                        }

                        if ( channel.HasPositionKeys )
                        {
                            firstTranslation = ToVector3( channel.PositionKeys.First().Value );
                        }
                    }

                    var keyFrame = new RwKeyFrame( 0, firstRotation, firstTranslation, null );
                    animationNode.KeyFrames.Add( keyFrame );

                    previousKeyFrames[ hierarchyNode.NodeId ] = keyFrame;
                    nodeKeyFrames[ hierarchyNode.NodeId ] = new List< RwKeyFrame >();
                }

                foreach ( var keyFrameTime in nodeKeyframeTimes )
                {
                    if ( keyFrameTime == 0.0f )
                        continue;

                    foreach ( var channel in aiAnimation.NodeAnimationChannels )
                    {
                        if ( !channel.HasPositionKeys && !channel.HasRotationKeys )
                            continue;

                        if ( !channel.RotationKeys.Any( x => x.Time == keyFrameTime ) || !channel.PositionKeys.Any( x => x.Time == keyFrameTime ) )
                            continue;

                        var hierarchAnimNodeId = nodeNameToHAnimId[ channel.NodeName ];
                        var previousKeyFrame = previousKeyFrames[ hierarchAnimNodeId ];
                        var rotation = previousKeyFrame.Rotation;
                        var translation = previousKeyFrame.Translation;

                        var rotationKeys = channel.RotationKeys.Where( x => x.Time == keyFrameTime );
                        if ( rotationKeys.Any() )
                        {
                            var aiRotation = rotationKeys.First().Value;
                            rotation = new Quaternion( aiRotation.X, aiRotation.Y, aiRotation.Z, aiRotation.W );
                        }

                        var translationKeys = channel.PositionKeys.Where( x => x.Time == keyFrameTime );
                        if ( translationKeys.Any() )
                        {
                            var aiTranslation = translationKeys.First().Value;
                            translation = new Vector3( aiTranslation.X, aiTranslation.Y, aiTranslation.Z );
                        }

                        var keyFrame = new RwKeyFrame( ( float ) ( keyFrameTime / aiAnimation.TicksPerSecond ), rotation, translation, previousKeyFrame );
                        nodeKeyFrames[ hierarchAnimNodeId ].Add( keyFrame );
                        previousKeyFrames[ hierarchAnimNodeId ] = keyFrame;
                    }
                }

                while ( !nodeKeyFrames.All( x => x.Value.Count == 0 ) )
                {
                    foreach ( var kvp in nodeKeyFrames )
                    {
                        if ( animationNode.KeyFrames.Count == 0 )
                            continue;

                        var keyFrame = kvp.Value.First();
                        animationNode.KeyFrames.Add( keyFrame );
                        kvp.Value.Remove( keyFrame );

                        if ( animationNode.KeyFrames.Count == 0 )
                        {
                            var previousKeyFrame = previousKeyFrames[ kvp.Key ];
                            if ( previousKeyFrame.Time != animationNode.Duration )
                            {
                                var lastRotation = previousKeyFrame.Rotation;
                                var lastTranslation = previousKeyFrame.Translation;
                                var channel = aiAnimation.NodeAnimationChannels.SingleOrDefault( x => nodeNameToHAnimId[ x.NodeName ] == kvp.Key );
                                if ( channel != null )
                                {
                                    if ( channel.HasRotationKeys )
                                        lastRotation = ToQuaternion( channel.RotationKeys.Last().Value );

                                    if ( channel.HasPositionKeys )
                                        lastTranslation = ToVector3( channel.PositionKeys.Last().Value );
                                }

                                animationNode.KeyFrames.Add( new RwKeyFrame( animationNode.Duration, lastRotation, lastTranslation, previousKeyFrame ) );
                            }
                        }
                    }
                }
            }
            else
            {
                animationNode = new RwAnimationNode( null, RwKeyFrameType.Uncompressed, 0f );
            }

            return animationNode;
        }

        public static Assimp.Scene ToAssimpScene( RwAnimationNode animation, RwFrameListNode frameList )
        {
            var aiScene = new Assimp.Scene();

            // RootNode
            var rootFrame = frameList[0];
            var aiRootNode = new Assimp.Node( "RootNode", null );
            aiRootNode.Transform = new Assimp.Matrix4x4( rootFrame.Transform.M11, rootFrame.Transform.M21, rootFrame.Transform.M31, rootFrame.Transform.M41,
                                                         rootFrame.Transform.M12, rootFrame.Transform.M22, rootFrame.Transform.M32, rootFrame.Transform.M42,
                                                         rootFrame.Transform.M13, rootFrame.Transform.M23, rootFrame.Transform.M33, rootFrame.Transform.M43,
                                                         rootFrame.Transform.M14, rootFrame.Transform.M24, rootFrame.Transform.M34, rootFrame.Transform.M44 );

            aiScene.RootNode = aiRootNode;

            for ( int i = 1; i < frameList.Count; i++ )
            {
                var frame = frameList[i];
                var frameName = "_" + frame.HAnimFrameExtensionNode.NameId;

                Assimp.Node aiParentNode = null;
                if ( frame.Parent != null )
                {
                    string parentName = "RootNode";
                    if ( frame.Parent.HasHAnimExtension )
                    {
                        parentName = "_" + frame.Parent.HAnimFrameExtensionNode.NameId;
                    }

                    aiParentNode = aiRootNode.FindNode( parentName );
                }

                var aiNode = new Assimp.Node( frameName, aiParentNode );
                aiNode.Transform = new Assimp.Matrix4x4( frame.Transform.M11, frame.Transform.M21, frame.Transform.M31, frame.Transform.M41,
                                                         frame.Transform.M12, frame.Transform.M22, frame.Transform.M32, frame.Transform.M42,
                                                         frame.Transform.M13, frame.Transform.M23, frame.Transform.M33, frame.Transform.M43,
                                                         frame.Transform.M14, frame.Transform.M24, frame.Transform.M34, frame.Transform.M44 );
                aiParentNode.Children.Add( aiNode );
            }

            var aiAnimation = new Assimp.Animation();
            aiAnimation.TicksPerSecond = FPS;
            aiAnimation.DurationInTicks = animation.Duration * FPS;
            aiAnimation.Name = "Take 01";
            aiScene.Animations.Add( aiAnimation );

            var keyFrameToNodeAnimationChannel = new Dictionary< RwKeyFrame, Assimp.NodeAnimationChannel>();
            for ( var i = 0; i < frameList.AnimationRootNode.HAnimFrameExtensionNode.Hierarchy.Nodes.Count; i++ )
            {
                var hierarchyNode = frameList.AnimationRootNode.HAnimFrameExtensionNode.Hierarchy.Nodes[ i ];
                var keyFrame = animation.KeyFrames[ i ];

                // Create channel
                var aiNodeAnimationChannel = new Assimp.NodeAnimationChannel();
                aiNodeAnimationChannel.NodeName = "_" + hierarchyNode.NodeId;
                aiNodeAnimationChannel.PostState = Assimp.AnimationBehaviour.Default;
                aiNodeAnimationChannel.PreState = Assimp.AnimationBehaviour.Default;
                aiNodeAnimationChannel.PositionKeys.Add( new Assimp.VectorKey( 0, new Assimp.Vector3D( keyFrame.Translation.X, keyFrame.Translation.Y, keyFrame.Translation.Z ) ) );
                aiNodeAnimationChannel.RotationKeys.Add( new Assimp.QuaternionKey( 0, new Assimp.Quaternion( keyFrame.Rotation.X, keyFrame.Rotation.Y, keyFrame.Rotation.Z, keyFrame.Rotation.W ) ) );
                aiNodeAnimationChannel.ScalingKeys.Add( new Assimp.VectorKey( 0, new Assimp.Vector3D( 1, 1, 1 ) ) );
                aiNodeAnimationChannel.ScalingKeys.Add( new Assimp.VectorKey( Math.Round(animation.Duration * FPS), new Assimp.Vector3D( 1, 1, 1 ) ) );

                keyFrameToNodeAnimationChannel[keyFrame] = aiNodeAnimationChannel;
                aiAnimation.NodeAnimationChannels.Add( aiNodeAnimationChannel );
            }

            for ( int i = frameList.AnimationRootNode.HAnimFrameExtensionNode.Hierarchy.Nodes.Count; i < animation.KeyFrames.Count; i++ )
            {
                var keyFrame = animation.KeyFrames[i];

                // Add keys
                var aiNodeAnimationChannel = keyFrameToNodeAnimationChannel[ keyFrame.Previous ];
                aiNodeAnimationChannel.PositionKeys.Add( new Assimp.VectorKey( keyFrame.Time * FPS , new Assimp.Vector3D( keyFrame.Translation.X, keyFrame.Translation.Y, keyFrame.Translation.Z ) ) );
                aiNodeAnimationChannel.RotationKeys.Add( new Assimp.QuaternionKey( keyFrame.Time * FPS, new Assimp.Quaternion( keyFrame.Rotation.X, keyFrame.Rotation.Y, keyFrame.Rotation.Z, keyFrame.Rotation.W ) ) );

                keyFrameToNodeAnimationChannel[keyFrame] = aiNodeAnimationChannel;
            }

            return aiScene;
        }

        public static void SaveToCollada( RwAnimationNode animation, RwFrameListNode frameList, string path )
        {
            var aiScene = ToAssimpScene( animation, frameList );
 
            using ( var aiContext = new Assimp.AssimpContext() )
            {
                if ( !aiContext.ExportFile( aiScene, path, "collada" ) )
                {
                    throw new Exception( "Failed to export" );
                }
            }            
        }

        protected internal override void ReadBody( BinaryReader reader )
        {
            ReadHeader( reader );
            ReadKeyFrames( reader );
        }

        protected internal override void WriteBody( BinaryWriter writer )
        {
            WriteHeader( writer );
            WriteKeyFrames( writer );
        }

        private void ReadHeader( BinaryReader reader )
        {
            Version = reader.ReadInt32();
            KeyFrameType = (RwKeyFrameType)reader.ReadInt32();
            int keyFrameCount = reader.ReadInt32();
            Flags = reader.ReadInt32();
            Duration = reader.ReadSingle();

            KeyFrames = new List<RwKeyFrame>( keyFrameCount );
        }

        private void ReadKeyFrames( BinaryReader reader )
        {
            switch ( KeyFrameType )
            {
                case RwKeyFrameType.Uncompressed:
                    ReadUncompressedKeyFrames( reader );
                    break;

                case RwKeyFrameType.Compressed:
                    mCachedCompressedKeyframeData = ReadCompressedKeyFrames( reader );
                    DecompressKeyframes( mCachedCompressedKeyframeData.KeyFrames, mCachedCompressedKeyframeData.CustomData );
                    break;
            }
        }

        private void ReadUncompressedKeyFrames( BinaryReader reader )
        {
            var keyFrameByOffsetMap = new Dictionary<int, RwKeyFrame>();
            for ( int i = 0; i < KeyFrames.Capacity; i++ )
            {
                var keyFrame = new RwKeyFrame
                {
                    Time = reader.ReadSingle(),
                    Rotation = new Quaternion( reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() ),
                    Translation = new Vector3( reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                };

                int previousKeyFrameOffset = reader.ReadInt32();

                if ( keyFrame.Time != 0.0f )
                {
                    keyFrame.Previous = keyFrameByOffsetMap[previousKeyFrameOffset];
                }

                keyFrameByOffsetMap[i * UNCOMPRESSED_KEYFRAME_SIZE] = keyFrame;

                KeyFrames.Add( keyFrame );
            }
        }

        // Writing compressed & decompressing keyframes
        private RwCompressedKeyFrameData ReadCompressedKeyFrames( BinaryReader reader )
        {          
            var compressedKeyFrames = new List<RwCompressedKeyFrame>();

            for ( int i = 0; i < KeyFrames.Capacity; i++ )
            {
                var compressedKeyFrame = new RwCompressedKeyFrame
                {
                    Time = reader.ReadSingle(),
                    RotationX = reader.ReadUInt16(),
                    RotationY = reader.ReadUInt16(),
                    RotationZ = reader.ReadUInt16(),
                    RotationW = reader.ReadUInt16(),
                    TranslationX = reader.ReadUInt16(),
                    TranslationY = reader.ReadUInt16(),
                    TranslationZ = reader.ReadUInt16(),
                    PreviousFrameOffset = reader.ReadInt32()
                };

                compressedKeyFrames.Add( compressedKeyFrame );
            }

            var customData = new RwCompressedKeyFrameCustomData
            {
                Offset = reader.ReadVector3(),
                Scalar = reader.ReadVector3()
            };

            return new RwCompressedKeyFrameData( compressedKeyFrames, customData );
        }

        private void DecompressKeyframes( List<RwCompressedKeyFrame> compressedKeyFrames, RwCompressedKeyFrameCustomData customData )
        {
            var keyFrameByOffsetMap = new Dictionary<int, RwKeyFrame>();

            for ( int i = 0; i < compressedKeyFrames.Count; i++ )
            {
                var compressedKeyFrame = compressedKeyFrames[i];

                var keyFrame = new RwKeyFrame
                {
                    Time = compressedKeyFrame.Time,
                    Rotation = new Quaternion
                    (
                        DecompressFloat( compressedKeyFrame.RotationX ),
                        DecompressFloat( compressedKeyFrame.RotationY ),
                        DecompressFloat( compressedKeyFrame.RotationZ ),
                        DecompressFloat( compressedKeyFrame.RotationW )
                    ),
                    Translation = new Vector3
                    (
                        ( DecompressFloat( compressedKeyFrame.TranslationX ) * customData.Scalar.X ) +
                        customData.Offset.X,
                        ( DecompressFloat( compressedKeyFrame.TranslationY ) * customData.Scalar.Y ) +
                        customData.Offset.Y,
                        ( DecompressFloat( compressedKeyFrame.TranslationZ ) * customData.Scalar.Z ) +
                        customData.Offset.Z
                    )
                };

                if ( keyFrame.Time != 0.0f )
                {
                    keyFrame.Previous = keyFrameByOffsetMap[compressedKeyFrame.PreviousFrameOffset];
                }

                keyFrameByOffsetMap[i * COMPRESSED_KEYFRAME_SIZE] = keyFrame;

                keyFrame.Dirty = false;
                KeyFrames.Add( keyFrame );
            }
        }

        private static float DecompressFloat( ushort compressed )
        {
            int decompressed = ( compressed & 0x8000 ) << 16; // sign bit
            if ( ( compressed & 0x7fff ) != 0 )
            {
                decompressed |= ( ( compressed & 0x7800 ) << 12 ) + 0x38000000;
                decompressed |= ( compressed & 0x07ff ) << 12;
            }

            float decompressedFloat = Unsafe.As<int, float>( ref decompressed );

            return decompressedFloat;
        }

        private void WriteHeader( BinaryWriter writer )
        {
            writer.Write( Version );
            writer.Write( (int)KeyFrameType );
            writer.Write( KeyFrames.Count );
            writer.Write( Flags );
            writer.Write( Duration );
        }

        private void WriteKeyFrames( BinaryWriter writer )
        {
            switch ( KeyFrameType )
            {
                case RwKeyFrameType.Uncompressed:
                    {
                        WriteUncompressedKeyFrames( writer );
                    }
                    break;

                case RwKeyFrameType.Compressed:
                    {
                        if ( mCachedCompressedKeyframeData != null && KeyFrames.All( x => !x.Dirty ) )
                        {
                            // Sync time in case they were modified
                            for ( int i = 0; i < mCachedCompressedKeyframeData.KeyFrames.Count; i++ )
                            {
                                mCachedCompressedKeyframeData.KeyFrames[i].Time = KeyFrames[i].Time;
                            }

                            WriteCompressedKeyFrames( writer, mCachedCompressedKeyframeData.KeyFrames, mCachedCompressedKeyframeData.CustomData );
                        }
                        else
                        {
                            CompressKeyFrames( out List<RwCompressedKeyFrame> compressedKeyFrames,
                                               out RwCompressedKeyFrameCustomData customData );

                            WriteCompressedKeyFrames( writer, compressedKeyFrames, customData );
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WriteUncompressedKeyFrames( BinaryWriter writer )
        {
            var keyFrameToOffsetMap = new Dictionary<RwKeyFrame, int>();

            for ( var i = 0; i < KeyFrames.Count; i++ )
            {
                var keyFrame = KeyFrames[ i ];

                // add key frame to offset map for mapping the previous keyframe
                keyFrameToOffsetMap[keyFrame] = i * UNCOMPRESSED_KEYFRAME_SIZE;

                writer.Write( keyFrame.Time );
                writer.Write( keyFrame.Rotation.X );
                writer.Write( keyFrame.Rotation.Y );
                writer.Write( keyFrame.Rotation.Z );
                writer.Write( keyFrame.Rotation.W );
                writer.Write( keyFrame.Translation.X );
                writer.Write( keyFrame.Translation.Y );
                writer.Write( keyFrame.Translation.Z );

                int previousKeyFrameOffset = -1;
                if ( keyFrame.Previous != null )
                    previousKeyFrameOffset = keyFrameToOffsetMap[ keyFrame.Previous ];

                writer.Write( previousKeyFrameOffset );
            }
        }

        // Compress keyframes
        private void CompressKeyFrames(out List<RwCompressedKeyFrame> compressedKeyFrames, out RwCompressedKeyFrameCustomData customData)
        {
            var keyFrameToOffsetMap = new Dictionary<RwKeyFrame, int>();

            // Move translations >= 0
            customData = new RwCompressedKeyFrameCustomData
            {
                Offset = new Vector3
                (
                    KeyFrames.Min( x => x.Translation.X ),
                    KeyFrames.Min( x => x.Translation.Y ),
                    KeyFrames.Min( x => x.Translation.Z )
                )
            };

            compressedKeyFrames = new List<RwCompressedKeyFrame>();
            var offsetTranslations = new List<Vector3>();

            for ( var i = 0; i < KeyFrames.Count; i++ )
            {
                var keyFrame = KeyFrames[i];

                // add key frame to offset map for mapping the previous keyframe
                keyFrameToOffsetMap[keyFrame] = i * COMPRESSED_KEYFRAME_SIZE;

                // create the compressed key frame
                var compressedKeyFrame = new RwCompressedKeyFrame
                {
                    // time will remain the same
                    Time = keyFrame.Time,

                    // compress rotation quaternion
                    RotationX = CompressFloat( keyFrame.Rotation.X ),
                    RotationY = CompressFloat( keyFrame.Rotation.Y ),
                    RotationZ = CompressFloat( keyFrame.Rotation.Z ),
                    RotationW = CompressFloat( keyFrame.Rotation.W ),
                };

                // set to previous frame offset using the map
                if ( keyFrame.Previous != null )
                {
                    compressedKeyFrame.PreviousFrameOffset = keyFrameToOffsetMap[keyFrame.Previous];
                }
                else
                {
                    compressedKeyFrame.PreviousFrameOffset = -1;
                }

                // add the newly created compressed key frame to the list
                // it's not yet fully initialized as the translations still
                // have to be processed
                compressedKeyFrames.Add( compressedKeyFrame );

                // offset the translation by the distance overall closest to zero
                var translation = keyFrame.Translation;
                translation.X -= customData.Offset.X;
                translation.Y -= customData.Offset.Y;
                translation.Z -= customData.Offset.Z;

                // add the offset translation to a seperate list
                offsetTranslations.Add( translation );
            }

            // calculate the translation scalar by taking the largest value of each
            // axis
            customData.Scalar = new Vector3(
                offsetTranslations.Max(x => x.X),
                offsetTranslations.Max(x => x.Y),
                offsetTranslations.Max(x => x.Z));

            // compress the translation using the offset translation and calculated scalar
            for ( int i = 0; i < offsetTranslations.Count; i++ )
            {
                compressedKeyFrames[i].TranslationX = CompressFloat( offsetTranslations[i].X / customData.Scalar.X );
                compressedKeyFrames[i].TranslationY = CompressFloat( offsetTranslations[i].Y / customData.Scalar.Y );
                compressedKeyFrames[i].TranslationZ = CompressFloat( offsetTranslations[i].Z / customData.Scalar.Z );
            }
        }

        private static void WriteCompressedKeyFrames(BinaryWriter writer, List<RwCompressedKeyFrame> compressedKeyFrames,
            RwCompressedKeyFrameCustomData customData)
        {
            foreach ( var compressedKeyFrame in compressedKeyFrames )
            {
                writer.Write( compressedKeyFrame.Time );
                writer.Write( compressedKeyFrame.RotationX );
                writer.Write( compressedKeyFrame.RotationY );
                writer.Write( compressedKeyFrame.RotationZ );
                writer.Write( compressedKeyFrame.RotationW );
                writer.Write( compressedKeyFrame.TranslationX );
                writer.Write( compressedKeyFrame.TranslationY );
                writer.Write( compressedKeyFrame.TranslationZ );
                writer.Write( compressedKeyFrame.PreviousFrameOffset );
            }

            writer.Write( customData.Offset );
            writer.Write( customData.Scalar );
        }

        private static ushort CompressFloat(float uncompressed)
        {
            int floatInt = Unsafe.As<float, int>(ref uncompressed);
            ushort compressed = (ushort)(( floatInt & 0x80000000 ) >> 16); // sign bit

            if ( (floatInt & 0x7fffffff) != 0 )
            {
                compressed |= (ushort)(( ( floatInt & 0x7800000 ) ) >> 12);
                compressed |= (ushort)(( floatInt & 0x7FF000 ) >> 12);
            }

            return compressed;
        }
    }

    public class RwKeyFrame
    {
        private Quaternion mRotation;
        private Vector3 mTranslation;
        private RwKeyFrame mPrevious;

        internal bool Dirty { get; set; }

        public float Time { get; set; }

        public Quaternion Rotation
        {
            get { return mRotation; }
            set { mRotation = value; Dirty = true; }
        }

        public Vector3 Translation
        {
            get { return mTranslation; }
            set { mTranslation = value; Dirty = true; }
        }

        public RwKeyFrame Previous
        {
            get { return mPrevious; }
            set { mPrevious = value; Dirty = true; }
        }

        public RwKeyFrame()
        {
        }

        public RwKeyFrame(float time, Quaternion rotation, Vector3 translation, RwKeyFrame previous)
        {
            Time = time;
            Rotation = rotation;
            Translation = translation;
            Previous = previous;
        }
    }

    internal class RwCompressedKeyFrame
    {
        public float Time { get; set; }

        public ushort RotationX { get; set; }

        public ushort RotationY { get; set; }

        public ushort RotationZ { get; set; }

        public ushort RotationW { get; set; }

        public ushort TranslationX { get; set; }

        public ushort TranslationY { get; set; }

        public ushort TranslationZ { get; set; }

        public int PreviousFrameOffset { get; set; }
    }

    internal class RwCompressedKeyFrameCustomData
    {
        public Vector3 Offset { get; set; }

        public Vector3 Scalar { get; set; }
    }

    internal class RwCompressedKeyFrameData
    {
        public List<RwCompressedKeyFrame> KeyFrames { get; set; }

        public RwCompressedKeyFrameCustomData CustomData { get; set; }

        public RwCompressedKeyFrameData( List<RwCompressedKeyFrame> keyFrames, RwCompressedKeyFrameCustomData customData )
        {
            KeyFrames = keyFrames;
            CustomData = customData;
        }
    }

    public enum RwKeyFrameType
    {
        Uncompressed = 1,
        Compressed = 2
    }
}
