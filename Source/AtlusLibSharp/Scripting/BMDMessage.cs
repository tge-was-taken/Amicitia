namespace AtlusLibSharp.Scripting
{
    using System.Collections.Generic;
    using System.IO;

    public abstract class BmdMessage
    {
        internal const int NameLength = 24;

        protected string name;
        protected BmdDialog[] dialogs;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public BmdDialog[] Dialogs
        {
            get { return dialogs; }
            set { dialogs = value; }
        }

        public int DialogCount
        {
            get { return dialogs.Length; }
        }

        public abstract BmdMessageType MessageType { get; }

        internal BmdMessage() { }

        internal abstract void InternalWrite(BinaryWriter writer, ref List<int> addressList, int fp);
    }
}
