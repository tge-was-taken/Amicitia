namespace AtlusLibSharp.Graphics.MT
{
    using System.IO;

    public class ABAnimation
    {
        // Private fields
        private int _numFrames;
        private ABKey[] _keys;

        // Constructors
        internal ABAnimation(short numKeys, BinaryReader reader)
        {
            InternalRead(numKeys, reader);
        }

        // Properties
        public int FrameCount
        {
            get { return _numFrames; }
        }

        public ABKey[] Keys
        {
            get { return _keys; }
        }

        // Methods
        internal void Write(BinaryWriter writer)
        {
            writer.Write(_numFrames);
            foreach (ABKey key in _keys)
            {
                key.InternalWrite(writer);
            }
        }

        internal int GetSize()
        {
            int size = sizeof(int);
            foreach (ABKey key in _keys)
            {
                size += key.GetSize();
            }
            return size;
        }

        private void InternalRead(short numKeys, BinaryReader reader)
        {
            _numFrames = reader.ReadInt32();
            _keys = new ABKey[numKeys];
            for (int i = 0; i < numKeys; i++)
            {
                _keys[i] = new ABKey(reader);
            }
        }
    }
}
