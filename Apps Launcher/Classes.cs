using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;

namespace Apps_Launcher
{
    public enum WindowVisibility
    {
        Show = 1,
        Hide = 2,
        Visible = 3,
        UnVisible = 4
    }

    public enum HideLayout
    {
        HideToLeft = 1,
        HideToRight = 2
    }

    [Serializable]
    class AppItem : IComparable
    {
        public double point;

        string name;
        public string AppName
        {
            get { return name; }
            set { name = value; }
        }

        string path;
        public string Location
        {
            get { return path; }
        }

        byte priority;
        public byte Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        long used_times;
        public long UsedTimes
        {
            get { return used_times; }
            set { used_times = value; }
        }

        DateTime last_used;
        public DateTime LastUsed
        {
            get { return last_used; }
            set { last_used = value; }
        }

        public AppItem(string name, string location, byte priority)
        {
            point = priority * 0.65 + used_times * 0.35;
            AppName = name;
            path = location;
            this.priority = priority;
            used_times = 0;
            last_used = new DateTime(0);
        }

        public int CompareTo(object ob)
        {
            return -point.CompareTo(((AppItem)ob).point);
        }

        public void CalcPoint(string search)
        {
            //equal name or equal name with charecter sensitive
            //arabic letter problem أآ ئ
            double j = 50 * Priority;
            double h = UsedTimes * 250 / AppList.frequancy;
            double k = 0;
            if (last_used.Ticks < DateTime.Now.Ticks)
            {
                k = (DateTime.Now.Ticks / 600000000) - (last_used.Ticks / 600000000);
                if (k > 4320)
                    k = 0;
                else
                    k = (4360 - k) * 100 / 4360;
            }
            double l = 0;
            if (search.Length > 0)
            {
                if (name.ToLower().Contains(search))
                {
                    l += 55;
                    int x = name.ToLower().IndexOf(search);
                    l += ((name.Length - x) * 50 / name.Length);
                }
                if (Path.GetFileName(Location).ToLower().Contains(search))
                    l += 30;
                if (Location.ToLower().Contains(search))
                    l += 15;
                if(search.Length > 2)
                {
                    string[] sp = { search };
                    int x = name.ToLower().Split(sp, StringSplitOptions.None).Length + Path.GetFileName(Location).ToLower().Split(sp, StringSplitOptions.None).Length;
                    if (x > 7)
                        l += 50;
                    else
                        l += x * 7;
                }
            }
            point = h + j + k + l;
        }

        public bool ChangeLocation(string new_location)
        {
            if (path.ToLower() == new_location.ToLower())
                return true;
            if (AppList.Dictionary.ContainsKey(new_location.ToLower()))
                return false;
            AppList.Dictionary.Remove(path.ToLower());
            path = new_location;
            AppList.Dictionary.Add(path.ToLower(), this);
            AppList.ListHasChanged = true;
            return true;
        }
    }

    [Serializable]
    static class AppList
    {
        public static HideLayout Layout = HideLayout.HideToLeft;
        static public Dictionary<string, AppItem> Dictionary;
        static public List<AppItem> List;
        static public long frequancy = 1;
        static public bool ListHasChanged = false;
        static public bool SuccessfulStartUp;

        public static bool AddItem(string path, string name, byte priority, out string error)
        {
            if (List == null)
            {
                List = new List<AppItem>();
                Dictionary = new Dictionary<string, AppItem>();
            }
            if (Dictionary.ContainsKey(path.ToLower()))
            {
                error = "App already exists in the list";
                return false;
            }
            var tmp = new AppItem(name, path, priority);
            Dictionary.Add(path.ToLower(), tmp);
            List.Add(tmp);
            ListHasChanged = true;
            error = "No Error";
            return true;
        }

        public static bool RemoveItem(string file_path, out string error)
        {
            if (List != null)
            {
                if (Dictionary.ContainsKey(file_path.ToLower()))
                {
                    List.Remove(Dictionary[file_path.ToLower()]);
                    Dictionary.Remove(file_path.ToLower());
                    ListHasChanged = true;
                    error = "No Error";
                    return true;
                }
                else
                {
                    error = "App doesn't exist in the list";
                    return false;
                }
            }
            error = "List is empty";
            return false;
        }

