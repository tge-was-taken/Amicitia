namespace AtlusLibSharp.Tables.Persona4
{
    using System.IO;

    // TODO
    internal class P4ModelTable : ModelTableBase
    {
        private P4ModelTablePartyModelProperties[] _partyModelProperties = new P4ModelTablePartyModelProperties[P4TableConstants.PARTY_NUM_SLOT];

        internal override void InternalWrite(BinaryWriter writer)
        {
            WriteSection(writer, _partyModelProperties);
        }
    }
}
