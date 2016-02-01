using AtlusLibSharp.Generic.Archives;
using AtlusLibSharp.Persona3.Archives;
using System;
using System.IO;

namespace archiveconvert
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No input specified.");
                Console.WriteLine("Usage:");
                Console.WriteLine(" Enter path to an ARC file to convert it to a PAK file OR");
                Console.WriteLine(" Enter path to a PAK file to convert it to an ARC file.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            if (GenericPSVitaArchive.VerifyFileType(args[0]))
            {
                GenericPSVitaArchive arc = new GenericPSVitaArchive(args[0]);
                for (int i = 0; i < arc.EntryCount; i++)
                {
                    GenericVitaArchiveEntry entry = arc.Entries[i];
                    ConvertEntry(ref entry);
                    arc.Entries[i] = entry;
                }

                GenericPAK pak = GenericPAK.Create(arc);
                pak.Save(args[0] + ".pak");
            }
            else if (GenericPAK.VerifyFileType(args[0]))
            {
                GenericPAK pak = new GenericPAK(args[0]);
                for (int i = 0; i < pak.EntryCount; i++)
                {
                    GenericPAKEntry entry = pak.Entries[i];
                    ConvertEntry(ref entry);
                    pak.Entries[i] = entry;
                }

                GenericPSVitaArchive arc = GenericPSVitaArchive.Create(pak);
                arc.Save(args[0] + ".arc");
            }
            else
            {
                Console.WriteLine("File supplied is not an ARC or PAK file!");
            }
        }

        static void ConvertEntry(ref GenericVitaArchiveEntry entry)
        {
            using (MemoryStream mStream = new MemoryStream(entry.Data))
            {
                if (GenericPSVitaArchive.VerifyFileType(mStream))
                {
                    GenericPSVitaArchive subArc = new GenericPSVitaArchive(mStream);
                    for (int i = 0; i < subArc.EntryCount; i++)
                    {
                        GenericVitaArchiveEntry subEntry = subArc.Entries[i];
                        ConvertEntry(ref subEntry);
                        subArc.Entries[i] = subEntry;
                    }

                    entry = new GenericVitaArchiveEntry(entry.Name, GenericPAK.Create(subArc).GetBytes());
                }
            }
        }

        static void ConvertEntry(ref GenericPAKEntry entry)
        {
            using (MemoryStream mStream = new MemoryStream(entry.Data))
            {
                if (GenericPAK.VerifyFileType(mStream))
                {
                    GenericPAK subPak = new GenericPAK(mStream);
                    for (int i = 0; i < subPak.EntryCount; i++)
                    {
                        GenericPAKEntry subEntry = subPak.Entries[i];
                        ConvertEntry(ref subEntry);
                        subPak.Entries[i] = subEntry;
                    }

                    entry = new GenericPAKEntry(entry.Name, GenericPSVitaArchive.Create(subPak).GetBytes());
                }
            }
        }
    }
}
