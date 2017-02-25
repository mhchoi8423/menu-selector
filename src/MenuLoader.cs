using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace MenuSelector
{
    [Serializable]
    public class Menu
    {
        [XmlAttribute]
        public string Name { get; set; }
    }

    [Serializable]
    [XmlRoot]
    public class MenuList
    {
        [XmlElement]
        public Menu[] Menu { get; set; }
    }

    public class MenuLoader
    {
        public bool Load(out List<string> menus)
        {
            menus = new List<string>();

            try
            {
                var serializer = new XmlSerializer(typeof(MenuList));
                var reader = new StreamReader("Menu.xml");
                var list = (MenuList)serializer.Deserialize(reader);
                reader.Close();

                menus.AddRange(list.Menu.Select(menu => menu.Name));

                if (menus.Any() == false)
                {
                    Logger.Log("menu is empty");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);

                return false;
            }
        }
    }
}
