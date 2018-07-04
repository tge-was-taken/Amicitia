using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using AmicitiaLibrary.Utilities;

namespace AmicitiaLibrary.IO
{
    public class EndianBinaryWriter : BinaryWriter
    {
        internal class ScheduledWrite
        {
            public long Position { get; }

            public Func<long> Action { get; }

            public ScheduledWrite( long position, Func<long> action )
            {
                Position = position;
                Action = action;
            }
        }

        private Endianness mEndianness;
        private Encoding mEncoding;
        private LinkedList<ScheduledWrite> mScheduledWrites;
        private LinkedList<ScheduledWrite> mScheduledLateWrites;
        private LinkedList<long> mScheduledFileSizeWrites;
        private List<long> mOffsetPositions;
        private Stack<long> mBaseOffset;

        public Endianness Endianness
        {
            get => mEndianness;
            set
            {
                SwapBytes = value != EndiannessHelper.SystemEndianness;
                mEndianness = value;
            }
        }

        public bool SwapBytes { get; private set; }

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public long Length => BaseStream.Length;

        public IReadOnlyList<long> OffsetPositions => mOffsetPositions;

        public EndianBinaryWriter(Stream input, Endianness endianness)
            : base(input)
        {
            Init(Encoding.Default, endianness);
        }

        public EndianBinaryWriter(Stream input, Encoding encoding, Endianness endianness)
            : base(input, encoding)
        {
            Init(encoding, endianness);
        }

        public EndianBinaryWriter(Stream input, Encoding encoding, bool leaveOpen, Endianness endianness)
            : base(input, encoding, leaveOpen)
        {
            Init(encoding, endianness);
        }

        private void Init(Encoding encoding, Endianness endianness)
        {
            Endianness = endianness;
            mEncoding = encoding;
            mScheduledWrites = new LinkedList<ScheduledWrite>();
            mScheduledLateWrites = new LinkedList<ScheduledWrite>();
            mScheduledFileSizeWrites = new LinkedList<long>();
            mOffsetPositions = new List<long>();
            mBaseOffset = new Stack<long>();
            mBaseOffset.Push( 0 );
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            BaseStream.Seek(offset, origin);
        }

        public void SeekBegin(long offset)
        {
            BaseStream.Seek(offset, SeekOrigin.Begin);
        }

        public void SeekCurrent(long offset)
        {
            BaseStream.Seek(offset, SeekOrigin.Current);
        }

        public void SeekEnd(long offset)
        {
            BaseStream.Seek(offset, SeekOrigin.End);
        }

        public void PushBaseOffset( long baseOffset ) => mBaseOffset.Push( baseOffset );

        public void PopBaseOffset() => mBaseOffset.Pop();

        public new void Write(byte[] values)
        {
            base.Write(values);
        }

        public void Write( IEnumerable<sbyte> values)
        {
            foreach ( sbyte t in values )
                Write(t);
        }

        public void Write( IEnumerable<bool> values)
        {
            foreach ( bool t in values )
                Write(t);
        }

        public override void Write(short value)
        {
            base.Write(SwapBytes ? EndiannessHelper.Swap(value) : value);
        }

        public void Write( IEnumerable<short> values)
        {
            foreach (var value in values)
                Write(value);
        }

        public override void Write(ushort value)
        {
            base.Write(SwapBytes ? EndiannessHelper.Swap(value) : value);
        }

        public void Write( IEnumerable<ushort> values)
        {
            foreach (var value in values)
                Write(value);
        }

        public override void Write(int value)
        {
            base.Write(SwapBytes ? EndiannessHelper.Swap(value) : value);
        }

        public void Write(IEnumerable<int> values)
        {
            foreach (var value in values)
                Write(value);
        }

        public override void Write(uint value)
        {
            base.Write(SwapBytes ? EndiannessHelper.Swap(value) : value);
        }

        public void Write( IEnumerable<uint> values)
        {
            foreach (var value in values)
                Write(value);
        }

        public override void Write(long value)
        {
            base.Write(SwapBytes ? EndiannessHelper.Swap(value) : value);
        }

        public void Write( IEnumerable<long> values)
        {
            foreach (var value in values)
                Write(value);
        }

        public override void Write(ulong value)
        {
            base.Write(SwapBytes ? EndiannessHelper.Swap(value) : value);
        }

        public void Write( IEnumerable<ulong> values)
        {
            foreach (var value in values)
                Write(value);
        }

        public override void Write(float value)
        {
            base.Write(SwapBytes ? EndiannessHelper.Swap(value) : value);
        }

        public void Write( IEnumerable<float> values)
        {
            foreach (var value in values)
                Write(value);
        }

        public override void Write(decimal value)
        {
            base.Write(SwapBytes ? EndiannessHelper.Swap(value) : value);
        }

