using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace CharacterDatabase.IO
{
    class Settings
    {
        private string mSettingsFilename;

        public Settings(string filename)
        {
            this.mSettingsFilename = filename;

            if (File.Exists(this.mSettingsFilename))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(this.mSettingsFilename);

                foreach (XmlNode node in doc.SelectNodes("/CharacterDatabase/Settings/Property"))
                {
                    switch (node.Attributes.GetNamedItem("Name").Value)
                    {
                        case "SearchCaseSensitive":
                            this.mSearchCaseSensitive = Boolean.Parse(node.Attributes.GetNamedItem("Value").Value);
                            break;
                        case "SearchWhenTyping":
                            this.mSearchWhenTyping = Boolean.Parse(node.Attributes.GetNamedItem("Value").Value);
                            break;
                        case "MainSidebarWidth":
                            this.mMainSidebarWidth = Int32.Parse(node.Attributes.GetNamedItem("Value").Value);
                            break;
                    }
                }
            }
        }

        #region Backing Fields

        private bool mSearchCaseSensitive = false;
        private bool mSearchWhenTyping = true;
        private int mMainSidebarWidth = 250;

        #endregion

        #region Properties

        /// <summary>
        /// Whether a search for tags should be case sensitive
        /// </summary>
        public bool SearchCaseSensitive {
            get
            {
                return this.mSearchCaseSensitive;
            }
            set
            {
                this.mSearchCaseSensitive = value;
                this.Save("SearchCaseSensitive", this.mSearchCaseSensitive.ToString());
            }
        }

        /// <summary>
        /// Whether a search should start immediately when typing
        /// </summary>
        public bool SearchWhenTyping {
            get
            {
                return this.mSearchWhenTyping;
            }
            set
            {
                this.mSearchWhenTyping = value;
                this.Save("SearchWhenTyping", this.mSearchWhenTyping.ToString());
            }
        }

        /// <summary>
        /// The width of the sidebar in the main window in pixels
        /// </summary>
        public int MainSidebarWidth
        {
            get
            {
                return this.mMainSidebarWidth;
            }
            set
            {
                this.mMainSidebarWidth = value;
                this.Save("MainSidebarWidth", this.mMainSidebarWidth.ToString());
            }
        }

        #endregion

        #region Saving

        private void Save(string property, string value)
        {
            XmlDocument doc = new XmlDocument();
            if (File.Exists(this.mSettingsFilename)) doc.Load(this.mSettingsFilename);

            foreach (XmlNode node in doc.SelectNodes("/CharacterDatabase/Settings/Property"))
            {
                if (node.Attributes.GetNamedItem("Name").Value == property)
                {
                    node.Attributes.GetNamedItem("Value").Value = value;
                    doc.Save(this.mSettingsFilename);
                    return;
                }
            }

            XmlNode nnode = doc.CreateElement("Property");
            XmlAttribute name = doc.CreateAttribute("Name");
            name.Value = property;

            XmlAttribute val = doc.CreateAttribute("Value");
            val.Value = value;

            nnode.Attributes.Append(name);
            nnode.Attributes.Append(val);

            if (doc.SelectSingleNode("/CharacterDatabase") == null)
            {
                XmlNode root = doc.CreateElement("CharacterDatabase");
                XmlAttribute version = doc.CreateAttribute("version");
                version.Value = "1.0";

                root.Attributes.Append(version);
                doc.AppendChild(root);
            }

            if (doc.SelectSingleNode("/CharacterDatabase/Settings") == null)
            {
                XmlNode settings = doc.CreateElement("Settings");
                doc.SelectSingleNode("/CharacterDatabase").AppendChild(settings);
            }

            doc.SelectSingleNode("/CharacterDatabase/Settings").AppendChild(nnode);
            doc.Save(this.mSettingsFilename);
        }

        #endregion
    }
}
