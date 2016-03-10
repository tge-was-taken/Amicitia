namespace AtlusLibSharp.Tables.Persona4
{
    internal static class P4TableConstants
    {
        // message table constants
        public const int ARCANA_NAME_LENGTH = 21;
        public const int SKILL_NAME_LENGTH = 19;
        public const int UNIT_NAME_LENGTH = 21;
        public const int PERSONA_NAME_LENGTH = 17;

        // model table constants
        public const int MODEL_TABLE_SIZE = 0x196F0;
        public const int MODEL_CAMERA_PROPERTY_NUM = 5;

        // party constants
        public const int PARTY_NUM_SLOT = 11;
        public const int PARTY_NUM_ANIM = 27;
        public const int PARTY_NUM_CRIT_ASSIST = 8;
        public static string[] PARTY_SLOT_NAMES = new string[PARTY_NUM_SLOT] // as these aren't stored in the MSG.TBL
        {
            "000",
            "Protagonist",
            "Yosuke",
            "Chie",
            "Yukiko",
            "Rise",
            "Kanji",
            "Naoto",
            "Teddie",
            "009",
            "00A"
        };

        // unit constants
        public const int UNIT_NUM_SLOT = 336;
        public const int UNIT_NUM_ANIM = 18;

        // persona constants
        public const int PERSONA_NUM_SLOT = 256;
        public const int PERSONA_NUM_ANIM = 7;
        public const int ARCANA_NUM_SLOT = 32;

        // skill contants
        public const int SKILL_NUM_SLOT = 440;

        // encounter constants
        public const int ENCOUNTER_NUM_SLOT = 912;
    }
}
