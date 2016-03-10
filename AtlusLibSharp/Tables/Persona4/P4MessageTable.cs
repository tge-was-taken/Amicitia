namespace AtlusLibSharp.Tables.Persona4
{
    using Scripting;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Class representing the message data table in Persona 4.
    /// </summary>
    public class P4MessageTable : MessageTableBase
    {
        private string[] _arcanaNames   = new string[P4TableConstants.ARCANA_NUM_SLOT];
        private string[] _skillNames    = new string[P4TableConstants.SKILL_NUM_SLOT];
        private string[] _unitNames     = new string[P4TableConstants.UNIT_NUM_SLOT];
        private string[] _personaNames  = new string[P4TableConstants.PERSONA_NUM_SLOT];

        /********************/
        /**** Properties ****/
        /********************/

        /// <summary>
        /// Gets the array of arcana names.
        /// </summary>
        public string[] ArcanaNames
        {
            get { return _arcanaNames; }
        }

        /// <summary>
        /// Gets the array of skill names.
        /// </summary>
        public string[] SkillNames
        {
            get { return _skillNames; }
        }

        /// <summary>
        /// Gets the array of encountered enemy names.
        /// </summary>
        public string[] EncounterNames
        {
            get { return _unitNames; }
        }

        /// <summary>
        /// Gets the array of persona names.
        /// </summary>
        public string[] PersonaNames
        {
            get { return _personaNames; }
        }

        /// <summary>
        /// Gets the message file containing the dialogue for skills used in battle.
        /// </summary>
        public BMDFile SkillDialogue { get; set; }

        /****************/
        /* Constructors */
        /****************/

        /// <summary>
        /// Loads a message table file from the given path.
        /// </summary>
        /// <param name="path">File path to the message table.</param>
        public P4MessageTable(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                InternalRead(reader);
            }
        }

        /// <summary>
        /// Loads a message table file from the stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <param name="leaveStreamOpen">Option to leave the stream open after use.</param>
        public P4MessageTable(Stream stream, bool leaveStreamOpen)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default, leaveStreamOpen))
                InternalRead(reader);
        }

        internal P4MessageTable(BinaryReader reader)
        {
            InternalRead(reader);
        }

        /*****************/
        /**** Methods ****/
        /*****************/

        /// <summary>
        /// Save the contents of the message table to a text file.
        /// </summary>
        /// <param name="path">Path to save the text file to.</param>
        public override void SaveText(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                WriteStringSectionToText(writer, _arcanaNames,  "Arcana",   P4TableConstants.ARCANA_NAME_LENGTH);
                WriteStringSectionToText(writer, _skillNames,   "Skill",    P4TableConstants.SKILL_NAME_LENGTH);
                WriteStringSectionToText(writer, _unitNames,    "Unit",     P4TableConstants.UNIT_NAME_LENGTH);
                WriteStringSectionToText(writer, _personaNames, "Persona",  P4TableConstants.PERSONA_NAME_LENGTH);
            }
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            // write string sections
            WriteStringSection(writer, _arcanaNames,    P4TableConstants.ARCANA_NAME_LENGTH);
            WriteStringSection(writer, _skillNames,     P4TableConstants.SKILL_NAME_LENGTH);
            WriteStringSection(writer, _unitNames,      P4TableConstants.UNIT_NAME_LENGTH);
            WriteStringSection(writer, _personaNames,   P4TableConstants.PERSONA_NAME_LENGTH);

            using (MemoryStream skillDialogueStream = new MemoryStream())
            {
                // write the skill dialogue bmd file to the temporary stream
                SkillDialogue.Save(skillDialogueStream);

                // write the section to the file using the temporary stream
                WriteSection(writer, skillDialogueStream);
            }
        }

        private void InternalRead(BinaryReader reader)
        {
            ReadStringsFromSection(reader, ref _arcanaNames,    P4TableConstants.ARCANA_NAME_LENGTH);
            ReadStringsFromSection(reader, ref _skillNames,     P4TableConstants.SKILL_NAME_LENGTH);
            ReadStringsFromSection(reader, ref _unitNames,      P4TableConstants.UNIT_NAME_LENGTH);
            ReadStringsFromSection(reader, ref _personaNames,   P4TableConstants.PERSONA_NAME_LENGTH);
            SkillDialogue = new BMDFile(ReadSection(reader), false);
        }
    }
}
