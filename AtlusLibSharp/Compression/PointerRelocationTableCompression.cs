namespace AtlusLibSharp.Compression
{
    using System.Collections.Generic;

    internal static class PointerRelocationTableCompression
    {
        private const byte ADDRESS_SIZE = sizeof(int);
        private const byte SEQ_BASE = 0x07;
        private const byte SEQ_BASE_NUM_LOOP = 2;
        private const byte SEQ_FLAG_ODD = 1 << 3;

        public static int[] Decompress(byte[] addressRelocTable, int baseOffset)
        {
            List<int> addressLocs = new List<int>();
            int prevRelocSum = 0;

            for (int i = 0; i < addressRelocTable.Length; i++)
            {
                int reloc = addressRelocTable[i];

                // Check if the value is odd
                if ((reloc % 2) != 0)
                {
                    // Check if the value indicates a sequence run of addresses
                    if ((reloc & SEQ_BASE) == SEQ_BASE)
                    {
                        // Get the base loop multiplier
                        int baseLoopMult = (reloc & 0xF0) >> 4;

                        // Get the number of loops, base loop number is 2
                        int numLoop = SEQ_BASE_NUM_LOOP + (baseLoopMult * SEQ_BASE_NUM_LOOP);

                        // Check if the number of loops is odd
                        if ((reloc & SEQ_FLAG_ODD) == SEQ_FLAG_ODD)
                        {
                            // If so then add an extra loop cycle.
                            numLoop += 1;
                        }

                        for (int j = 0; j < numLoop; j++)
                        {
                            addressLocs.Add(baseOffset + prevRelocSum + 4);
                            prevRelocSum += 4;
                        }

                        // Continue the loop early so we skip adding the reloc value to the list later on
                        continue;
                    }
                    else
                    {
                        // If value isn't a sequence run then read the next byte and bitwise OR it onto the value

                        // Decrement the reloc value to remove the extra bit added to make it an odd number
                        reloc -= 1;
                        reloc |= addressRelocTable[++i] << 8;
                    }
                }
                else
                {
                    // If the value isn't odd, shift the value 1 bit to the left
                    reloc <<= 1;
                }

                addressLocs.Add(baseOffset + prevRelocSum + reloc);
                prevRelocSum += reloc;
            }

            return addressLocs.ToArray();
        }

        public static byte[] Compress(IList<int> addressLocs, int baseOffset)
        {
            int prevRelocSum = 0;
            List<byte> addressRelocBytes = new List<byte>();

            // Detect address sequence runs
            List<AddressSequence> sequences = DetectAddressSequenceRuns(addressLocs);

            for (int i = 0; i < addressLocs.Count; i++)
            {
                int seqIdx = sequences.FindIndex(item => item.addrLocationListStartIdx == i);
                int reloc = (addressLocs[i] - prevRelocSum) - baseOffset;

                // Check if a matching sequence was found
                if (seqIdx != -1)
                {
                    // We have a sequence to add.
                    // Use the first entry to position to the start of the sequence

                    // Encode the first entries' address and add it to the list of bytes
                    EncodeReloc(reloc, ref addressRelocBytes, ref prevRelocSum);

                    // Subtract one because the first entry is used to locate to the start of the sequence
                    int numSeq = sequences[seqIdx].numAddressInSeq - 1;

                    int baseLoopMult = (numSeq - SEQ_BASE_NUM_LOOP) / SEQ_BASE_NUM_LOOP;
                    bool isOdd = (numSeq % 2) == 1;

                    reloc = SEQ_BASE;
                    reloc |= baseLoopMult << 4;

                    if (isOdd)
                    {
                        reloc |= SEQ_FLAG_ODD;
                    }

                    addressRelocBytes.Add((byte)reloc);

                    i += numSeq;
                    prevRelocSum += numSeq * ADDRESS_SIZE;
                }
                else
                {
                    // Encode address and add it to the list of bytes
                    EncodeReloc(reloc, ref addressRelocBytes, ref prevRelocSum);
                }
            }

            return addressRelocBytes.ToArray();
        }

        private static void EncodeReloc(int reloc, ref List<byte> addressRelocBytes, ref int prevRelocSum)
        {
            // First we check if we can shift it to the right to shrink the value.
            // Check if lowest bit is set to see if we an shift it to the right
            if ((reloc & 0x01) == 0)
            {
                // We can shift to the right without losing data
                int newReloc = reloc >> 1;

                if (newReloc <= byte.MaxValue)
                {
                    // If the shifted reloc is within the byte size boundary, add it to the reloc byte list
                    addressRelocBytes.Add((byte)newReloc);
                }
                else
                {
                    // If it's still too big, extend it.
                    ExtendReloc(reloc, ref addressRelocBytes);
                }
            }
            else
            {
                // If we can't shift to the right to shrink it, we must extend it.
                ExtendReloc(reloc, ref addressRelocBytes);
            }

            // Add the reloc value to the current sum of reloc values
            prevRelocSum += reloc;
        }

        private static void ExtendReloc(int reloc, ref List<byte> addressRelocBytes)
        {
            // Make the low bits odd by adding 1 to them to indicate that it's an extended reloc.
            byte relocLo = (byte)((reloc & 0x00FF) + 1);
            byte relocHi = (byte)((reloc & 0xFF00) >> 8);

            addressRelocBytes.Add(relocLo);
            addressRelocBytes.Add(relocHi);
        }

        private static List<AddressSequence> DetectAddressSequenceRuns(IList<int> addressLocs)
        {
            List<AddressSequence> sequences = new List<AddressSequence>();

            for (int i = 0; i < addressLocs.Count; i++)
            {
                // There can't be any more sequences if we're on the last iteration
                if (i + 1 == addressLocs.Count)
                {
                    break;
                }

                if (addressLocs[i + 1] - addressLocs[i] == ADDRESS_SIZE)
                {
                    // We have found a sequence of at least 2 addresses
                    AddressSequence seq = new AddressSequence
                    {
                        addrLocationListStartIdx = i,
                        numAddressInSeq = 2
                    };
                    i++;
        
                    while (i + 1 < addressLocs.Count)
                    {
                        if (addressLocs[i + 1] - addressLocs[i] == ADDRESS_SIZE)
                        {
                            // We have found another sequence to add.
                            seq.numAddressInSeq++;
                            i++;
                        }
                        else
                        {
                            // The consecutive sequence ends.
                            break;
                        }
                    }

                    // Check if there are more than 2 addresses in a sequence.
                    if (seq.numAddressInSeq > 2)
                    {
                        // Add the sequence to the list of sequences.
                        sequences.Add(seq);
                    }
                }
            }

            return sequences;
        }

        private struct AddressSequence
        {
            public int addrLocationListStartIdx;
            public int numAddressInSeq;
        }
    }
}
