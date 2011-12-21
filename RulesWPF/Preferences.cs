using System;
using System.IO;
using System.Xml.Serialization;

namespace RulesWPF
{
    [Serializable]
    public class Preferences
    {
        private static Preferences _instance;

        private static readonly string Filename =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "HttpRules\\Preferences.xml");

        private Preferences() : this(false)
        {
        }

        private Preferences(bool loadDefaults)
        {
            if (loadDefaults)
            {
                this.MaxLogLength = 1000;
                this.Enabled = true;
                this.EnableLogging = true;
            }
        }

        public static Preferences Current
        {
            get { return _instance ?? (_instance = Load()); }
        }

        public int MaxLogLength { get; set; }

        public bool Enabled { get; set; }

        public bool EnableLogging { get; set; }

        public static Preferences Load()
        {
            var serializer = new XmlSerializer(typeof (Preferences));
            if (File.Exists(Filename))
            {
                try
                {
                    using (var file = File.OpenRead(Filename))
                    {
                        return (Preferences) serializer.Deserialize(file);
                    }
                }
                catch
                {
                }
            }

            return new Preferences(true);
        }

        public static void Save()
        {
            var serializer = new XmlSerializer(typeof (Preferences));
            using (var file = File.OpenWrite(Filename))
            {
                file.SetLength(0);
                serializer.Serialize(file, _instance);
                file.Flush();
                file.Close();
            }
        }
    }
}