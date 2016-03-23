using AtlusLibSharp.Scripting;
using System;
using System.IO;

namespace bftool
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            if (args.Length == 0)
            {
                Console.WriteLine("No input specified.");
                Console.WriteLine("Usage:");
                Console.WriteLine(" Enter path to BF file to extract it to an XML and a folder of the same name.");
                Console.WriteLine(" Enter path to XML file to pack into a BF.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            if (args[0].EndsWith(".BF", StringComparison.InvariantCultureIgnoreCase))
            {
                BFFile bf = BFFile.LoadFromFile(args[0]);
                string baseName = Path.GetFileNameWithoutExtension(args[0]);
                //bf.Extract(Path.GetDirectoryName(baseName) + baseName + "\\" + baseName);
            }
            else if (args[0].EndsWith(".XML", StringComparison.InvariantCultureIgnoreCase))
            {
                BFFile bf;

                try
                {
                    bf = new BFFile(args[0]);
                }
                catch (InvalidDataException)
                {
                    Console.WriteLine("Xml root element name mismatch.\nAre you sure it was exported by this tool?");
                    Console.ReadKey();
                    return;
                }

                bf.Save(args[0].Replace(".XML", ".BF"));
            }
            */
        }
    }
}
