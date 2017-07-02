using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using AtlusLibSharp.Utilities;

namespace AtlusLibSharp.Graphics.RenderWare
{
    public class RwAnimationNode : RwNode
    {
        public const int VERSION = 0x100;

        public int Version { get; set; }

        public RwKeyFrameType KeyFrameType { get; set; }

        public List<RwKeyFrame> KeyFrames { get; private set; }

        public int Flags { get; set; }

        public float Duration { get; set; }

        public RwAnimationNode(RwNode parent, RwKeyFrameType keyFrameType, float duration) : 
            base(RwNodeId.RwAnimationNode, parent)
        {
            Version = VERSION;
            KeyFrameType = keyFrameType;
            KeyFrames = new List<RwKeyFrame>();
            Flags = 0;
            Duration = duration;
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
                    throw new NotImplementedException();
                    break;

                case RwKeyFrameType.Compressed:
                    ReadCompressedKeyFrames( reader );
                    break;
            }
        }

        private void ReadCompressedKeyFrames( BinaryReader reader )
        {
            var keyFrameByOffsetMap = new Dictionary<int, RwKeyFrame>();
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

                keyFrameByOffsetMap[i * 24] = keyFrame;

                KeyFrames.Add( keyFrame );
            }
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
                    throw new NotImplementedException();

                case RwKeyFrameType.Compressed:
                    {
                        CompressKeyFrames( out List<RwCompressedKeyFrame> compressedKeyFrames,
                            out RwCompressedKeyFrameCustomData customData );

                        WriteCompressedKeyFrames( writer, compressedKeyFrames, customData );
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CompressKeyFrames(out List<RwCompressedKeyFrame> compressedKeyFrames, out RwCompressedKeyFrameCustomData customData)
        {
            var keyFrameToOffsetMap = new Dictionary<RwKeyFrame, int>();

            // Calculate offset from zero
            customData = new RwCompressedKeyFrameCustomData
            {
                Offset = new Vector3
                (
                    KeyFrames.Max( x => x.Translation.X ),
                    KeyFrames.Max( x => x.Translation.Y ),
                    KeyFrames.Max( x => x.Translation.Z )
                )
            };

            compressedKeyFrames = new List<RwCompressedKeyFrame>();
            var offsetTranslations = new List<Vector3>();

            for ( var i = 0; i < KeyFrames.Count; i++ )
            {
                var keyFrame = KeyFrames[i];

                // add key frame to offset map for mapping the previous keyframe
                keyFrameToOffsetMap[keyFrame] = i * 24;

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
                CalculateScalar(offsetTranslations.Min(x => x.X), offsetTranslations.Max(x => x.X)),
                CalculateScalar(offsetTranslations.Min(x => x.Y), offsetTranslations.Max(x => x.Y)),
                CalculateScalar(offsetTranslations.Min(x => x.Z), offsetTranslations.Max(x => x.Z)));

            // compress the translation using the offset translation and calculated scalar
            for ( int i = 0; i < offsetTranslations.Count; i++ )
            {
                compressedKeyFrames[i].TranslationX = CompressFloat( offsetTranslations[i].X / customData.Scalar.X );
                compressedKeyFrames[i].TranslationY = CompressFloat( offsetTranslations[i].Y / customData.Scalar.Y );
                compressedKeyFrames[i].TranslationZ = CompressFloat( offsetTranslations[i].Z / customData.Scalar.Z );
            }
        }

        private static float CalculateScalar( float min, float max )
        {
            float scalarX;
            if ( max > ( min * -1 ) )
            {
                scalarX = max;
            }
            else
            {
                scalarX = min * -1;
            }

            return scalarX;
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

        private static float DecompressFloat(ushort compressed)
        {
            int decompressed = ( compressed & 0x8000 ) << 16; // sign bit
            if ( (compressed & 0x7fff) != 0 )
            {
                decompressed |= ( ( compressed & 0x7800 ) << 12 ) + 0x38000000;
                decompressed |= ( compressed & 0x07ff ) << 12;
            }

            float decompressedFloat = Unsafe.As<int, float>(ref decompressed);

            return decompressedFloat;
        }

        private static ushort CompressFloat(float uncompressed)
        {
            int floatInt = Unsafe.As<float, int>(ref uncompressed);
            ushort compressed = (ushort)(( floatInt & 0x80000000 ) >> 16); // sign bit

            if ( (floatInt & 0x7fffffff) != 0 )
            {
                compressed |= (ushort)(( ( floatInt & 0x7800000 ) - 0x38000000 ) >> 12);
                compressed |= (ushort)(( floatInt & 0x7FF000 ) >> 12);
            }

            return compressed;
        }
    }

    public class RwKeyFrame
    {
        public float Time { get; set; }

        public Quaternion Rotation { get; set; }

        public Vector3 Translation { get; set; }

        public RwKeyFrame Previous { get; set; }

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

    public enum RwKeyFrameType
    {
        Uncompressed = 1,
        Compressed = 2
    }
}
