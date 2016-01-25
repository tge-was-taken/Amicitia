namespace AtlusLibSharp.SMT3.ChunkResources.Scripting
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    public abstract class MSGMessage
    {
        internal const int MSG_MESSAGE_NAME_LENGTH = 24;

        protected string _name;
        protected MSGDialog[] _dialogs;
        protected int[] _dialogPointerTable;
        protected int _dialogDataLength;

        public string Name
        {
            get { return _name; }
        }

        public MSGDialog[] Dialogs
        {
            get { return _dialogs; }
        }

        protected internal abstract XElement ConvertToXmlElement(params object[] param);

        protected internal abstract void Write(BinaryWriter writer, ref List<int> addressList, int fp);
    }
}
