namespace AtlusLibSharp.SMT3.ChunkResources
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Base class for chunks.
    /// </summary>
    public abstract class Chunk : BinaryFileBase
    {
        // Internal Constants
        internal const int HEADER_SIZE = 12;

        // Private fields
        private ushort _flag;
        private ushort _userID;
        private int _length;
        private string _tag;

        // Constructors
        protected Chunk(ushort flg, ushort id, int length, string tag)
        {
            _flag = flg;
            _userID = id;
            _length = length;
            _tag = tag;
        }

        public ushort Flags
        {
            get { return _flag; }
            protected set { _flag = value; }
        }

        public ushort UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public int Length
        {
            get { return _length; }
            protected set { _length = value; }
        }

        public string Tag
        {
            get { return _tag; }
            protected set { _tag = value; }
        }
    }
}
