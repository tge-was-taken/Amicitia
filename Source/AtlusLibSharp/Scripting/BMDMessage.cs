namespace AtlusLibSharp.Scripting
{
    using System.Collections.Generic;
    using System.IO;

    public abstract class BMDMessage
    {
        internal const int NAME_LENGTH = 24;

        protected string name;
        protected BMDDialog[] dialogs;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public BMDDialog[] Dialogs
        {
            get { return dialogs; }
            set { dialogs = value; }
        }

        public int DialogCount
        {
            get { return dialogs.Length; }
        }

        public abstract BMDMessageType MessageType { get; }

        internal BMDMessage() { }

        internal abstract void InternalWrite(BinaryWriter writer, ref List<int> addressList, int fp);
    }
}
