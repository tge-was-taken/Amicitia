namespace paktool
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
                Console.WriteLine(" Enter path to PAK file to extract it to a folder of the same name.");
                Console.WriteLine(" Enter path to directory to pack into a PAK.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            if (Path.HasExtension(args[0]))
            {
                if (!PAKToolArchiveFile.VerifyFileType(args[0]))
                {
                    Console.WriteLine("This is not a proper pak file!");
                    if (ListArchiveFile.VerifyFileType(args[0]))
                    {
                        Console.WriteLine("Detected format: Vita .arc archive.");
                    }
                    return;
                }

                PAKToolArchiveFile pak = new PAKToolArchiveFile(args[0]);
                string path = Path.GetFileNameWithoutExtension(args[0]);
                Directory.CreateDirectory(path);
                for (int i = 0; i < pak.EntryCount; i++)
                {
                    File.WriteAllBytes(path + "//" + pak.Entries[i].Name, pak.Entries[i].Data);
                }
            }
            else if (!Path.HasExtension(args[0]))
            {
                PAKToolArchiveFile pak = PAKToolArchiveFile.Create(args[0]);
                pak.Save(Path.GetFileName(args[0]) + ".PAK");
            }
        }
    }
}
