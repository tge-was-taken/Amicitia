namespace AtlusLibSharp.Graphics.MT
{
    using System.IO;

    public class AbAnimation
    {
        // Private fields
        private int mNumFrames;
        private AbKey[] mKeys;

        // Constructors
        internal AbAnimation(short numKeys, BinaryReader reader)
        {
            InternalRead(numKeys, reader);
        }

        // Properties
        public int FrameCount
        {
            get { return mNumFrames; }
        }

        public AbKey[] Keys
        {
            get { return mKeys; }
        }

        // Methods
        internal void Write(BinaryWriter writer)
        {
            writer.Write(mNumFrames);
            foreach (AbKey key in mKeys)
            {
                key.InternalWrite(writer);
            }
        }

        internal int GetSize()
        {
            int size = sizeof(int);
            foreach (AbKey key in mKeys)
            {
                size += key.GetSize();
            }
            return size;
        }

        private void InternalRead(short numKeys, BinaryReader reader)
        {
            mNumFrames = reader.ReadInt32();
            mKeys = new AbKey[numKeys];
            for (int i = 0; i < numKeys; i++)
            {
                mKeys[i] = new AbKey(reader);
            }
        }
    }
}
