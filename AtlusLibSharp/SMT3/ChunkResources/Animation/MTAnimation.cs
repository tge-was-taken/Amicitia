namespace AtlusLibSharp.SMT3.ChunkResources.Animation
{
    using System.IO;

    public class MTAnimation
    {
        // Private fields
        private int _numFrames;
        private MTKey[] _keys;

        // Constructors
        internal MTAnimation(ushort numKeys, BinaryReader reader)
        {
            Read(numKeys, reader);
        }

        // Properties
        public int FrameCount
        {
            get { return _numFrames; }
        }

        public MTKey[] Keys
        {
            get { return _keys; }
        }

        // Methods
        internal void Write(BinaryWriter writer)
        {
            writer.Write(_numFrames);
            foreach (MTKey key in _keys)
            {
                key.Write(writer);
            }
        }

        internal int GetSize()
        {
            int size = sizeof(int);
            foreach (MTKey key in _keys)
            {
                size += key.GetSize();
            }
            return size;
        }

        private void Read(ushort numKeys, BinaryReader reader)
        {
            _numFrames = reader.ReadInt32();
            _keys = new MTKey[numKeys];
            for (int i = 0; i < numKeys; i++)
            {
                _keys[i] = new MTKey(reader);
            }
        }
    }
}
