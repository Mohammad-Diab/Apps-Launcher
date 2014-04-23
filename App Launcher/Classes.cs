using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;

namespace App_Launcher
{
    [Serializable]
    class AppElements : IComparable
    {
        public string app_name;
        public string app_path;
        public int priority;
        public int used_times;
        public DateTime last_used;

        public AppElements()
        {

        }

        public AppElements(string name, string location, int priority)
        {
            app_name = name;
            app_path = location;
            this.priority = priority;
            used_times = 0;
            last_used = new DateTime(0);
        }

        public bool AppComporator(AppElements app)
        {
            if (app_path.ToLower().Equals(app.app_path.ToLower()))
                return true;
            return false;
        }

        public bool ExistedIn(List<AppElements> apps, int x)
        {
            for (int i = 0; i < apps.Count; i++)
                if (i == x)
                    continue;
                else
                    if (app_path.ToLower().Equals(apps[i].app_path.ToLower()))
                        return true;
            return false;
        }

        public int CompareTo(object ob)
        {
            AppElements obj = (AppElements)ob;
            double p1 = priority * 0.65 + used_times * 0.35;
            double p2 = obj.priority * 0.65 + obj.used_times * 0.35;
            if (p1 > p2)
                return 1;
            else if (p2 > p1)
                return -1;
            else
                if (last_used.Ticks > obj.last_used.Ticks)
                    return 1;
                else if (last_used.Ticks < obj.last_used.Ticks)
                    return -1;
                else
                    return string.Compare(app_name, obj.app_name);
        }
    }

    [Serializable]
    static class OrginalList
    {
        static public List<AppElements> orginal_list;
        static public Thickness name_thickness = new Thickness(85, 5, 100, 0);
        static public Thickness path_thickness = new Thickness(115, 28, 100, 0);
        static public Thickness ch_thickness = new Thickness(10, 5, 0, 0);
        static public Thickness img_thickness = new Thickness(31, 0, 0, 5);
        static public Thickness rectangle_thickness = new Thickness(10, 0, 10, 1);
        static public Thickness scroll_bar_thickness = new Thickness(0, 13, 10, 0);
        static public Thickness priority_thickness = new Thickness(0, 20, 32, 0);

        static public bool SaveFile(out string error)
        {
            try
            {
                Stream stream = File.Open(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\App Launcher\\applist", FileMode.Truncate);
                BinaryWriter sw = new BinaryWriter(stream);
                sw.BaseStream.WriteByte(12);
                sw.BaseStream.WriteByte(21);
                sw.BaseStream.WriteByte(14);
                sw.BaseStream.WriteByte(1);
                sw.BaseStream.WriteByte(38);
                sw.BaseStream.WriteByte(77);
                sw.BaseStream.WriteByte(65);
                BinaryFormatter ii = new BinaryFormatter();
                ii.Serialize(stream, OrginalList.orginal_list);
                stream.Close();
                Properties.Settings.Default.Save();
                error = "No error";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
