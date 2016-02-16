using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PersonaModManager
{
    internal partial class Form1 : Form
    {
        public static string SettingsFileName = "PersonaModManagerSettings";
        public static string ModsPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + "mods";
        public static List<ModDatabaseEntry> ModDatabaseEntries;

        public Form1()
        {
            InitializeComponent();

            if(DoesSettingsFileExist())
            {
                LoadSettingsFile(ModsPath + "\\" + SettingsFileName + ".xml");
            }
            else
            {
                CreateSettingsFile(ModsPath + "\\" + SettingsFileName + ".xml");
            }

            foreach (ModDatabaseEntry entry in ModDatabaseEntries)
            {
                dataGridView1.Rows.Add(entry.Active, entry.Configuration.Title, entry.Configuration.Version, entry.Configuration.Author);
            }
        }

        private bool DoesSettingsFileExist()
        {
            if (Directory.Exists(ModsPath))
            {
                return Directory.GetFiles(ModsPath, SettingsFileName + ".xml", SearchOption.TopDirectoryOnly).Length == 1;
            }
            else
            {
                return false;
            }
        }

        private void LoadSettingsFile(string path)
        {
            XDocument document = XDocument.Load(path);
            XElement rootElement = document.Root;

            if (rootElement.Name != SettingsFileName)
            {
                CreateSettingsFile(ModsPath + "\\" + SettingsFileName + ".xml");
                LoadSettingsFile(ModsPath + "\\" + SettingsFileName + ".xml");
            }

            foreach (XElement element in rootElement.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "Config":
                        // Add config loading code here
                        break;
                    case "ModDatabase":
                        LoadModDatabase(element);
                        break;
                }
            } 
        }

        private void LoadModDatabase(XElement modsElement)
        {
            ModDatabaseEntries = new List<ModDatabaseEntry>();

            foreach (XElement element in modsElement.Elements())
            {
                ModDatabaseEntries.Add(new ModDatabaseEntry(element));
            }
        }

        private void CreateSettingsFile(string path)
        {

        }
    }

    internal class ModDatabaseEntry
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public bool Active { get; private set; }
        public ModConfiguration Configuration { get; private set; }

        public ModDatabaseEntry(XElement element)
        {
            Name = element.Attribute("Name").Value;
            Path = element.Attribute("Path").Value;
            Active = bool.Parse(element.Attribute("Active").Value);
            Configuration = new ModConfiguration(Form1.ModsPath + "\\" + Path);
        }

        public XElement GetXmlElement()
        {
            XElement element = 
                    new XElement("Mod",
                        new XAttribute("Name", Name),
                        new XAttribute("Path", Path),
                        new XAttribute("Active", Active.ToString())
                    );

            return element;
        }
    }

    internal class ModConfiguration
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Version { get; private set; }
        public string Date { get; private set; }
        public string Author { get; private set; }
        public string URL { get; private set; }

        public ModConfiguration(string path)
        {
            if (!File.Exists(path))
            {
                // Create new config here?
            }

            XDocument document = XDocument.Load(path);

            foreach (XElement element in document.Root.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "Title":
                        Title = element.Value;
                        break;

                    case "Description":
                        Description = element.Value;
                        break;

                    case "Version":
                        Version = element.Value;
                        break;

                    case "Date":
                        Date = element.Value;
                        break;

                    case "Author":
                        Author = element.Value;
                        break;

                    case "URL":
                        URL = element.Value;
                        break;
                }
            }
        }

        public XElement GetXmlElement()
        {
            XElement element = 
                new XElement("PersonaModManagerModConfig",
                    new XElement("Title", Title),
                    new XElement("Description", Description),
                    new XElement("Version", Version),
                    new XElement("Date", Date),
                    new XElement("Author", Author),
                    new XElement("URL", URL)
                    );

            return element;
        }
    }
}
