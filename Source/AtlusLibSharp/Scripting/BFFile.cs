namespace AtlusLibSharp.Scripting
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using AtlusLibSharp.Utilities;
    using IO;

    public enum TypeTableType
    {
        Procedures = 0,
        JumpLabels = 1,
        Opcodes = 2,
        Messages = 3,
        Strings = 4,
    }

    public class BfFile : BinaryBase
    {
        private const byte HeaderSize = 0x10;
        private const int DataStart = 0x70;
        private const int TypeTableCount = 5;
        private const short Type = 0;
        private const string Tag = "FLW0";

        private BfCodeLabel[] mProcedures;
        private BfCodeLabel[] mJumpLabels;
        private bool mRequireSortProcedures;
        private bool mRequireSortJumps;
        private List<BfOpcode> mOpcodes;
        private BmdFile mMessageFile;

        #region Properties

        public BfCodeLabel[] Procedures
        {
            get { return mProcedures; }
        }

        public BfCodeLabel[] Jumps
        {
            get { return mJumpLabels; }
        }

        public List<BfOpcode> Opcodes
        {
            get { return mOpcodes; }
        }

        public BmdFile MessageFile
        {
            get { return mMessageFile; }
        }

        #endregion

        public BfFile(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                InternalRead(reader);
        }

        internal BfFile(BinaryReader reader)
        {
            InternalRead(reader);
        }

        internal BfFile(BfCodeLabel[] procedures, BfCodeLabel[] jumpLabels, List<BfOpcode> opcodes, BmdFile messageFile = null)
        {
            mProcedures = procedures;
            mJumpLabels = jumpLabels;
            mOpcodes = opcodes;
            mMessageFile = messageFile;
        }

        /*
        public BFFile(string xmlPath)
        {
            XDocument xDoc = XDocument.Load(xmlPath);
            XElement xRoot = xDoc.Root;

            if (xRoot.Name != "FLW0")
            {
                throw new InvalidDataException($"Root element name is \"{xRoot.Name}\". Expected \"FLW0\"");
            }

            XElement[] arrayDescriptorElements = xRoot.Elements().ToArray();

            _numTypeTableEntries = arrayDescriptorElements.Length;
            _typeTable = new TypeTableEntry[_numTypeTableEntries];

            for (int i = 0; i < _numTypeTableEntries; i++)
            {
                _typeTable[i] = new TypeTableEntry(arrayDescriptorElements[i]);
            }
        }
        */

        public void ExportDisassembly(string path)
        {
            BfDisassembler.Disassemble(path, mOpcodes, mProcedures, mJumpLabels);
        }

        public static BfFile AssembleFromBfasm(string path, BmdFile messageDialog = null)
        {
            BfFile bf = BfAssembler.AssembleFromAsmText(path);
            bf.mMessageFile = messageDialog;
            return bf;
        }

        public static BfFile LoadFromStream(Stream stream, bool leaveStreamOpen)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                return new BfFile(reader);
        }

        private static TypeTableEntry CreateTypeTableEntry(TypeTableType entryType, int entriesTotalSize, int dataOffset = 0)
        {
            int elementLength = 0;
            switch (entryType)
            {
                case TypeTableType.Procedures:
                case TypeTableType.JumpLabels:
                    elementLength = 32;
                    break;
                case TypeTableType.Opcodes:
                    elementLength = 4;
                    break;
                case TypeTableType.Messages:
                case TypeTableType.Strings:
                    elementLength = 1;
                    break;
            }

            TypeTableEntry entry = new TypeTableEntry()
            {
                Type = (int)entryType,
                DataOffset = dataOffset,
                ElementCount = entriesTotalSize / elementLength,
                ElementLength = elementLength,
            };

            return entry;
        }
       
        internal override void Write(BinaryWriter writer)
        {
            int posFileStart = (int)writer.BaseStream.Position;

            // seek past the header, id table and code labels
            writer.BaseStream.Seek(DataStart + (mProcedures.Length * BfCodeLabel.Size) + (mJumpLabels.Length * BfCodeLabel.Size), SeekOrigin.Current);

            // create code label id table entries
            int procsLength = mProcedures.Length * BfCodeLabel.Size;
            int jumpsLength = mJumpLabels.Length * BfCodeLabel.Size;

            int opCodeDataStart = (int)(writer.BaseStream.Position - posFileStart);
            int numProcsSet = 0;
            int numJumpsSet = 0;
            for (int i = 0; i < mOpcodes.Count; i++)
            {
                int procIdx = -1;

                // try to find a procedure with this index
                if (numProcsSet != mProcedures.Length)
                {
                    procIdx = Array.FindIndex(mProcedures, p => p.OpcodeIndex == i);
                    if (procIdx != -1)
                    {
                        mProcedures[procIdx].Offset = (uint)(((writer.BaseStream.Position - posFileStart) - opCodeDataStart) / 4);
                        numProcsSet++;
                    }
                }

                // if we haven't found a procedure, try to find a jump label with this index
                if (procIdx == -1 && numJumpsSet != mJumpLabels.Length)
                {
                    int jumpIdx = Array.FindIndex(mJumpLabels, j => j.OpcodeIndex == i);
                    if (jumpIdx != -1)
                    {
                        mJumpLabels[jumpIdx].Offset = (uint)(((writer.BaseStream.Position - posFileStart) - opCodeDataStart) / 4);
                        numJumpsSet++;
                    }
                }
                
                // write the opcode data
                writer.Write((ushort)mOpcodes[i].Instruction);

                if (mOpcodes[i].Operand != null)
                {
                    switch (mOpcodes[i].Operand.Type)
                    {
                        case BfOperandType.Immediate:
                            if (mOpcodes[i].Instruction == BfInstruction.PushUInt32)
                            {
                                writer.Write((ushort)0);
                                writer.Write((uint)mOpcodes[i].Operand.ImmediateValue);
                            }
                            else
                            {
                                writer.Write((ushort)mOpcodes[i].Operand.ImmediateValue);
                            }
                            break;
                        case BfOperandType.FloatingPoint:
                            writer.Write((ushort)0);
                            writer.Write((float)mOpcodes[i].Operand.FloatValue);
                            break;
                    }
                }
                else
                {
                    writer.Write((ushort)0);
                }
            }

            int opCodeDataEnd = (int)(writer.BaseStream.Position - posFileStart);

            // set id table entries
            TypeTableEntry[] typeTableEntries = new TypeTableEntry[TypeTableCount];
            typeTableEntries[(int)TypeTableType.Procedures] = CreateTypeTableEntry(TypeTableType.Procedures, procsLength, DataStart);
            typeTableEntries[(int)TypeTableType.JumpLabels] = CreateTypeTableEntry(TypeTableType.JumpLabels, jumpsLength, DataStart + procsLength);
            typeTableEntries[(int)TypeTableType.Opcodes]    = CreateTypeTableEntry(TypeTableType.Opcodes, opCodeDataEnd - opCodeDataStart, opCodeDataStart);
            typeTableEntries[(int)TypeTableType.Messages]   = CreateTypeTableEntry(TypeTableType.Messages, 0, opCodeDataEnd);
            typeTableEntries[(int)TypeTableType.Strings]    = CreateTypeTableEntry(TypeTableType.Strings, 0xF0, opCodeDataEnd);

            if (mMessageFile != null)
            {
                mMessageFile.Write(writer);
                int messageDataEnd = (int)writer.BaseStream.Position - posFileStart;
                int messageDataSize = messageDataEnd - opCodeDataEnd;

                typeTableEntries[(int)TypeTableType.Strings].DataOffset += messageDataSize;
                typeTableEntries[(int)TypeTableType.Messages] = CreateTypeTableEntry(TypeTableType.Messages, messageDataSize, opCodeDataEnd);
            }

            // TODO: add code for the 'string' table here? haven't really seen it being used though.
            // fixed size zero bytes for the strings table when unused
            writer.Write(0, 0xF0);

            long posFileEnd = writer.BaseStream.Position;
            int length = (int)posFileEnd - posFileStart;

            writer.BaseStream.Seek(posFileStart, SeekOrigin.Begin);

            // write standard header
            writer.Write(Type);
            writer.Write((short)0); // userID
            writer.Write(length);
            writer.WriteCString(Tag, 4);
            writer.Write(0); // unused

            // write bf header
            writer.Write(typeTableEntries.Length);
            writer.Write(0); // some unknown value here, not sure what its for
            writer.AlignPosition(16);

            // write id table entries
            foreach (TypeTableEntry entry in typeTableEntries)
            {
                entry.InternalWrite(writer);
            }

            // lastly, write the code labels
            foreach (BfCodeLabel label in mProcedures)
            {
                label.InternalWrite(writer);
            }

            foreach (BfCodeLabel label in mJumpLabels)
            {
                label.InternalWrite(writer);
            }
        }

        private void InternalRead(BinaryReader reader)
        {
            long posFileStart = reader.GetPosition();
            short flag = reader.ReadInt16();
            short userId = reader.ReadInt16();
            int length = reader.ReadInt32();
            string tag = reader.ReadCString(4);
            int unused = reader.ReadInt32();

            if (tag != Tag)
            {
                throw new InvalidDataException("Identifier mismatch.");
            }

            int numTypeTableEntries = reader.ReadInt32();
            int numUnknown = reader.ReadInt32();

            reader.AlignPosition(16);

            TypeTableEntry[] typeTable = new TypeTableEntry[numTypeTableEntries];
            for (int i = 0; i < numTypeTableEntries; i++)
            {
                typeTable[i] = new TypeTableEntry(reader);
            }

            System.Diagnostics.Debug.Assert(typeTable[(int)TypeTableType.Strings].ElementCount == 0xF0);

            for (int i = 0; i < numTypeTableEntries; i++)
            {
                reader.Seek(posFileStart + typeTable[i].DataOffset, SeekOrigin.Begin);

                switch ((TypeTableType)typeTable[i].Type)
                {
                    case TypeTableType.Procedures:
                        ReadCodeLabels(reader, ref mProcedures, typeTable[i].ElementCount, out mRequireSortProcedures);
                        break;
                    case TypeTableType.JumpLabels:
                        ReadCodeLabels(reader, ref mJumpLabels, typeTable[i].ElementCount, out mRequireSortJumps);
                        break;
                    case TypeTableType.Opcodes:
                        {
                            bool hasExtendedOpcodes;
                            mOpcodes = BfDisassembler.ParseCodeblock(reader.ReadUInt32Array(typeTable[i].ElementCount), out hasExtendedOpcodes);

                            if (hasExtendedOpcodes) // only fix up the opcode indices if they have to be
                                FixupOpcodeIndices(); // this function is kinda 2*O(n^2)
                        }
                        break;
                    case TypeTableType.Messages:
                        if (typeTable[i].ElementCount > 0)
                            mMessageFile = new BmdFile(StreamHelper.ReadStream(reader, typeTable[i].ElementCount), false);
                        break;
                    case TypeTableType.Strings:
                        // TODO: Implement this
                        break;
                }
            }
        }

        private static void ReadCodeLabels(BinaryReader reader, ref BfCodeLabel[] array, int count, out bool requireSort)
        {
            array = new BfCodeLabel[count];
            requireSort = false;
            int lastIdx = -1;
            for (int j = 0; j < array.Length; j++)
            {
                array[j] = new BfCodeLabel(reader);
                if (!requireSort && lastIdx > array[j].OpcodeIndex)
                    requireSort = true;
                else
                    lastIdx = array[j].OpcodeIndex;
            }
        }

        private void FixupOpcodeIndices()
        {
            for (int i = 0; i < mProcedures.Length; i++)
            {
                mProcedures[i].OpcodeIndex = mOpcodes.FindIndex(op => op.CodeBlockIndex == mProcedures[i].Offset);
            }

            for (int i = 0; i < mJumpLabels.Length; i++)
            {
                mJumpLabels[i].OpcodeIndex = mOpcodes.FindIndex(op => op.CodeBlockIndex == mJumpLabels[i].Offset);
            }
        }

        /*
        struct NativeFunctionCall
        {
            public int OpcodeIndex;
            public string Name;
            public List<object> Arguments;
            public int AssignmentVariableIndex;

            public NativeFunctionCall(int opIndex, string name)
            {
                OpcodeIndex = opIndex;
                Name = name;
                Arguments = new List<object>();
                AssignmentVariableIndex = -1;
            }
        }

        
        public void DecompileToScr(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (BFProcedure proc in _procedures)
                {
                    List<string> scr = new List<string>();
                    List<int> localVarIds = GetLocalVarIds(proc);
                    List<NativeFunctionCall> nativeFunctionCalls = GetNativeFunctionCalls(proc.Opcodes);

                    writer.WriteLine("procedure {0}", proc.Name);
                    writer.WriteLine("begin\n");

                    for (int i = 0; i < localVarIds.Count; i++)
                    {
                        writer.WriteLine("\tint localVar" + localVarIds[i] + ";");
                    }

                    if (localVarIds.Count > 0)
                        writer.Write("\n");

                    int nativeCallCounter = 0;
                    for (int i = 0; i < proc.Opcodes.Count; i++)
                    {
                        switch (proc.Opcodes[i].Instruction)
                        {
                            case BFInstruction.CallNative:
                                {
                                    if (nativeFunctionCalls[nativeCallCounter].OpcodeIndex != i)
                                        break;

                                    NativeFunctionCall call = nativeFunctionCalls[nativeCallCounter];
                                    writer.Write("\t");
                                    WriteNativeFunctionCall(writer, call);
                                    writer.WriteLine(";");
                                    nativeCallCounter++;
                                }
                                break;
                            case BFInstruction.CallProcedure:
                                {
                                    writer.WriteLine("\tCallProcedure({0});", _procedures[Convert.ToInt32(proc.Opcodes[i].Argument)].Name);
                                }
                                break;
                            case BFInstruction.Add:
                                break;
                            case BFInstruction.Subtract:
                                break;
                            case BFInstruction.NotEqual:
                                break;
                            case BFInstruction.JumpIfFalse:
                                break;
                            case BFInstruction.PushUInt16:
                                break;
                            case BFInstruction.PushResult:
                                break;
                            case BFInstruction.SetVariable:
                                break;
                            default:
                                break;
                        }
                    }

                    writer.WriteLine("\nend\n");
                }
            }            
        }
        

        private static List<int> GetLocalVarIds(List<BFOpcode> opcodes, int startIndex, int count)
        {
            List<int> uniqueLocalVarIds = new List<int>();
            for (int i = startIndex; i < count; i++)
            {
                if (opcodes[i].Instruction == BFInstruction.SetVariable)
                {
                    int id = (int)opcodes[i].Operand.ImmediateValue;
                    if (!uniqueLocalVarIds.Contains(id))
                    {
                        uniqueLocalVarIds.Add(id);
                    }
                }
            }
            return uniqueLocalVarIds;
        }

        private static void WriteNativeFunctionCall(StreamWriter writer, NativeFunctionCall call)
        {
            if (call.AssignmentVariableIndex != -1)
            {
                writer.Write("\n\tlocalVar{0} = ", call.AssignmentVariableIndex);
            }

            List<int> callReplacedWithLocalIndices = new List<int>();

            for (int i = 0; i < call.Arguments.Count; i++)
            {
                if (call.Arguments[i] is NativeFunctionCall)
                {
                    NativeFunctionCall argcall = (NativeFunctionCall)call.Arguments[i];
                    if (argcall.AssignmentVariableIndex != -1)
                    {
                        WriteNativeFunctionCall(writer, argcall);
                        callReplacedWithLocalIndices.Add(i);
                    }
                }
            }

            writer.Write("{0}", call.Name); // method name

            writer.Write("("); // argument parenthesis open

            for (int i = 0; i < call.Arguments.Count; i++)
            {
                if (call.Arguments[i] is NativeFunctionCall)
                {
                    if (!callReplacedWithLocalIndices.Contains(i))
                    {
                        WriteNativeFunctionCall(writer, (NativeFunctionCall)call.Arguments[i]);
                    }
                    else
                    {
                        NativeFunctionCall argcall = (NativeFunctionCall)call.Arguments[i];
                        writer.Write("localVar" + argcall.AssignmentVariableIndex);
                    }
                }
                else
                {
                    writer.Write(call.Arguments[i]);
                    if (i + 1 != call.Arguments.Count)
                        writer.Write(", ");
                }
            }

            writer.Write(")"); // argument parenthesis close

            if (call.AssignmentVariableIndex != -1)
            {
                writer.Write("\n\t");
            }
        }

        private static List<NativeFunctionCall> GetNativeFunctionCalls(List<BFOpcode> opcodes)
        {
            List<NativeFunctionCall> calls = new List<NativeFunctionCall>();

            List<BFOpcode> nativeFunctionOps = opcodes.FindAll(op => op.Instruction == BFInstruction.CallNative);

            int lastIdx = 0;
            for (int i = 0; i < nativeFunctionOps.Count; i++)
            {
                int opIdx = opcodes.IndexOf(nativeFunctionOps[i]);

                NativeFunctionCall call = new NativeFunctionCall(opIdx, "NativeFunction_" + (int)nativeFunctionOps[i].Operand.ImmediateValue);

                for (int j = 0; j < opIdx - lastIdx; j++)
                {
                    int idx = (opIdx - 1) - j;
                    switch (opcodes[idx].Instruction)
                    {
                        case BFInstruction.PushUInt32:
                        case BFInstruction.PushFloat:
                        case BFInstruction.PushVariable:
                        case BFInstruction.PushUInt16:
                            call.Arguments.Add((int)opcodes[idx].Operand.ImmediateValue);
                            break;
                        case BFInstruction.PushResult:
                            call.Arguments.Add(calls[calls.Count - 1]);
                            calls.RemoveAt(calls.Count - 1);
                            break;

                        case BFInstruction.SetVariable:
                            if (opcodes[idx - 1].Instruction == BFInstruction.PushResult)
                            {
                                int callIdx = calls.FindIndex(c => c.OpcodeIndex == idx - 2);
                                NativeFunctionCall setcall = calls[callIdx];
                                setcall.AssignmentVariableIndex = (int)opcodes[(opIdx-1)-j].Operand.ImmediateValue;
                                calls[callIdx] = setcall;
                            }
                            break;
                        default:
                            j = opIdx - lastIdx;
                            break;
                    }
                }

                lastIdx = opIdx;
                calls.Add(call);
            }

            return calls;
        }
        */
    }
}
