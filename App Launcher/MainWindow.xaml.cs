using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace App_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<int> n = new List<int>();
        List<Key> Keys;
        Key[] keys = { Key.Oem3, Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P, Key.OemOpenBrackets, Key.Oem6, Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.Oem1, Key.OemQuotes, Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M, Key.OemComma, Key.OemPeriod, Key.OemQuestion };
        string[] AR = { "ذ", "ض", "ص", "ث", "ق", "ف", "غ", "ع", "ه", "خ", "ح", "ج", "د", "ش", "س", "ي", "ب", "ل", "ا", "ت", "ن", "م", "ك", "ط", "ئ", "ء", "ؤ","ر", "لا", "ى", "ة", "و", "ز", "ظ", "ّ", "َ", "ً", "ُ", "ٌ", "لإ", "إ", "‘", "÷", "×", "؛", "<", ">", "ِ", "ٍ", "]", "[", "لأ", "أ", "ـ", "،", ":", "\"", "~", "ْ", "}", "{", "لآ", "آ", "’", ",", ".", "؟" };
        string[] EN = { "`", "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "[", "]", "a", "s", "d", "f", "g", "h", "j", "k", "l", ";", "'", "z", "x", "c", "v", "b", "n", "m", ",", ".", "/", "~", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "{", "}", "A", "S", "D", "F", "G", "H", "J", "K", "L", ":", "\"", "Z", "X", "C", "V", "B", "N", "M", "<", ">", "?" };
        List<AppElements> new_apps = new List<AppElements>();
        List<string> windows_apps = new List<string>();
        BackgroundWorker bw1;
        bool fake = true;
        bool inAction = false;
        bool auto = false;
        bool cancel_hide;
        public MainWindow()
        {
            //((Window)Process.GetProcesses()[0].MainWindowHandle).Show() ;
            InitializeComponent();
        }

        //        private object myJumpList_JumpItemsRejected(object sender,JumpItemsRejectedEventArgs e)
        //        {
        //                                             handle myJumpList.JumpItemsRejected
        //        foreach (var i  in e.RejectedItems)
        //            Debug.WriteLine(CType(i, JumpTask).Title);
        //        }

        //private Sub myJumpList_JumpItemsRemovedByUser(ByVal sender As Object, ByVal e As  _                                                 System.Windows.Shell.JumpItemsRemovedEventArgs)
        //    {
        //                                                  Handles myJumpList.JumpItemsRemovedByUser
        //        foreach( element In e.RemovedItems
        //            Debug.WriteLine(CType(element, JumpTask).Title)
        //        Next
        //        Debug.WriteLine(e.RemovedItems.Count)
        //        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ShowHide_Click(object sender, RoutedEventArgs e)
        {
            if (WindowStyle == WindowStyle.None)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ((MenuItem)sender).IsChecked = false;
                Properties.Settings.Default.HideTitlebar = false;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                ((MenuItem)sender).IsChecked = true;
                Properties.Settings.Default.HideTitlebar = true;
            }
            Properties.Settings.Default.Save();
        }

        private void QuickAdd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Applications|*.exe";
            ofd.Title = "Select an application to add";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int compleated = 0;
                string not_complete = "";
                foreach (string i in ofd.FileNames)
                    if (new FileInfo(i).Extension.ToLower().Equals(".exe"))
                    {
                        AppElements app = new AppElements(i.Substring(0, i.Length - 4), i, 0);
                        if (!app.ExistedIn(OrginalList.orginal_list, -1))
                        {
                            ListViewItem item = new ListViewItem();
                            item.Content = app.app_name;
                            item.ToolTip = app.app_path;
                            app_list.Items.Add(item);
                            OrginalList.orginal_list.Add(app);
                            compleated++;
                        }
                        else
                            not_complete = i;
                    }
                if (compleated < ofd.FileNames.Length)
                    MessageBox.Show((ofd.FileNames.Length - compleated == 1 ? "This application \"" + not_complete + "\"" : "Some applications") + " is already existe in the list.", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                string ex;
                if (!OrginalList.SaveFile(out ex))
                    MessageBox.Show(ex, "App Launcher", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            OrginalList.orginal_list = new EditApp().Show_Dialog();
            string ex;
            if (!OrginalList.SaveFile(out ex))
                MessageBox.Show(ex, "App Launcher", MessageBoxButton.OK, MessageBoxImage.Stop);
            RebuildList(search.Text);
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBlock tmp;
            if (((TextBox)sender) == search)
                tmp = search_box_label;
            else
                tmp = run_label;
            if (((TextBox)sender).Text.Equals(string.Empty))
                tmp.Visibility = Visibility.Visible;
            else
                tmp.Visibility = Visibility.Collapsed;
            if (((TextBox)sender) == search)
                RebuildList(search.Text);
            else
                AutoCompleate(run.Text);
        }

        private void AutoCompleate(string p)
        {
            if (!auto)
            {
                auto = true;
                int i;
                if (p.Length > 1 && !p.Contains('\\') && !p.Contains(':'))
                    foreach (var pro in windows_apps)
                    {
                        if (pro.Length < p.Length)
                            continue;
                        for (i = 0; i < p.Length; i++)
                            if (pro[i] != p[i])
                                break;
                        if (i == p.Length)
                        {
                            run.Text = pro;
                            //run.CaretIndex = p.Length;
                            run.Select(p.Length, pro.Length - p.Length);
                            break;
                        }
                    }
                auto = false;
            }
        }

        private void RebuildList(string p)
        {
            app_list.Items.Clear();
            new_apps.Clear();
            foreach (AppElements i in OrginalList.orginal_list)
                if (i.app_name.ToLower().Contains(p.ToLower()) || i.app_path.ToLower().Contains(p.ToLower()))
                    new_apps.Add(i);
            new_apps.Sort();
            foreach (AppElements i in new_apps)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = i.app_name;
                item.ToolTip = i.app_path;
                item.TabIndex = i.priority;
                item.ContextMenu = new ContextMenu();
                item.ContextMenuOpening += CreateContextMenu;
                item.MouseDoubleClick += Item_MouseDoubleClick;
                app_list.Items.Insert(0, item);
            }
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = app_list.Items.IndexOf((ListBoxItem)sender);
            try
            {
                Process.Start(new_apps[app_list.Items.Count - 1 - index].app_path);
                new_apps[app_list.Items.Count - 1 - index].last_used = DateTime.Now;
                new_apps[app_list.Items.Count - 1 - index].used_times++;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, "App Launcher", MessageBoxButton.OK, MessageBoxImage.Stop); }
        }

        private void CreateContextMenu(object sender, ContextMenuEventArgs e)
        {
            ContextMenu app_context_menu = ((ListBoxItem)sender).ContextMenu;
            if (app_context_menu.Items.Count == 0)
            {
                app_context_menu.Tag = ((ListBoxItem)sender).Content.ToString();
                app_context_menu.TabIndex = ((ListBoxItem)sender).TabIndex;
                app_context_menu.ItemStringFormat = ((ListBoxItem)sender).ToolTip.ToString();
                MenuItem menu_item = new MenuItem();
                menu_item.Header = "Start";
                menu_item.Icon = new Image();
                try
                {
                    if (File.Exists(((ListBoxItem)sender).ToolTip.ToString()))
                        ((Image)menu_item.Icon).Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.Icon.ExtractAssociatedIcon(((ListBoxItem)sender).ToolTip.ToString()).Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    else
                        ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/App Launcher;component/Images/no_icon.png"));
                }
                catch (Exception) { ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/App Launcher;component/Images/no_icon.png")); }
                menu_item.Click += Open_Click;
                app_context_menu.Items.Add(menu_item);
                menu_item = new MenuItem();
                menu_item.Header = "Run as administartor";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/App Launcher;component/Images/run-as-adimstrator.png"));
                menu_item.Click += RunAsAdmin_Click;
                app_context_menu.Items.Add(menu_item);
                app_context_menu.Items.Add(new Separator());
                menu_item = new MenuItem();
                menu_item.Header = "Edit this application";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/App Launcher;component/Images/edit-app.png"));
                menu_item.Click += EditApp_Click;
                app_context_menu.Items.Add(menu_item);
                menu_item = new MenuItem();
                menu_item.Header = "Delete";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/App Launcher;component/Images/delete-app.png"));
                menu_item.Click += DeleteApp_Click;
                app_context_menu.Items.Add(menu_item);
                app_context_menu.Items.Add(new Separator());
                menu_item = new MenuItem();
                menu_item.Header = "Open directory";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/App Launcher;component/Images/directory.png"));
                menu_item.Click += OpenDirectory_Click;
                app_context_menu.Items.Add(menu_item);
                menu_item = new MenuItem();
                menu_item.Header = "Copy file path";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/App Launcher;component/Images/path.png"));
                menu_item.Click += CopyFilePath_Click;
                app_context_menu.Items.Add(menu_item);
                menu_item = new MenuItem();
                menu_item.Header = "Copy directory path";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/App Launcher;component/Images/directory_path.png"));
                menu_item.Click += CopyDirectoryPath_Click;
                app_context_menu.Items.Add(menu_item);
            }
        }

        private void DeleteApp_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to delete this app?", "App Launcher", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int index = 0;
                ContextMenu cont = (ContextMenu)((MenuItem)sender).Parent;
                while (!cont.ItemStringFormat.Equals(OrginalList.orginal_list[index].app_path) && index < OrginalList.orginal_list.Count)
                    index++;
                OrginalList.orginal_list.RemoveAt(index);
                RebuildList(search.Text);
            }
        }

        private void CopyDirectoryPath_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(new FileInfo(((ContextMenu)((MenuItem)sender).Parent).ItemStringFormat).Directory.FullName);
        }

        private void CopyFilePath_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(new FileInfo(((ContextMenu)((MenuItem)sender).Parent).ItemStringFormat).FullName);
        }

        private void EditApp_Click(object sender, RoutedEventArgs e)
        {
            bool OK;
            ContextMenu cont = (ContextMenu)((MenuItem)sender).Parent;
            AppElements app = new AddAPP().Show_Dialog(new AppElements(cont.Tag.ToString(), cont.ItemStringFormat, cont.TabIndex), out OK);
            if (OK)
            {
                int index = 0;
                while (!cont.ItemStringFormat.Equals(OrginalList.orginal_list[index].app_path) && index < OrginalList.orginal_list.Count)
                    index++;
                int new_index = 0;
                while (!cont.ItemStringFormat.Equals(new_apps[new_index].app_path) && new_index < OrginalList.orginal_list.Count)
                    new_index++;
                new_index = new_apps.Count - new_index;
                if (!app.ExistedIn(OrginalList.orginal_list, index))
                {
                    OrginalList.orginal_list[index].app_name = app.app_name;
                    OrginalList.orginal_list[index].app_path = app.app_path;
                    OrginalList.orginal_list[index].priority = app.priority;
                    ((ListBoxItem)app_list.SelectedItem).ContextMenu = new ContextMenu();
                    RebuildList(search.Text);
                    while (new_index < 0)
                        new_index++;
                    while (new_index > app_list.Items.Count - 1)
                        new_index--;
                    app_list.ScrollIntoView(app_list.Items[new_index]);
                }
                else
                    MessageBox.Show("This application is already existe in the list", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void OpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            { Process.Start(new FileInfo(((ContextMenu)((MenuItem)sender).Parent).ItemStringFormat).Directory.FullName); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void RunAsAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = -1;
                for (int i = 0; i < app_list.Items.Count; i++)
                    if (((ListBoxItem)app_list.Items[i]).ContextMenu == (ContextMenu)((MenuItem)sender).Parent)
                    {
                        index = i;
                        break;
                    }
                ProcessStartInfo ps = new ProcessStartInfo(new_apps[app_list.Items.Count - index - 1].app_path);
                ps.Verb = "runas";
                Process.Start(ps);
                new_apps[app_list.Items.Count - index - 1].last_used = DateTime.Now;
                new_apps[app_list.Items.Count - index - 1].used_times++;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, "App Launcher", MessageBoxButton.OK, MessageBoxImage.Stop); }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = -1;
                for (int i = 0; i < app_list.Items.Count; i++)
                    if (((ListBoxItem)app_list.Items[i]).ContextMenu == (ContextMenu)((MenuItem)sender).Parent)
                    {
                        index = i;
                        break;
                    }
                Process.Start(new_apps[app_list.Items.Count - index - 1].app_path);
                new_apps[app_list.Items.Count - index - 1].last_used = DateTime.Now;
                new_apps[app_list.Items.Count - index - 1].used_times++;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, "App Launcher", MessageBoxButton.OK, MessageBoxImage.Stop); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keys = keys.ToList<Key>();
            DirectoryInfo system = new DirectoryInfo(Environment.SystemDirectory);
            DirectoryInfo windows = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            FileInfo[] win_pro = windows.GetFiles("*.exe");
            FileInfo[] sys_pro = system.GetFiles("*.exe");
            foreach (var i in win_pro)
                windows_apps.Add(i.Name.Remove(i.Name.Length - 4));
            foreach (var i in sys_pro)
                windows_apps.Add(i.Name.Remove(i.Name.Length - 4));
            Width = Properties.Settings.Default.Width;
            Height = Properties.Settings.Default.Height;
            Left = Properties.Settings.Default.Xpos;
            Top = Properties.Settings.Default.Ypos;
            if (Properties.Settings.Default.HideTitlebar)
            {
                WindowStyle = WindowStyle.None;
                show.IsChecked = true;
            }
            else
                show.IsChecked = false;
            OrginalList.orginal_list = new List<AppElements>();
            try
            {
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\App Launcher"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\App Launcher");
                if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\App Launcher\\applist"))
                    File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\App Launcher\\applist");
                else
                {
                    Stream stream = File.OpenRead(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\App Launcher\\applist");
                    if (stream.ReadByte() == 12 && stream.ReadByte() == 21 && stream.ReadByte() == 14 && stream.ReadByte() == 1 && stream.ReadByte() == 38 && stream.ReadByte() == 77 && stream.ReadByte() == 65)
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        OrginalList.orginal_list = (List<AppElements>)bf.Deserialize(stream);
                        stream.Close();
                        RebuildList(search.Text);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Something happened\n\r" + ex, "App Launcher", MessageBoxButton.OK, MessageBoxImage.Stop); }
            search.Focus();
            fake = false;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string ex;
            if (!OrginalList.SaveFile(out ex))
                MessageBox.Show(ex, "Error while saving application list", MessageBoxButton.OK, MessageBoxImage.Stop);
            //e.Cancel = true;
            WindowState = WindowState.Minimized;

            //Hide();
        }

        private void search_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (app_list.Items.Count > 0)
                {
                    try
                    {
                        Process.Start(new_apps[new_apps.Count - 1].app_path);
                        new_apps[new_apps.Count - 1].last_used = DateTime.Now;
                        new_apps[new_apps.Count - 1].used_times++;
                    }
                    catch (Exception ex)
                    { MessageBox.Show(ex.Message, "App Launcher", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
            }
            if (search.Text.Equals(string.Empty))
                n.Clear();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (!fake)
            {
                Properties.Settings.Default.Xpos = Left;
                Properties.Settings.Default.Ypos = Top;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!fake)
            {
                Properties.Settings.Default.Width = Width;
                Properties.Settings.Default.Height = Height;
            }
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            List<AppElements> deleted = new List<AppElements>();
            foreach (var i in OrginalList.orginal_list)
                if (!File.Exists(i.app_path))
                    deleted.Add(i);
            if (deleted.Count > 0)
            {
                StringBuilder st = new StringBuilder("These app has been deleted:\n\r");
                foreach (AppElements i in deleted)
                {
                    OrginalList.orginal_list.Remove(i);
                    st.Append(i.app_path);
                    st.Append("\n\r");
                }
                RebuildList(search.Text);
                string ex;
                OrginalList.SaveFile(out ex);
                MessageBox.Show(st.ToString(), "App Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Done", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Run_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (app_list.Items.Count > 0)
                {
                    try
                    { Process.Start(run.Text); }
                    catch (Exception ex) { MessageBox.Show(ex.Message, "App Launcher", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
            }

        }

        private void Browser_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Applications|*.exe";
            ofd.Title = "Select an application to run";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK && new FileInfo(ofd.FileName).Extension.ToLower().Equals(".exe"))
            {
                run.Text = ofd.FileName;
                try
                { Process.Start(run.Text); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "App Launcher", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            search.Clear();
            n.Clear();
        }

        private void search_PreviewKeydown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.A && e.Key <= Key.Z)
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    if (Keyboard.IsKeyToggled(Key.CapsLock))
                        n.Add(Keys.IndexOf(e.Key));
                    else
                        n.Add(Keys.IndexOf(e.Key) + 34);
                else
                    if (Keyboard.IsKeyToggled(Key.CapsLock))
                        n.Add(Keys.IndexOf(e.Key) + 34);
                    else
                        n.Add(Keys.IndexOf(e.Key));
            else if (e.Key >= Key.Oem3 || e.Key >= Key.OemOpenBrackets || e.Key >= Key.Oem6 || e.Key >= Key.Oem1 || e.Key >= Key.OemQuotes || e.Key >= Key.OemComma || e.Key >= Key.OemPeriod || e.Key >= Key.OemQuestion)
                n.Add(Keys.IndexOf(e.Key));
            else if (e.Key.Equals(Key.Back) && n.Count > 0 && search.Text.Length > 1)
                n.RemoveAt(search.CaretIndex < search.Text.Length ? search.CaretIndex : search.Text.Length - 1 > n.Count ? 0 : search.Text.Length - 1);
            else if (e.Key.Equals(Key.Delete) && n.Count > 1 && search.CaretIndex > 1 && search.Text.Length > 1)
                n.RemoveAt(search.CaretIndex < search.Text.Length ? search.CaretIndex - 1 : search.Text.Length - 1 > n.Count ? 0 : search.Text.Length - 1);
        }

        private void SwicthLanguage_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder st_ar = new StringBuilder();
            StringBuilder st_en = new StringBuilder();
            foreach (int i in n)
            {
                st_ar.Append(AR[i]);
                st_en.Append(EN[i]);
            }
            if (st_ar.ToString().Equals(search.Text))
                search.Text = (st_en.ToString());
            else if (st_en.ToString().Equals(search.Text))
                search.Text = (st_ar.ToString());
            else
            {
                List<string> ar_list=AR.ToList<string>();
                List<string> en_list=EN.ToList<string>();
                string search_text = search.Text;
                List<int> search_index = new List<int>();
                StringBuilder new_search_text=new StringBuilder();
                foreach (char i in search_text)
                {
                    int ar_index=ar_list.IndexOf(i.ToString());
                    int en_index = -1;
                    if (ar_index > -1)
                        new_search_text.Append(EN[ar_index]);
                    else
                    {
                        en_index = en_list.IndexOf(i.ToString());
                        new_search_text.Append(AR[en_index]);
                    }
                }
                search.Text = new_search_text.ToString();
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            cancel_hide = false;
            bw1 = new BackgroundWorker();
            bw1.DoWork += bw1_DoWork;
            bw1.WorkerSupportsCancellation = true;
            bw1.WorkerReportsProgress = true;
            bw1.ProgressChanged += bw1_ProgressChanged;
            bw1.RunWorkerAsync();
        }

        private void bw1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (Left > -10 && !inAction)
            {
                inAction = true;
                //DoubleAnimation animation = new DoubleAnimation((-1) * ActualWidth, new Duration(new TimeSpan(1500000))); //arabic Layout
                DoubleAnimation animation = new DoubleAnimation(SystemParameters.PrimaryScreenWidth, new Duration(new TimeSpan(1500000))); //english
                BeginAnimation(LeftProperty, animation);
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += bw_DoWork;
                bw.RunWorkerCompleted += bw_RunWorkerCompleted;
                bw.RunWorkerAsync();
            }
        }

        private void bw1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 50; i++)
            {
                Thread.Sleep(20);
                if (cancel_hide)
                    break;
            }
            if (!cancel_hide)
                bw1.ReportProgress(10);
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //DoubleAnimation animation = new DoubleAnimation(Left + 15, new Duration(new TimeSpan(750000))); //arabic Layout
            DoubleAnimation animation = new DoubleAnimation(Left - 15, new Duration(new TimeSpan(750000))); //english Layout
            BeginAnimation(LeftProperty, animation);
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw2_DoWork;
            bw.RunWorkerAsync(150);
        }

        private void bw2_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep((int)e.Argument);
            inAction = false;
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(200);
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (bw1 != null && bw1.IsBusy)
                bw1.CancelAsync();
            if (!inAction)
            {
                //if(Left<-4) arabic
                if (Left < SystemParameters.PrimaryScreenWidth - 5)
                {
                    Focus();
                    inAction = true;
                    //DoubleAnimation animation = new DoubleAnimation(-5, new Duration(new TimeSpan(2500000))); //arabic
                    DoubleAnimation animation = new DoubleAnimation(SystemParameters.PrimaryScreenWidth - ActualWidth + 5, new Duration(new TimeSpan(2500000))); //english
                    BeginAnimation(LeftProperty, animation);
                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += bw2_DoWork;
                    bw.RunWorkerAsync(350);
                }
                cancel_hide = true;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (Left > SystemParameters.PrimaryScreenWidth - ActualWidth && !inAction)
            //if (Left > -10 && !inAction)  arabic
            {
                inAction = true;
                //DoubleAnimation animation = new DoubleAnimation((-1) * ActualWidth, new Duration(new TimeSpan(1500000))); //arabic Layout
                DoubleAnimation animation = new DoubleAnimation(SystemParameters.PrimaryScreenWidth, new Duration(new TimeSpan(1500000))); //english
                BeginAnimation(LeftProperty, animation);
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += bw_DoWork;
                bw.RunWorkerCompleted += bw_RunWorkerCompleted;
                bw.RunWorkerAsync();
            }
        }
    }
}