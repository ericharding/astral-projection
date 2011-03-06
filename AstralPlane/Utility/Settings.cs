using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Markup;
using System.Xml;
using System.Text;

namespace Astral.Plane.Utility
{
    public class Settings : INotifyPropertyChanged
    {
        private Hashtable _settingsTable = new Hashtable();
        private object _syncRoot = new object();
        const string DEFAULT_FILENAME = "config.xml";

        public object this[object setting]
        {
            get
            {
                lock (_syncRoot)
                {
                    return _settingsTable[setting];
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    if (value != null)
                    {
                        _settingsTable[setting] = value;
                    }
                    else
                    {
                        _settingsTable.Remove(setting);
                    }
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Item[]"));
                }
            }
        }

        public T Get<T>(string setting, T defaultValue)
        {
            lock(_syncRoot)
            {
                if (_settingsTable.ContainsKey(setting))
                {
                    return (T)this[setting];
                }
            }
            return defaultValue;
        }
        
        public void Set<T>(string setting, T value)
        {
            this[setting] = value;
        }

        #region internal initializer properties
        public static string ApplicationName
        {
            get
            {
                string assemblyName = Assembly.GetEntryAssembly().FullName;
                return assemblyName.Substring(0, assemblyName.IndexOf(','));
            }
        }

        private DirectoryInfo SettingsDirectory
        {
            get
            {
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + ApplicationName;
                DirectoryInfo dInfo = new DirectoryInfo(dir);
                if (!dInfo.Exists) dInfo.Create();
                return dInfo;
            }
        }
        #endregion

        public void Save()
        {
            Save(DEFAULT_FILENAME, true);
        }

        public void Save(bool formated)
        {
            Save(DEFAULT_FILENAME, formated);
        }

        public void Save(string filename)
        {
            Save(filename, false);
        }

        public void Save(string filename, bool formated)
        {
            lock (_syncRoot)
            {
                string filePath = SettingsDirectory.FullName + Path.DirectorySeparatorChar + filename;
                using (Stream writeStream = File.Open(filePath, FileMode.Create))
                {
                    if (formated)
                    {
                        XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, NewLineOnAttributes = true };
                        XamlWriter.Save(_settingsTable, XmlWriter.Create(writeStream, settings));
                    }
                    else
                    {
                        XamlWriter.Save(_settingsTable, writeStream);
                    }
                }
            }
        }

        public void Load()
        {
            Load(DEFAULT_FILENAME);
        }

        public void Load(string filename)
        {
            lock (_syncRoot)
            {
                string filePath = SettingsDirectory.FullName + Path.DirectorySeparatorChar + filename;
                try
                {
                    if (File.Exists(filePath))
                    {
                        using (var stream = File.OpenRead(filePath))
                        {
                            object oSettings = XamlReader.Load(stream);
                            Hashtable newSettings = oSettings as Hashtable;
                            if (newSettings != null)
                            {
                                _settingsTable = newSettings;
                            }
                        }
                    }
                }
                catch (XamlParseException) { File.Delete(filePath); }
                catch (XmlException) { }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }
}
