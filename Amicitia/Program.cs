using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Amicitia
{
    internal static class Program
    {
        public static Assembly Assembly = Assembly.GetExecutingAssembly();
        public static string Name = Assembly.GetName().Name;
        public static string Version = Assembly.GetName().Version.ToString();
        public static DateTime BuildTime = Assembly.GetLinkerTime();

#if DEBUG
        public static string TitleString = string.Format("Amicitia {0}/{1}/{2} [DEBUG]", BuildTime.Day, BuildTime.Month, BuildTime.Year);
#else
        public static string TitleString = string.Format("Amicitia {0}/{1}/{2}", BuildTime.Day, BuildTime.Month, BuildTime.Year);
#endif

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
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
