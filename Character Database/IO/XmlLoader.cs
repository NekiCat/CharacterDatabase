using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Character_Database.IO
{
    class XmlLoader
    {
        public ICollection<Character> Read(string filename)
        {
            var result = new List<Character>();

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            foreach (XmlNode node in doc.SelectNodes("/CharacterDatabase/Characters/Character"))
            {
                var character = new Character(node.Attributes.GetNamedItem("Name").Value);
                character.Tags = node.Attributes.GetNamedItem("Tags").Value;
                character.Picture = node.Attributes.GetNamedItem("Picture").Value;
                character.Description = node.InnerText;

                result.Add(character);
            }

            return result;
        }

        public void Write(string filename, IList<Character> list)
        {
            XmlDocument doc = new XmlDocument();
            var root = doc.CreateElement("CharacterDatabase");
            var version = doc.CreateAttribute("Version");
            version.Value = "1.0";
            root.Attributes.Append(version);
            doc.AppendChild(root);

            var characters = doc.CreateElement("Characters");
            root.AppendChild(characters);

            foreach (var character in list)
            {
                var tag = doc.CreateElement("Character");

                var name = doc.CreateAttribute("Name");
                name.Value = character.Name;
                tag.Attributes.Append(name);

                var tags = doc.CreateAttribute("Tags");
                tags.Value = character.Tags;
                tag.Attributes.Append(tags);

                var pic = doc.CreateAttribute("Picture");
                pic.Value = character.Picture;
                tag.Attributes.Append(pic);

                tag.InnerText = character.Description;
                characters.AppendChild(tag);
            }

            doc.Save(filename);
        }
    }
}