        static public bool SaveFile(string path, out string error)
        {
            Stream stream = null;
            try
            {
                if (File.Exists(path))
                    stream = File.Open(path, FileMode.Truncate);
                else
                    stream = File.Open(path, FileMode.OpenOrCreate);
                BinaryWriter sw = new BinaryWriter(stream);
                sw.BaseStream.WriteByte(20);
                sw.BaseStream.WriteByte(21);
                sw.BaseStream.WriteByte(14);
                sw.BaseStream.WriteByte(1);
                sw.BaseStream.WriteByte(38);
                sw.BaseStream.WriteByte(77);
                sw.BaseStream.WriteByte(65);
                BinaryFormatter ii = new BinaryFormatter();
                ii.Serialize(stream, Dictionary);
                stream.Close();
                Properties.Settings.Default.Save();
                if (path.ToLower() == (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\applist").ToLower())
                    ListHasChanged = false;
                error = "No error";
                return true;
            }
            catch (Exception ex)
            {
                if (stream != null)
                    stream.Close();
                error = ex.Message;
                return false;
            }
        }

        static public bool LoadFile(string path, out string error)
        {
            string save_error;
            if (ListHasChanged)
                SaveFile((Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\applist"), out save_error);
            if (File.Exists(path))
            {
                //create stream
                Stream stream;
                try
                { stream = File.OpenRead(path); }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }

                //check file and version
                byte[] file_header;
                try
                {
                    if (stream.Length > 7)
                    {
                        file_header = new byte[7];
                        for (int i = 0; i < 7; i++)
                            file_header[i] = (byte)stream.ReadByte();
                    }
                    else
                        throw new Exception("Invalid file.");
                }
                catch (Exception ex)
                {
                    stream.Close();
                    error = ex.Message;
                    return false;
                }

                if (file_header[0] == 12) //version 1
                {
                    if (file_header[1] == 21 && file_header[2] == 14 && file_header[3] == 1 && file_header[4] == 38 && file_header[5] == 77 && file_header[6] == 65)
                    {
                        List<AppElements> tmp;
                        try
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            tmp = (List<AppElements>)bf.Deserialize(stream);
                            stream.Close();
                        }
                        catch (Exception ex)
                        {
                            stream.Close();
                            error = ex.Message;
                            return false;
                        }
                        string err;
                        List = new List<AppItem>();
                        Dictionary = new Dictionary<string, AppItem>();
                        frequancy = 1;
                        foreach (var i in tmp)
                            AddItem(i.app_path, i.app_name, (byte)i.priority, out err);


                        if (path.ToLower() == (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\applist").ToLower())
                        {
                            //save as new version
                            for (int i = 0; i < 3 && !SaveFile(path, out err); i++) ;
                        }

                        error = "No error";
                        return true;
                    }
                }
                else if (file_header[0] == 20) //version 2
                {
                    if (file_header[1] == 21 && file_header[2] == 14 && file_header[3] == 1 && file_header[4] == 38 && file_header[5] == 77 && file_header[6] == 65)
                    {
                        try
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            Dictionary = (Dictionary<string, AppItem>)bf.Deserialize(stream);
                            List = new List<AppItem>();
                            stream.Close();
                            frequancy = 1;
                            foreach (var i in Dictionary)
                            {
                                List.Add(i.Value);
                                if (i.Value.UsedTimes > frequancy)
                                    frequancy = i.Value.UsedTimes;
                            }
                            error = "No error";
                            return true;
                        }
                        catch (Exception ex)
                        {
                            stream.Close();
                            error = ex.Message;
                            return false;
                        }
                    }
                }
                stream.Close();
            }
            error = "Invalid file.";
            return false;
        }

        static public bool SaveWindowsApps(Dictionary<string, string> win_app_list, FileInfo output_file)
        {
            try
            {
                if (!output_file.Exists)
                {
                    if (!Directory.Exists(output_file.DirectoryName))
                        Directory.CreateDirectory(output_file.DirectoryName);
                    output_file.Create().Close();
                }
                StreamWriter sw = new StreamWriter(output_file.Open(FileMode.Truncate));
                foreach (var i in win_app_list)
                    sw.WriteLine(i.Value);
                sw.Close();
                return true;
            }
            catch (Exception) { return false; }
        }

        static public void LoadWindowsApps(Dictionary<string, string> win_app_list, FileInfo file)
        {
            bool invalid_file = false;
            if (file.Exists)
            {
                win_app_list.Clear();
                try
                {
                    StreamReader sw = new StreamReader(file.Open(FileMode.Open));
                    string tmp, name;
                    if (sw.BaseStream.Length == 0)
                        invalid_file = true;
                    while (!sw.EndOfStream)
                    {
                        tmp = sw.ReadLine();
                        if (File.Exists(tmp))
                        {
                            name = Path.GetFileNameWithoutExtension(tmp);
                            if (!win_app_list.ContainsKey(name.ToLower()))
                                win_app_list.Add(name.ToLower(), tmp);
                        }
                    }
                    sw.Close();
                }
                catch (Exception) { invalid_file = true; }
            }
            if (!file.Exists || invalid_file)
            {
                win_app_list.Clear(); //Clear the List if "File.Exist" stop for some reason;
                try
                {
                    string name_tmp;
                    string[] win_apps;
                    win_apps = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "*.exe");
                    foreach (var i in win_apps)
                        if (!win_app_list.ContainsKey(name_tmp = Path.GetFileNameWithoutExtension(i).ToLower()))
                            win_app_list.Add(name_tmp, i);
                }
                catch (Exception) { }

                try
                {
                    string name_tmp;
                    string[] sys_apps;
                    sys_apps = Directory.GetFiles(Environment.SystemDirectory, "*.exe");
                    foreach (var i in sys_apps)
                        if (!win_app_list.ContainsKey(name_tmp = Path.GetFileNameWithoutExtension(i).ToLower()))
                            win_app_list.Add(name_tmp, i);
                }
                catch (Exception) { }
                SaveWindowsApps(win_app_list, file);
            }

        }

        internal static void ReBuildList(Dictionary<string, AppItem> new_apps)
        {
            ListHasChanged = true;
            Dictionary.Clear();
            List.Clear();
            foreach (var i in new_apps)
            {
                Dictionary.Add(i.Value.Location.ToLower(), i.Value);
                List.Add(i.Value);
            }
        }
    }

    //legacy classes
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
}
