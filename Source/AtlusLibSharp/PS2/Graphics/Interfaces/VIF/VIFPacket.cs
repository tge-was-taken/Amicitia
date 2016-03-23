namespace AtlusLibSharp.PS2.Graphics.Interfaces.VIF
{
    public class VIFPacket
    {
        // Protected fields
        protected ushort _immediate;
        protected byte _count;
        protected byte _command;

        public VIFPacket(VIFTag vt)
        {
            _immediate = vt.Immediate;
            _count = vt.Count;
            _command = vt.Command;
        }

        // Properties
        public virtual ushort Immediate
        {
            get { return _immediate; }
        }

        public virtual byte DataCount
        {
            get { return _count; }
        }

        public virtual PS2VIFCommand Command
        {
            get { return (PS2VIFCommand)_command; }
        }
    }
}
