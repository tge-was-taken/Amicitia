namespace AtlusLibSharp.PS2.Graphics.Interfaces.VIF
{
    public class VifPacket
    {
        // Protected fields
        protected ushort _immediate;
        protected byte Count;
        protected byte _command;

        public VifPacket(VifTag vt)
        {
            _immediate = vt.Immediate;
            Count = vt.Count;
            _command = vt.Command;
        }

        // Properties
        public virtual ushort Immediate
        {
            get { return _immediate; }
        }

        public virtual byte DataCount
        {
            get { return Count; }
        }

        public virtual PS2VifCommand Command
        {
            get { return (PS2VifCommand)_command; }
        }
    }
}
