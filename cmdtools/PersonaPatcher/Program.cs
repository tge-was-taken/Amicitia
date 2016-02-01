using AtlusLibSharp.Persona3.CRIWare;
using AtlusLibSharp.Utilities;
using System;
using System.IO;
using System.Timers;

namespace PersonaPatcher
{
    class Program
    {
        // Elf size constants
        public const int ELF_SIZE_PERSONA4_NTSC     = 0x838C1C;
        public const int ELF_SIZE_PERSONA3FES_NTSC  = 0x8ACE9C;
        public const int ELF_SIZE_PERSONA3_NTSC     = 0x79DD9C;

        // Cvm listing offset constants
        public const int CVM_LIST_OFFSET_PERSONA4_NTSC      = 0x4598C0;
        public const int CVM_LIST_OFFSET_PERSONA3FES_NTSC   = 0x4E51D0;
        public const int CVM_LIST_OFFSET_PERSONA3_NTSC      = 0x4E5FA0;

        // Cvm order
        public static string[] CVM_ORDER_PERSONA4 = new string[4]
        {
            "DATA", "BGM", "BTL", "ENV"
        };

        public static string[] CVM_ORDER_PERSONA3 = new string[3]
        {
            "DATA", "BGM", "BTL"
        };

        static void Main(string[] args)
        {
            Console.SetBufferSize(Console.BufferWidth, 32766);

            if (args.Length != 2)
            {
                Console.WriteLine("Usage: \n");
                Console.WriteLine("PersonaPatcher.exe [SLUS FILE] [CVM FILE]");
                Console.WriteLine("Tool written by TGE, please give credit where is due! :^)");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            // Declare variables
            byte[] elfHeader;
            byte[] elfFooter;
            string[] cvmOrder = null;
            CVMExecutableListing[] cvmExecutableListings;

            using (FileStream stream = File.OpenRead(args[0]))
            {
                int cvmListingOffset = 0;

                switch (stream.Length)
                {
                    case ELF_SIZE_PERSONA4_NTSC: // Persona 4 NTSC
                        cvmListingOffset = CVM_LIST_OFFSET_PERSONA4_NTSC;
                        cvmOrder = CVM_ORDER_PERSONA4;
                        break;

                    case ELF_SIZE_PERSONA3FES_NTSC: // Persona 3 FES NTSC
                        cvmListingOffset = CVM_LIST_OFFSET_PERSONA3FES_NTSC;
                        cvmOrder = CVM_ORDER_PERSONA3;
                        break;

                    case ELF_SIZE_PERSONA3_NTSC: // Persona 3 NTSC
                        cvmListingOffset = CVM_LIST_OFFSET_PERSONA3_NTSC;
                        cvmOrder = CVM_ORDER_PERSONA3;
                        break;
                }

                if (cvmListingOffset == 0)
                {
                    Console.WriteLine("Error: Executable not supported");
                    return;
                }

                // read data before list
                elfHeader = stream.ReadBytes(cvmListingOffset);

                // Read cvm lists
                cvmExecutableListings = new CVMExecutableListing[cvmOrder.Length];
                for (int i = 0; i < cvmOrder.Length; i++)
                {
                    cvmExecutableListings[i] = new CVMExecutableListing(stream);
                }

                // read data after listing
                elfFooter = stream.ReadBytes((int)(stream.Length - stream.Position));
            }

            // Load cvm
            CVMFile cvm = new CVMFile(args[1]);

            // Get the index from the cvm order
            // Check if the name of the cvm at least contains the original name
            string cvmName = Path.GetFileNameWithoutExtension(args[1]).ToUpperInvariant();
            int cvmIndex = Array.FindIndex(cvmOrder, o => cvmName.Contains(o));

            if (cvmIndex < 0)
            {
                Console.WriteLine("Error: Can't identify cvm.. Did you rename it to something else?");
                Console.WriteLine("The name of the cvm has to at least contain the original name for the program to be able to identify it.");
                return;
            }

            // Update the listing
            cvmExecutableListings[cvmIndex].Update(cvm);

            // Write the new executable
            using (BinaryWriter writer = new BinaryWriter(File.Create(args[0])))
            {
                writer.Write(elfHeader);
                foreach (CVMExecutableListing cvmExecutableList in cvmExecutableListings)
                {
                    cvmExecutableList.Save(writer.BaseStream);
                }
                writer.Write(elfFooter);
            }

            Console.WriteLine("\nDone!, press any key to continue");
            Console.ReadKey();
        }
    }
}