        public void Write( IEnumerable<decimal> values)
        {
            foreach (var value in values)
                Write(value);
        }

        public void Write( Vector2 value )
        {
            Write( value.X );
            Write( value.Y );
        }

        public void Write( IEnumerable<Vector2> values )
        {
            foreach ( var item in values )
                Write( item );
        }

        public void Write( Vector3 value )
        {
            Write( value.X );
            Write( value.Y );
            Write( value.Z );
        }

        public void Write( IEnumerable<Vector3> values )
        {
            foreach ( var item in values )
                Write( item );
        }

        public void Write(string value, StringBinaryFormat format, int fixedLength = -1)
        {
            switch (format)
            {
                case StringBinaryFormat.NullTerminated:
                    {
                        Write(mEncoding.GetBytes(value));

                        for (int i = 0; i < mEncoding.GetMaxByteCount(1); i++)
                            Write((byte)0);
                    }
                    break;
                case StringBinaryFormat.FixedLength:
                    {
                        if (fixedLength == -1)
                        {
                            throw new ArgumentException("Fixed length must be provided if format is set to fixed length", nameof(fixedLength));
                        }

                        var bytes = mEncoding.GetBytes(value);
                        if (bytes.Length > fixedLength)
                            Array.Resize( ref bytes, fixedLength );

                        Write(bytes);
                        fixedLength -= bytes.Length;

                        while (fixedLength-- > 0)
                            Write((byte)0);
                    }
                    break;

                case StringBinaryFormat.PrefixedLength8:
                    {
                        Write((byte)value.Length);
                        Write(mEncoding.GetBytes(value));
                    }
                    break;

                case StringBinaryFormat.PrefixedLength16:
                    {
                        Write((ushort)value.Length);
                        Write(mEncoding.GetBytes(value));
                    }
                    break;

                case StringBinaryFormat.PrefixedLength32:
                    {
                        Write((uint)value.Length);
                        Write(mEncoding.GetBytes(value));
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid format specified", nameof(format));
            }
        }

        public void WritePadding( int count )
        {
            for ( int i = 0; i < count / 8; i++ )
                Write( 0L );

            for ( int i = 0; i < count % 8; i++ )
                Write( ( byte ) 0 );
        }

        public void WriteAlignmentPadding( int alignment )
        {
            WritePadding( AlignmentHelper.GetAlignedDifference( Position, alignment ) );
        }

        public void ScheduleOffsetWrite( Action action, bool late = false )
        {
            ScheduleOffsetWrite( () =>
            {
                long offset = BaseStream.Position;
                action();
                return offset;
            } );
        }

        public void ScheduleOffsetWrite( int alignment, Action action, bool late = false )
        {
            ScheduleOffsetWrite( () =>
            {
                WriteAlignmentPadding( alignment );
                long offset = BaseStream.Position;
                action();
                return offset;
            } );
        }

        public void ScheduleFileSizeWrite()
        {
            mScheduledFileSizeWrites.AddLast( Position );
            Write( 0 );
        }

        private void ScheduleOffsetWrite( Func<long> action, bool late = false )
        {
            if (!late )
                mScheduledWrites.AddLast( new ScheduledWrite( BaseStream.Position, action ) );
            else
                mScheduledLateWrites.AddLast( new ScheduledWrite( BaseStream.Position, action ) );

            Write( 0 );
        }

        public void PerformScheduledWrites()
        {
            DoScheduledOffsetWrites();
            DoScheduledLateOffsetWrites();
            DoScheduledFileSizeWrites();
        }

        private void DoScheduledOffsetWrites()
        {
            var current = mScheduledWrites.First;
            while ( current != null )
            {
                DoScheduledWrite( current.Value );
                current = current.Next;
            }
        }

        private void DoScheduledLateOffsetWrites()
        {
            var current = mScheduledLateWrites.First;
            while ( current != null )
            {
                DoScheduledWrite( current.Value );
                current = current.Next;
            }
        }

        private void DoScheduledFileSizeWrites()
        {
            var current = mScheduledFileSizeWrites.First;
            while ( current != null )
            {
                SeekBegin( current.Value );
                Write( ( int )Length );
                current = current.Next;
            }
        }

        private void DoScheduledWrite( ScheduledWrite scheduledWrite )
        {
            long offsetPosition = scheduledWrite.Position;
            mOffsetPositions.Add( offsetPosition - mBaseOffset.Peek() );

            // Do actual write
            long offset = scheduledWrite.Action() - mBaseOffset.Peek();

            // Write offset
            long returnPos = BaseStream.Position;
            BaseStream.Seek( offsetPosition, SeekOrigin.Begin );
            Write( ( int )offset );

            // Seek back for next one
            BaseStream.Seek( returnPos, SeekOrigin.Begin );
        }
    }
}
