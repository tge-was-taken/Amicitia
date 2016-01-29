using AtlusLibSharp.SMT3.ChunkResources;
using AtlusLibSharp.SMT3.ChunkResources.Scripting;
using System;
using System.IO;

namespace msgtool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No input specified.");
                Console.WriteLine("Usage:");
                Console.WriteLine(" Enter path to BMD file to convert BMD file to XML.");
                Console.WriteLine(" Enter path to XML file to convert XML file to BMD.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            if (args[0].EndsWith(".BMD", StringComparison.InvariantCultureIgnoreCase))
            {
                MSGChunk msg = ChunkFactory.Get<MSGChunk>(args[0]);

                if (msg == null)
                {
                    Console.WriteLine("Could not read BMD.");
                    Console.ReadKey();
                    return;
                }

                msg.SaveXml(args[0] + ".XML");
            }
            else if (args[0].EndsWith(".XML", StringComparison.InvariantCultureIgnoreCase))
            {
                MSGChunk msg;
                try
                {
                    msg = new MSGChunk(args[0]);
                }
                catch (InvalidDataException)
                {
                    Console.WriteLine("Xml header element name mismatch.\nAre you sure the xml was exported by this tool?");
                    Console.ReadKey();
                    return;
                }

                msg.Save(args[0].Replace(".XML", ".BMD"));
            }
        }
    }
}
