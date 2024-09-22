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
using System.Windows.Shell;

namespace Apps_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool app_is_running, starting_window = true, close_window = false, in_action = false;
        Dictionary<string, string> windows_apps;
        long hide_after = long.MaxValue;
        System.Windows.Forms.NotifyIcon ni;
        public MainWindow()
        {
            if (!AppList.SuccessfulStartUp)
            {
                Close();
                return;
            }

            InitializeComponent();
        }

        #region Hide and Show Application Window

        WindowVisibility state = WindowVisibility.Show;
        WindowVisibility State
        {
            get { return state; }
            set
            {
                state = value;
                switch (value)
                {
                    case WindowVisibility.Hide:
                        if (AppList.Layout == HideLayout.HideToRight)
                            MoveWindow(-ActualWidth, -ActualWidth + 20, true);
                        else
                            MoveWindow(SystemParameters.PrimaryScreenWidth, Left + ActualWidth - 20, true);
                        OpenMenu = false;
                        break;
                    case WindowVisibility.Show:
                        if (AppList.Layout == HideLayout.HideToRight)
                            MoveWindow(-5, 0, false);
                        else
                            MoveWindow(SystemParameters.PrimaryScreenWidth - ActualWidth + 5, 0, false);
                        if (WindowState == WindowState.Minimized)
                            WindowState = WindowState.Normal;
                        Activate();
                        break;
                    case WindowVisibility.Visible:
                        Show();
                        State = WindowVisibility.Show;
                        break;
                    case WindowVisibility.UnVisible:
                        State = WindowVisibility.Hide;
                        Hide();
                        break;
                }
            }
        }

        private void MoveWindow(double left1, double left2, bool two_stages)
        {
            in_action = true;
            try
            { BeginAnimation(LeftProperty, new DoubleAnimation(left1, new Duration(new TimeSpan(0, 0, 0, 0, 150)))); }
            catch (Exception) { }
            if (two_stages)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += (object sender, DoWorkEventArgs args) => { Thread.Sleep(200); };
                bw.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs args) =>
                {
                    if (!close_window)
                        try { BeginAnimation(LeftProperty, new DoubleAnimation(left2, new Duration(new TimeSpan(0, 0, 0, 0, 75)))); }
                        catch (Exception) { }
                };
                bw.RunWorkerAsync();
            }
            in_action = false;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            hide_after = 0;
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (State == WindowVisibility.Hide)
            {
                search.Focus();
                State = WindowVisibility.Show;
            }
            hide_after = long.MaxValue;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            hide_after = 3000;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (OpenMenu)
                OpenMenu = false;
            if (!starting_window && WindowState == WindowState.Normal && !in_action)
            {
                Properties.Settings.Default.Width = Width;
                Properties.Settings.Default.Height = Height;
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (!starting_window && !in_action)
            {
                Properties.Settings.Default.Xpos = Left;
                Properties.Settings.Default.Ypos = Top;
            }
        }

        #endregion

        #region MainMenu

        bool open_menu= false;
        bool OpenMenu
        {
            get { return open_menu; }
            set
            {
                if (!close_window)
                {
                    if (value)
                    {
                        main_menu.Margin = new Thickness(0, -100, 0, 0);
                        main_menu.Opacity = 0;
                        main_menu_grid.Visibility = Visibility.Visible;
                        DoubleAnimation da = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                        ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0), new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                        ThicknessAnimation ta1 = new ThicknessAnimation(new Thickness(0, -40, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 75)));
                        ta1.Completed += (object sender, EventArgs arg) =>
                        {
                            ThicknessAnimation ta2 = new ThicknessAnimation(new Thickness(11, -40, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 75)));
                            ta2.Completed += (object sender2, EventArgs arg2) =>
                            {
                                ThicknessAnimation ta3 = new ThicknessAnimation(new Thickness(22, -40, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 75)));
                                ta3.Completed += (object sender3, EventArgs arg3) =>
                                {
                                    DoubleAnimation da1 = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 60)));
                                    da1.Completed += (object sender4, EventArgs arg4) => { img_grid.Visibility = Visibility.Collapsed; };
                                    if (!close_window)
                                        img_grid.BeginAnimation(HeightProperty, da1);
                                    img3.Visibility = Visibility.Collapsed;
                                    img4.Visibility = Visibility.Visible;
                                };
                                if (!close_window)
                                    img3.BeginAnimation(MarginProperty, ta3);
                                img2.Visibility = Visibility.Collapsed;
                            };
                            if (!close_window)
                                img2.BeginAnimation(MarginProperty, ta2);
                            img1.Visibility = Visibility.Collapsed;
                        };
                        if (!close_window)
                        {
                            img1.BeginAnimation(MarginProperty, ta1);
                            main_menu.BeginAnimation(MarginProperty, ta);
                            main_menu.BeginAnimation(OpacityProperty, da);
                        }
                    }
                    else
                    {
                        DoubleAnimation da = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                        ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0, -100, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                        da.Completed += (object sender, EventArgs e) => { main_menu_grid.Visibility = Visibility.Collapsed; };
                        DoubleAnimation da1 = new DoubleAnimation(25, new Duration(new TimeSpan(0, 0, 0, 0, 60)));
                        da1.Completed += (object sender4, EventArgs arg4) =>
                        {
                            ThicknessAnimation ta3 = new ThicknessAnimation(new Thickness(22, 0, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 75)));
                            ta3.Completed += (object sender, EventArgs arg) =>
                            {
                                ThicknessAnimation ta2 = new ThicknessAnimation(new Thickness(11, 0, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 75)));
                                ta2.Completed += (object sender2, EventArgs arg2) =>
                                {
                                    ThicknessAnimation ta1 = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 75)));
                                    img1.Visibility = Visibility.Visible;
                                    img1.BeginAnimation(MarginProperty, ta1);
                                };
                                img2.Visibility = Visibility.Visible;
                                img2.BeginAnimation(MarginProperty, ta2);
                            };
                            img3.Visibility = Visibility.Visible;
                            img3.BeginAnimation(MarginProperty, ta3);
                        };
                        img_grid.Visibility = Visibility.Visible;
                        img_grid.BeginAnimation(HeightProperty, da1);
                        main_menu.BeginAnimation(MarginProperty, ta);
                        main_menu.BeginAnimation(OpacityProperty, da);
                    }
                }
                open_menu = value;
            }
        }

        private void MenuBtm_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu = !OpenMenu;
        }

        private void AddApplication_Click(object sender, RoutedEventArgs e)
        {
            string error;
            if (!Applications.AddApplication(out error))
                MessageBox.Show(error, "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
            RebuildList(search.Text);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu = false;
            EditApp.EditList();
            RebuildList(search.Text);
        }

        private void RestoreList_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu = false;
            var ofd = new System.Windows.Forms.OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string err;
                if (!AppList.LoadFile(ofd.FileName, out err))
                    MessageBox.Show(err, "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
                AppList.ListHasChanged = true;
                RebuildList(search.Text);
            }
        }

        private void BackupList_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu = false;
            var sfd = new System.Windows.Forms.SaveFileDialog();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string err;
                if (!AppList.SaveFile(sfd.FileName, out err))
                    MessageBox.Show(err, "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu = false;
            List<string> to_delete = new List<string>();
            for (int i = 0; i < AppList.List.Count; i++)
                if (!File.Exists(AppList.List[i].Location))
                    to_delete.Add(AppList.List[i].Location);
            if (to_delete.Count > 0)
            {
                StringBuilder st = new StringBuilder("These app has been deleted:\n");
                string err;
                foreach (string i in to_delete)
                {
                    if (AppList.RemoveItem(i, out err))
                        st.Append(i + "\n");
                }
                RebuildList(search.Text);
                //string ex;
                //OrginalList.SaveFile(out ex);
                MessageBox.Show(st.ToString(), "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Done", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu = false;
            new Options().ShowDialog();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu = false;
            new About().ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu = false;
            close_window = true;
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu = false;
            WindowState = WindowState.Minimized;
        }

        private void ShowHide_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu = false;
            if (WindowStyle == WindowStyle.None)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                showtitle_img.Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/hide-titlebar.png"));
                Properties.Settings.Default.HideTitlebar = false;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                showtitle_img.Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/hide-titlebar-tick.png"));
                Properties.Settings.Default.HideTitlebar = true;
            }
            Properties.Settings.Default.Save();
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenMenu = false;
        }

        #endregion

        #region Search Engine

        private void Search_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (app_list.Items.Count > 1)
                    StartApplication(((ListBoxItem)app_list.Items[1]).ToolTip.ToString(), false);
        }

        private List<AppItem> SortList(string search)
        {
            search = search.ToLower();
            List<AppItem> list = new List<AppItem>();
            if (AppList.List != null)
            {
                foreach (var i in AppList.List)
                    if (i.AppName.ToLower().Contains(search) || Path.GetDirectoryName(i.Location.ToLower()).Contains(search))
                        list.Add(i);
            }
            foreach (var i in list)
                i.CalcPoint(search);
            list.Sort();

            if (windows_apps.ContainsKey(search.ToLower()))
                list.Insert(0, new AppItem(search, windows_apps[search.ToLower()], 18) { point = 1000 });

            if (list.Count > 10)
                return list.GetRange(0, 10);
            else
                return list;
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search_word = search.Text;
            if (search_word == string.Empty)
            {
                searchbox_placeholder.Visibility = Visibility.Visible;
                clear_text.Visibility = Visibility.Collapsed;
            }
            else
            {
                searchbox_placeholder.Visibility = Visibility.Collapsed;
                clear_text.Visibility = Visibility.Visible;
            }

            if (search_word == "")
                ViewItems(SortList(""), false);
            else
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += (object obj, DoWorkEventArgs args) =>
                {
                    Thread.Sleep(500);
                };
                bw.RunWorkerCompleted += (object obj, RunWorkerCompletedEventArgs args) =>
                {
                    if (search_word == search.Text)
                        ViewItems(SortList(search.Text), search_word != "");
                };
                bw.RunWorkerAsync();
            }
        }

        private void RebuildList(string search_word)
        {
            ViewItems(SortList(search_word), search_word != "");
        }

        #endregion

        #region Apperance

        ListViewItem CreateHeader(string text)
        {
            ListViewItem it = new ListViewItem();
            it.IsEnabled = false;
            it.Content = text;
            return it;
        }

        ListViewItem CreateBigItem(string name, string info, string app_path)
        {
            TextBlock tb1 = new TextBlock();
            tb1.Text = name;
            tb1.Margin = new Thickness(69, 5, 0, 0);
            tb1.HorizontalAlignment = HorizontalAlignment.Left;
            tb1.VerticalAlignment = VerticalAlignment.Top;
            tb1.FontSize = 16;
            tb1.TextTrimming = TextTrimming.CharacterEllipsis;
            tb1.Foreground = Brushes.Black;

            TextBlock tb2 = new TextBlock();
            tb2.Text = info;
            tb2.Margin = new Thickness(69, 31, 0, 0);
            tb2.HorizontalAlignment = HorizontalAlignment.Left;
            tb2.VerticalAlignment = VerticalAlignment.Top;
            tb2.FontSize = 14;
            tb2.TextTrimming = TextTrimming.CharacterEllipsis;
            tb2.Foreground = Brushes.Gray;

            Image img = new Image();
            img.Source = Applications.GetImage(app_path, 64);
            img.Margin = new Thickness(8, 8, 0, 0);
            img.Width = 48;
            img.Height = 48;
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.VerticalAlignment = VerticalAlignment.Top;

            Grid gr = new Grid();
            gr.Height = 64;
            gr.Children.Add(img);
            gr.Children.Add(tb1);
            gr.Children.Add(tb2);

            ListViewItem it = new ListViewItem();
            it.Content = gr;
            it.ToolTip = app_path;
            it.ContextMenu = new ContextMenu();
            it.MouseDoubleClick += (object sender, MouseButtonEventArgs arg) => { StartApplication(app_path, false); };
            it.ContextMenuOpening += (object sender, ContextMenuEventArgs arg) => { CreateContextMenu((ListBoxItem)sender, app_path); };
            return it;
        }

        ListViewItem CreateSmallItem(string name, string app_path)
        {
            TextBlock tb1 = new TextBlock();
            tb1.Text = name;
            tb1.Margin = new Thickness(37, 5, 0, 0);
            tb1.HorizontalAlignment = HorizontalAlignment.Left;
            tb1.VerticalAlignment = VerticalAlignment.Top;
            tb1.FontSize = 16;
            tb1.TextTrimming = TextTrimming.CharacterEllipsis;
            tb1.Foreground = Brushes.Black;

            Image img = new Image();
            img.Source = Applications.GetImage(app_path, 32);
            img.Width = 32;
            img.HorizontalAlignment = HorizontalAlignment.Left;

            Grid gr = new Grid();
            gr.Height = 32;
            gr.Children.Add(img);
            gr.Children.Add(tb1);

            ListViewItem it = new ListViewItem();
            it.Content = gr;
            it.ToolTip = app_path;
            it.ContextMenu = new ContextMenu();
            it.MouseDoubleClick += (object sender, MouseButtonEventArgs arg) => { StartApplication(app_path, false); };
            it.ContextMenuOpening += (object sender, ContextMenuEventArgs arg) => { CreateContextMenu((ListBoxItem)sender, app_path); };
            return it;
        }

        private void ViewItems(List<AppItem> list, bool IsSearch)
        {
            app_list.Items.Clear();
            if (list.Count == 0)
            {
                app_list.Items.Add(CreateHeader(IsSearch ? "No APP match your search." : "There is no apps in the list."));
            }
            else
            {
                int i = 0;
                if (list[0].point > 650 | list.Count == 1)
                {
                    app_list.Items.Add(CreateHeader(IsSearch ? "Best match" : "Top application"));
                    app_list.Items.Add(CreateBigItem(list[0].AppName, list[0].Priority > 9 ? "Command" : "Application", list[0].Location));
                    i++;
                }
                if (list.Count > 1 && list[1].point > 650)
                {
                    app_list.Items.Add(CreateBigItem(list[1].AppName, "Application", list[1].Location));
                    i++;
                }
                if (i < list.Count)
                    app_list.Items.Add(CreateHeader(IsSearch ? "Results" : "Applications"));
                for (; i < list.Count; i++)
                    app_list.Items.Add(CreateSmallItem(list[i].AppName, list[i].Location));
            }
        }

        private void CreateContextMenu(ListBoxItem app, string app_path)
        {
            bool is_application = AppList.Dictionary.ContainsKey(app_path.ToLower());
            ContextMenu app_context_menu = (app).ContextMenu;
            if (app_context_menu.Items.Count == 0)
            {
                MenuItem menu_item = new MenuItem();
                menu_item.Header = "Start";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Source = Applications.GetImage(app_path, 16);
                ((Image)menu_item.Icon).Width = 16;
                menu_item.Click += (object sender, RoutedEventArgs e) => { StartApplication(app_path, false); };
                app_context_menu.Items.Add(menu_item);

                menu_item = new MenuItem();
                menu_item.Header = "Run as administartor";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Width = 16;
                ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/run-as-adimstrator.png"));
                menu_item.Click += (object sender, RoutedEventArgs e) => { StartApplication(app_path, true); };
                app_context_menu.Items.Add(menu_item);

                app_context_menu.Items.Add(new Separator());

                if (is_application)
                {
                    menu_item = new MenuItem();
                    menu_item.Header = "Edit this application";
                    menu_item.Icon = new Image();
                    ((Image)menu_item.Icon).Width = 16;
                    ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/edit-app.png"));
                    menu_item.Click += (object sender, RoutedEventArgs e) => { EditApplication(app_path); };
                    app_context_menu.Items.Add(menu_item);

                    menu_item = new MenuItem();
                    menu_item.Header = "Delete";
                    menu_item.Icon = new Image();
                    ((Image)menu_item.Icon).Width = 16;
                    ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/delete-app.png"));
                    menu_item.Click += (object sender, RoutedEventArgs e) => { DeleteApplication(app_path); };
                    app_context_menu.Items.Add(menu_item);
                }
                else
                {
                    menu_item = new MenuItem();
                    menu_item.Header = "Run command";
                    menu_item.Icon = new Image();
                    ((Image)menu_item.Icon).Width = 16;
                    //((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/delete-app.png"));
                    menu_item.Click += (object sender, RoutedEventArgs e) => 
                    {
                        string args;
                        if (ModrenMessageBox.ShowMessage(out args) == ModrenMessageResult.Ok)
                            StartApplication(app_path, false, args);
                    };
                    app_context_menu.Items.Add(menu_item);
                }

                app_context_menu.Items.Add(new Separator());

                menu_item = new MenuItem();
                menu_item.Header = "Open directory";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Width = 16;
                ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/directory.png"));
                menu_item.Click += (object sender, RoutedEventArgs e) => { OpenDirectory(app_path); };
                app_context_menu.Items.Add(menu_item);

                menu_item = new MenuItem();
                menu_item.Header = "Copy file path";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Width = 16;
                ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/path.png"));
                menu_item.Click += (object sender, RoutedEventArgs e) => { CopyPath(app_path, false); };
                app_context_menu.Items.Add(menu_item);

                menu_item = new MenuItem();
                menu_item.Header = "Copy directory path";
                menu_item.Icon = new Image();
                ((Image)menu_item.Icon).Width = 16;
                ((Image)menu_item.Icon).Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/directory_path.png"));
                menu_item.Click += (object sender, RoutedEventArgs e) => { CopyPath(app_path, true); };
                app_context_menu.Items.Add(menu_item);
            }
        }

        #endregion

        #region Functions

        private void StartApplication(string app_path, bool AsAdmin)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(app_path);
                if (AsAdmin) psi.Verb = "runas";
                Process p = new Process();
                p.StartInfo = psi;
                p.Start();
                if (AppList.Dictionary.ContainsKey(app_path.ToLower()))
                {
                    AppList.Dictionary[app_path.ToLower()].LastUsed = DateTime.Now;
                    AppList.Dictionary[app_path.ToLower()].UsedTimes++;
                    if (AppList.Dictionary[app_path.ToLower()].UsedTimes > AppList.frequancy)
                        AppList.frequancy = AppList.Dictionary[app_path.ToLower()].UsedTimes;
                    AppList.ListHasChanged = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while starting this app:\n" + ex.Message, "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartApplication(string app_path, bool AsAdmin, string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(app_path);
                psi.Arguments = args;
                if (AsAdmin) psi.Verb = "runas";
                Process p = new Process();
                p.StartInfo = psi;
                p.Start();
                if (AppList.Dictionary.ContainsKey(app_path.ToLower()))
                {
                    AppList.Dictionary[app_path.ToLower()].LastUsed = DateTime.Now;
                    AppList.Dictionary[app_path.ToLower()].UsedTimes++;
                    if (AppList.Dictionary[app_path.ToLower()].UsedTimes > AppList.frequancy)
                        AppList.frequancy = AppList.Dictionary[app_path.ToLower()].UsedTimes;
                    AppList.ListHasChanged = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while starting this app:\n" + ex.Message, "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditApplication(string app_path)
        {
            Applications.EditApplication(AppList.Dictionary[app_path.ToLower()]);
            RebuildList(search.Text);
        }

        private void DeleteApplication(string app_path)
        {
            if (MessageBox.Show("Do you want to delete this app?", "Apps Launcher", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string error;
                if (!AppList.RemoveItem(app_path, out error))
                    MessageBox.Show("Error while remove the item:\n" + error, "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
                RebuildList(search.Text);
            }
        }

        private void OpenDirectory(string app_path)
        {
            try
            { Process.Start(new FileInfo(app_path).Directory.FullName); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CopyPath(string app_path, bool directory)
        {
            if (directory)
                Clipboard.SetText(new FileInfo(app_path).Directory.FullName);
            else
                Clipboard.SetText(app_path);
        }

        private void ClearTextBtm_Click(object sender, RoutedEventArgs e)
        {
            search.Text = "";
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //make notifyIcon
            ni = new System.Windows.Forms.NotifyIcon();
            System.Windows.Forms.NotifyIcon icon = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Apps Launcher;component/Images/small-icon.ico")).Stream);
            ni.Visible = true;
            ni.ContextMenu = new System.Windows.Forms.ContextMenu();

            ni.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Show", (object sender2, EventArgs args) => { State = WindowVisibility.Visible; }));
            ni.ContextMenu.MenuItems[0].DefaultItem = true;
            ni.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Exit", (object sender2, EventArgs args) => { close_window = true; Close(); }));
            
            ni.Click += (object sender2, EventArgs args) => { State = WindowVisibility.Visible; };

            //read apps list
            string err;
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\applist"))
            {
                if (new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\applist").Length > 7)
                {
                    while (!AppList.LoadFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\applist", out err))
                        if (MessageBox.Show("error while loading file:\n" + err + ".\nTry again?", "Apps Launcher", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            AppList.Dictionary = new Dictionary<string, AppItem>();
                            AppList.List = new List<AppItem>();
                            break;
                        }
                }
                else
                {
                    AppList.Dictionary = new Dictionary<string, AppItem>();
                    AppList.List = new List<AppItem>();
                    AppList.SaveFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\applist", out err);
                }
            }
            else
            {
                try
                {
                    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\"))
                        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\");
                }
                catch (Exception) { }
                AppList.Dictionary = new Dictionary<string, AppItem>();
                AppList.List = new List<AppItem>();
                AppList.SaveFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\applist", out err);
            }

            //Background saving worker
            BackgroundWorker SaverWorker = new BackgroundWorker();
            SaverWorker.WorkerReportsProgress = true;
            SaverWorker.DoWork += (object sender2, DoWorkEventArgs args) =>
            {
                while (app_is_running)
                {
                    bool end_loop = false;
                    for (int i = 0; i < 600; i++)
                    {
                        Thread.Sleep(500);
                        if (!app_is_running)
                        {
                            end_loop = true;
                            break;
                        }
                        if (end_loop)
                            break;
                    }
                    if (!end_loop)
                        SaverWorker.ReportProgress(0);
                };
            };
            SaverWorker.ProgressChanged += (object sender2, ProgressChangedEventArgs args) =>
            {
                string error;
                if (app_is_running && AppList.ListHasChanged)
                    AppList.SaveFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\applist", out error);
            };
            app_is_running = true;
            SaverWorker.RunWorkerAsync();

            //get windows applications
            windows_apps = new Dictionary<string, string>();
            AppList.LoadWindowsApps(windows_apps, new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\winapps"));

            //load setting

            if (Properties.Settings.Default.HideTitlebar)
            {
                WindowStyle = WindowStyle.None;
                showtitle_img.Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/hide-titlebar-tick.png"));
            }
            else
                showtitle_img.Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/hide-titlebar.png"));

            if (Properties.Settings.Default.HideToRight)
                AppList.Layout = HideLayout.HideToRight;

            in_action = true;
            Width = Properties.Settings.Default.Width;
            Height = Properties.Settings.Default.Height;
            Left = Properties.Settings.Default.Xpos;
            Top = Properties.Settings.Default.Ypos;
            in_action = false;

            //hide and show window worker
            BackgroundWorker Timer_BW = new BackgroundWorker();
            Timer_BW.WorkerReportsProgress = true;
            Timer_BW.DoWork += (object sender2, DoWorkEventArgs args) =>
            {
                while (!close_window)
                {
                    if (hide_after > 0)
                        hide_after -= 100;
                    else
                    {
                        Timer_BW.ReportProgress(0);
                        hide_after = long.MaxValue;
                    }
                    Thread.Sleep(100);
                }
            };
            Timer_BW.ProgressChanged += (object sender2, ProgressChangedEventArgs args) =>
            {
                if (!close_window && State == WindowVisibility.Show)
                    State = WindowVisibility.Hide;
            };
            Timer_BW.RunWorkerAsync();

            //View top application
            ViewItems(SortList(""), false);

            starting_window = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (AppList.ListHasChanged)
            {
                string err;
                while (!AppList.SaveFile((Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\applist"), out err))
                {
                    var result = MessageBox.Show("error while saving file:\n" + err + ".\nTry again?", "Apps Launcher", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        break;
                    }
                    if (result == MessageBoxResult.No)
                        break;
                }
            }
            if (!close_window)
            {
                e.Cancel = true;
                State = WindowVisibility.UnVisible;
            }
            else
            {
                app_is_running = false;
                AppList.SaveWindowsApps(windows_apps, new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\winapps"));
                if (ni != null)
                    ni.Visible = false;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           DragMove();
        }

        private void AppList_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (app_list.SelectedIndex > 0)
                    StartApplication((app_list.SelectedItem as ListBoxItem).ToolTip.ToString(), false);
        }

        private void AppList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                List<string> f = new List<string>();
                string err;
                foreach (string i in (string[])e.Data.GetData(DataFormats.FileDrop))
                    if (File.Exists(i) && Path.GetExtension(i).ToLower().Equals(".exe"))
                        f.Add(i);
                if (f.Count == 0)
                    MessageBox.Show("No apps in droped files", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                {
                    foreach (var i in f)
                        AppList.AddItem(i, Path.GetFileNameWithoutExtension(i), 4, out err);
                    RebuildList(search.Text);
                }
            }
        }

    }
}