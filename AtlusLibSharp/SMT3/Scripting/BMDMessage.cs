namespace AtlusLibSharp.SMT3.Scripting
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    public abstract class BMDMessage
    {
        internal const int MSG_MESSAGE_NAME_LENGTH = 24;

        protected string _name;
        protected BMDDialog[] _dialogs;
        protected int[] _dialogPointerTable;
        protected int _dialogDataLength;

        public string Name
        {
            get { return _name; }
        }

        public BMDDialog[] Dialogs
        {
            get { return _dialogs; }
        }

        public abstract MessageType MessageType { get; }

        protected internal abstract XElement ConvertToXmlElement(params object[] param);

        protected internal abstract void InternalWrite(BinaryWriter writer, ref List<int> addressList, int fp);
    }
}
