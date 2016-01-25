using System;
using System.Windows.Forms;

namespace Amicitia
{
    static class Program
    {
        public static string Name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        public static string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

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
}
