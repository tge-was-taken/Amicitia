namespace AtlusLibSharp.Tables.Persona4
{
    using System.IO;
    using IO;

    // TODO
    internal class P4ModelTablePartyModelProperties : IWriteable
    {
        private ushort[] _cameraProperties1 = new ushort[P4TableConstants.MODEL_CAMERA_PROPERTY_NUM];
        private ushort[] _cameraProperties2 = new ushort[P4TableConstants.MODEL_CAMERA_PROPERTY_NUM];
        private ushort _modelScale;
        private ushort _unknown1;
        private ushort _numCriticalHits;
        private ushort[] _criticalHitTimings = new ushort[P4TableConstants.PARTY_NUM_CRIT_ASSIST];
        private ushort _numAssistHits;
        private ushort[] _assistHitTimings = new ushort[P4TableConstants.PARTY_NUM_CRIT_ASSIST];
        private P4ModelTablePartyModelProperties[] _animationProperties = new P4ModelTablePartyModelProperties[P4TableConstants.PARTY_NUM_ANIM];
        private ushort _attackRange;

        internal P4ModelTablePartyModelProperties(BinaryReader reader)
        {
            // camera properties
            for (int i = 0; i < P4TableConstants.MODEL_CAMERA_PROPERTY_NUM; i++)
                _cameraProperties1[i] = reader.ReadUInt16();

            for (int i = 0; i < P4TableConstants.MODEL_CAMERA_PROPERTY_NUM; i++)
                _cameraProperties2[i] = reader.ReadUInt16();

            // scale & idk
            _modelScale = reader.ReadUInt16();
            _unknown1 = reader.ReadUInt16();

            // critical hits
            _numCriticalHits = reader.ReadUInt16();
            for (int i = 0; i < P4TableConstants.PARTY_NUM_CRIT_ASSIST; i++)
                _criticalHitTimings[i] = reader.ReadUInt16();

            // assist hits
            _numAssistHits = reader.ReadUInt16();
            for (int i = 0; i < P4TableConstants.PARTY_NUM_CRIT_ASSIST; i++)
                _assistHitTimings[i] = reader.ReadUInt16();

            // animation props
            for (int i = 0; i < P4TableConstants.PARTY_NUM_ANIM; i++)
                _animationProperties[i] = new P4ModelTablePartyModelProperties(reader);

            // range
            _attackRange = reader.ReadUInt16();
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            // camera properties
            for (int i = 0; i < _cameraProperties1.Length; i++)
                writer.Write(_cameraProperties1[i]);

            for (int i = 0; i < _cameraProperties2.Length; i++)
                writer.Write(_cameraProperties2[i]);

            // scale & unknown
            writer.Write(_modelScale);
            writer.Write(_unknown1);

            // critical timings
            writer.Write(_numCriticalHits);
            for (int i = 0; i < _criticalHitTimings.Length; i++)
                writer.Write(_criticalHitTimings[i]);

            // assist timings
            writer.Write(_numAssistHits);
            for (int i = 0; i < _assistHitTimings.Length; i++)
                writer.Write(_assistHitTimings[i]);

            // animation props
            for (int i = 0; i < P4TableConstants.PARTY_NUM_ANIM; i++)
                _animationProperties[i].InternalWrite(writer);

            // range
            writer.Write(_attackRange);
        }

        void IWriteable.InternalWrite(BinaryWriter writer)
        {
            InternalWrite(writer);
        }
    }
}
