namespace archiveconvert
{
    using System;
    using System.IO;
    using AtlusLibSharp.FileSystems.ListArchive;
    using AtlusLibSharp.FileSystems.PAKToolArchive;

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

            if (ListArchiveFile.VerifyFileType(args[0]))
            {
                ListArchiveFile arc = new ListArchiveFile(args[0]);
                for (int i = 0; i < arc.EntryCount; i++)
                {
                    ListArchiveFileEntry entry = arc.Entries[i];
                    ConvertEntry(ref entry);
                    arc.Entries[i] = entry;
                }

                PAKToolArchiveFile pak = PAKToolArchiveFile.Create(arc);
                pak.Save(args[0] + ".pak");
            }
            else if (PAKToolArchiveFile.VerifyFileType(args[0]))
            {
                PAKToolArchiveFile pak = new PAKToolArchiveFile(args[0]);
                for (int i = 0; i < pak.EntryCount; i++)
                {
                    PAKToolArchiveEntry entry = pak.Entries[i];
                    ConvertEntry(ref entry);
                    pak.Entries[i] = entry;
                }

                ListArchiveFile arc = ListArchiveFile.Create(pak);
                arc.Save(args[0] + ".arc");
            }
            else
            {
                Console.WriteLine("File supplied is not an ARC or PAK file!");
            }
        }

        static void ConvertEntry(ref ListArchiveFileEntry entry)
        {
            using (MemoryStream mStream = new MemoryStream(entry.Data))
            {
                if (ListArchiveFile.VerifyFileType(mStream))
                {
                    ListArchiveFile subArc = new ListArchiveFile(mStream);
                    for (int i = 0; i < subArc.EntryCount; i++)
                    {
                        ListArchiveFileEntry subEntry = subArc.Entries[i];
                        ConvertEntry(ref subEntry);
                        subArc.Entries[i] = subEntry;
                    }

                    entry = new ListArchiveFileEntry(entry.Name, PAKToolArchiveFile.Create(subArc).GetBytes());
                }
            }
        }

        static void ConvertEntry(ref PAKToolArchiveEntry entry)
        {
            using (MemoryStream mStream = new MemoryStream(entry.Data))
            {
                if (PAKToolArchiveFile.VerifyFileType(mStream))
                {
                    PAKToolArchiveFile subPak = new PAKToolArchiveFile(mStream);
                    for (int i = 0; i < subPak.EntryCount; i++)
                    {
                        PAKToolArchiveEntry subEntry = subPak.Entries[i];
                        ConvertEntry(ref subEntry);
                        subPak.Entries[i] = subEntry;
                    }

                    entry = new PAKToolArchiveEntry(entry.Name, ListArchiveFile.Create(subPak).GetBytes());
                }
            }
        }
    }
}
