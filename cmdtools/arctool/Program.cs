using AtlusLibSharp.Generic.Archives;
using AtlusLibSharp.Persona3.Archives;
using System;
using System.IO;

namespace arctool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No input specified.");
                Console.WriteLine("Usage:");
                Console.WriteLine(" Enter path to ARC file to extract it to a folder of the same name.");
                Console.WriteLine(" Enter path to directory to pack into an ARC.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            if (Path.HasExtension(args[0]))
            {
                if (!GenericPSVitaArchive.VerifyFileType(args[0]))
                {
                    Console.WriteLine("This is not a proper arc file!");
                    if (GenericPAK.VerifyFileType(args[0]))
                    {
                        Console.WriteLine("Detected format: regular .bin/.pac/.pak archive.");
                    }
                    return;
                }

                GenericPSVitaArchive arc = new GenericPSVitaArchive(args[0]);
                string path = Path.GetFileNameWithoutExtension(args[0]);
                Directory.CreateDirectory(path);
                for (int i = 0; i < arc.EntryCount; i++)
                {
                    File.WriteAllBytes(path + "//" + arc.Entries[i].Name, arc.Entries[i].Data);
                }
            }
            else if (!Path.HasExtension(args[0]))
            {
                GenericPSVitaArchive arc = GenericPSVitaArchive.Create(args[0]);
                arc.Save(Path.GetFileName(args[0]) + ".arc");
            }
        }
    }
}
