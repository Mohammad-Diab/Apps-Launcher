using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace App_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<AppElements> new_apps = new List<AppElements>();
        List<string> windows_apps = new List<string>();
        bool fake = true;
        bool auto = false;
        public MainWindow()
        {
            InitializeComponent();
        }

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
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK && new FileInfo(ofd.FileName).Extension.ToLower().Equals(".exe"))
            {
                AppElements app = new AppElements(ofd.SafeFileName.Substring(0, ofd.SafeFileName.Length - 4), ofd.FileName, 0);
                if (!app.ExistedIn(OrginalList.orginal_list, -1))
                {
                    ListViewItem item = new ListViewItem();
                    item.Content = app.app_name;
                    item.ToolTip = app.app_path;
                    app_list.Items.Add(item);
                    OrginalList.orginal_list.Add(app);
                    string ex;
                    if (!OrginalList.SaveFile(out ex))
                        MessageBox.Show(ex, "App Launcher", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else
                    MessageBox.Show("This application is already existe in the list", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Asterisk);
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
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

        private void run_PreviewKeyUp(object sender, KeyEventArgs e)
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
    }
}