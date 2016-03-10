namespace AtlusLibSharp.IO
{
    using System.IO;

    internal interface IWriteable
    {
        void InternalWrite(BinaryWriter writer);
    }
}
