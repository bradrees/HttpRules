using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace RulesWPF
{
    [Serializable]
    public class Preferences
    {
        private static Preferences instance;

        private static string filename = "Preferences.xml";

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
            get
            {
                if (instance == null)
                {
                    instance = Preferences.Load();
                }

                return instance;
            }
        }

        public static Preferences Load()
        {
            var serializer = new XmlSerializer(typeof(Preferences));
            if (File.Exists(filename))
            {
                try
                {
                    using (var file = File.OpenRead(filename))
                    {
                        return (Preferences)serializer.Deserialize(file);
                    }
                }
                catch { }
            }

            return new Preferences(true);
        }

        public static void Save()
        {
            var serializer = new XmlSerializer(typeof(Preferences));
            using (var file = File.OpenWrite(filename))
            {
                file.SetLength(0);
                serializer.Serialize(file, instance);
                file.Flush();
                file.Close();
            }
        }
        
        public int MaxLogLength { get; set; }

        public bool Enabled { get; set; }

        public bool EnableLogging { get; set; }
    }
}
