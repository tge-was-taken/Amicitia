using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Amicitia
{
    public delegate void Loop();

    internal static class Program
    {
        public static Assembly Assembly = Assembly.GetExecutingAssembly();
        public static string Name = Assembly.GetName().Name;
        public static string Version = Assembly.GetName().Version.ToString();
        public static DateTime BuildTime = Assembly.GetLinkerTime();
        public static Type[] Types = Assembly.GetTypes();
        public static List<Loop> LoopFunctions;

        // increment these
        public static int VersionMajor = 0;
        public static int VersionMinor = 3;

#if DEBUG
        public static string TitleString = string.Format("Amicitia v{0}.{1} [DEBUG]", VersionMajor, VersionMinor);
#else
        public static string TitleString = string.Format("Amicitia v{0}.{1}", VersionMajor, VersionMinor);
#endif

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            LoopFunctions = new List<Loop>();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm()); //classical way of doing forms but we want to make a message pump
            //oh boy here we go
            MainForm form = new MainForm();
            bool running = true;
            form.FormClosed += (object sender, FormClosedEventArgs e) => { running = false; };
            form.Show();
            do
            {
                Application.DoEvents();

                if(LoopFunctions.Count > 0)
                    foreach (Loop func in LoopFunctions)
                        func();

                //System.Threading.Thread.Sleep(1);
            } while (running);
        }
    }

    internal static class AssemblyExtension
    {
        public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }
    }
}
