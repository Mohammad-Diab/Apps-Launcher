using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.ComponentModel;
using System.Threading;

namespace Apps_Launcher
{
    /// <summary>
    /// Interaction logic for EditApp.xaml
    /// </summary>
    public partial class EditApp : Window
    {
        public static void EditList()
        {
            EditApp window = new EditApp();
            window.ShowDialog();
        }

        bool stop_workers;
        Dictionary<string, AppItem> new_apps = new Dictionary<string, AppItem>();
        Dictionary<string, ListBoxItem> new_apps_for_list = new Dictionary<string, ListBoxItem>();
        Thickness name_thickness = new Thickness(85, 5, 100, 0);
        Thickness path_thickness = new Thickness(115, 28, 100, 0);
        Thickness ch_thickness = new Thickness(10, 5, 0, 0);
        Thickness img_thickness = new Thickness(31, 0, 0, 5);
        Thickness rectangle_thickness = new Thickness(10, 0, 10, 1);
        Thickness scroll_bar_thickness = new Thickness(0, 13, 10, 0);
        Thickness priority_thickness = new Thickness(0, 20, 32, 0);

        public EditApp()
        {
            InitializeComponent();
        }

        #region Load Window

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (object sender2, DoWorkEventArgs args) =>
            {
                for (int i = 0; i < AppList.List.Count; i++)
                {
                    if (stop_workers)
                        break;
                    bw.ReportProgress(i);
                }
            };
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.ProgressChanged += (object sender2, ProgressChangedEventArgs args) =>
            {
                new_apps.Add(AppList.List[args.ProgressPercentage].Location.ToLower(), AppList.List[args.ProgressPercentage]);
                var item = CreateItem(AppList.List[args.ProgressPercentage], false);
                new_apps_for_list.Add(AppList.List[args.ProgressPercentage].Location.ToLower(), item);
                app_list.Items.Add(item);
                TaskbarItemInfo.ProgressValue += (1.0 / AppList.List.Count);
            };
            bw.RunWorkerCompleted += (object sender2, RunWorkerCompletedEventArgs args) =>
            {
                if (!stop_workers)
                {

                    BackgroundWorker set_images = new BackgroundWorker();
                    set_images.WorkerReportsProgress = true;
                    set_images.DoWork += (object sender3, DoWorkEventArgs args3) =>
                    {
                        for (int i = 0; i < AppList.List.Count; i++)
                        {
                            Thread.Sleep(5);
                            if (stop_workers)
                                return;
                            set_images.ReportProgress(i);
                        }
                    };

                    set_images.ProgressChanged += (object sender3, ProgressChangedEventArgs args2) =>
                    {
                        (((app_list.Items[args2.ProgressPercentage] as ListBoxItem).Content as Grid).Children[0] as Image).Source = Applications.GetImage(AppList.List[args2.ProgressPercentage].Location, 48);
                    };
                    set_images.RunWorkerAsync();

                    loading_tb.Visibility = Visibility.Collapsed;
                    app_list.Visibility = Visibility.Visible;
                    Count.Text = app_list.Items.Count + (app_list.Items.Count == 1 ? " item" : " items");
                }
            };
            bw.RunWorkerAsync();
        }

        private ListBoxItem CreateItem(AppItem app, bool get_image)
        {
            Image img = new Image();
            img.Height = img.Width = 48;
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.Margin = img_thickness;
            if (get_image)
                img.Source = Applications.GetImage(app.Location, 48);
            else
                img.Source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/applicaton.png"));

            TextBox name = new TextBox();
            name.Text = app.AppName;
            name.FontSize = 16;
            name.VerticalAlignment = VerticalAlignment.Top;
            name.Foreground = Brushes.Crimson;
            name.Background = Brushes.Transparent;
            name.Margin = name_thickness;
            name.BorderBrush = null;
            name.TextChanged += (object sender, TextChangedEventArgs args) => 
            {
                app.AppName = name.Text;
            };

            var rec = new System.Windows.Shapes.Rectangle();
            rec.Margin = rectangle_thickness;
            rec.Fill = Brushes.Black;
            rec.Height = 1.3;
            rec.VerticalAlignment = VerticalAlignment.Bottom;

            CheckBox ch = new CheckBox();
            ch.HorizontalAlignment = HorizontalAlignment.Left;
            ch.VerticalAlignment = VerticalAlignment.Top;
            ch.Margin = ch_thickness;
            ch.IsChecked = false;

            TextBlock path = new TextBlock();
            path.Text = app.Location;
            path.HorizontalAlignment = HorizontalAlignment.Left;
            path.VerticalAlignment = VerticalAlignment.Top;
            path.Foreground = Brushes.Green;
            path.Margin = path_thickness;
            path.TextTrimming = TextTrimming.CharacterEllipsis;

            ScrollBar scroll = new ScrollBar();
            scroll.HorizontalAlignment = HorizontalAlignment.Right;
            scroll.VerticalAlignment = VerticalAlignment.Top;
            scroll.Height = 30;
            scroll.Width = 15;
            scroll.Margin = scroll_bar_thickness;
            scroll.Value = 9 - app.Priority;
            scroll.Maximum = 9;
            scroll.Minimum = 0;
            scroll.SmallChange = 1;

            Run run1 = new Run("Priority: ");
            Run run2 = new Run(app.Priority.ToString());
            run2.Foreground = Brushes.CadetBlue;

            scroll.Scroll += (object sender, ScrollEventArgs e) =>
            {
                app.Priority = (byte)Math.Round(9 - e.NewValue);
                run2.Text = app.Priority.ToString();
            };

            TextBlock priority = new TextBlock();
            priority.HorizontalAlignment = HorizontalAlignment.Right;
            priority.VerticalAlignment = VerticalAlignment.Top;
            priority.Margin = priority_thickness;
            priority.Inlines.Add(run1);
            priority.Inlines.Add(run2);


            Grid grid = new Grid();
            grid.Width = app_list.ActualWidth - 32;
            if (!File.Exists(app.Location))
            {
                grid.IsEnabled = false;
                grid.Opacity = 0.7;
                grid.Background = Brushes.Coral;
            }
            grid.Children.Add(img);
            grid.Children.Add(name);
            grid.Children.Add(path);
            grid.Children.Add(ch);
            grid.Children.Add(priority);
            grid.Children.Add(scroll);
            grid.Children.Add(rec);

            ListViewItem item = new ListViewItem();
            item.ContentStringFormat = app.Location;
            item.Tag = app;
            item.Content = grid;

            return item;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            stop_workers = true;
        }

        #endregion

        #region Main Menu Function

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var item = Applications.GetApplication();
            if (item != null)
            {
                filter_tb.Text = "";
                if (new_apps.ContainsKey(item.Location.ToLower()))
                    MessageBox.Show("App already exist in the list.", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                {
                    new_apps.Add(item.Location.ToLower(), item);
                    app_list.Items.Add(CreateItem(item, true));
                }
            }
            Count.Text = app_list.Items.Count + (app_list.Items.Count == 1 ? " item" : " items");
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxItem i in app_list.Items)
                ((CheckBox)((Grid)i.Content).Children[3]).IsChecked = true;
        }

        private void CheckNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxItem i in app_list.Items)
                ((CheckBox)((Grid)i.Content).Children[3]).IsChecked = false;
        }

        private void Invert_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxItem i in app_list.Items)
                ((CheckBox)((Grid)i.Content).Children[3]).IsChecked = !((CheckBox)((Grid)i.Content).Children[3]).IsChecked.Value;
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            List<string> to_delete = new List<string>();
            foreach (ListBoxItem i in app_list.SelectedItems)
                to_delete.Add(i.ContentStringFormat.ToLower());
            foreach (var i in to_delete)
            {
                if (app_list.Items.Contains(new_apps_for_list[i]))
                    app_list.Items.Remove(new_apps_for_list[i]);
                new_apps.Remove(i);
                new_apps_for_list.Remove(i);
            }
            Count.Text = app_list.Items.Count + (app_list.Items.Count == 1 ? " item" : " items");
        }

        private void DeleteChecked_Click(object sender, RoutedEventArgs e)
        {
            List<string> to_delete = new List<string>();
            for (int i = app_list.Items.Count - 1; i > -1; i--)
                if (((CheckBox)((Grid)((ListBoxItem)app_list.Items[i]).Content).Children[3]).IsChecked == true)
                    to_delete.Add(((ListBoxItem)app_list.Items[i]).ContentStringFormat.ToLower());
            foreach (var i in to_delete)
            {
                if (app_list.Items.Contains(new_apps_for_list[i]))
                    app_list.Items.Remove(new_apps_for_list[i]);
                new_apps.Remove(i);
                new_apps_for_list.Remove(i);
            }
            Count.Text = app_list.Items.Count + (app_list.Items.Count == 1 ? " item" : " items");
        }

        private void AddFrom_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Select directory to search in it for application:";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo dir = new DirectoryInfo(fbd.SelectedPath);
                List<AppItem> exe_files = new List<AppItem>();
                GetEXEFiles(dir, exe_files);
                if (exe_files.Count > 0)
                {
                    List<AppItem> items = new AddFrom().Show_Dialog(exe_files);
                    if (items.Count > 0)
                    {
                        int added = 0;
                        foreach (var i in items)
                        {
                            if (!new_apps.ContainsKey(i.Location.ToLower()))
                            {
                                added++;
                                var app = CreateItem(i, true);
                                new_apps.Add(i.Location.ToLower(), i);
                                new_apps_for_list.Add(i.Location.ToLower(), app);
                                app_list.Items.Insert(0, app);
                            }
                        }
                        if (added == 0)
                            MessageBox.Show("No apps have been added.", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        else if (added < items.Count)
                            MessageBox.Show("Some applications is already existed in list and they have been skiped.", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        else
                            MessageBox.Show("All applications have been added sccessfully.", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                    MessageBox.Show("No applications in this location.", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            Count.Text = app_list.Items.Count + (app_list.Items.Count == 1 ? " item" : " items");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            app_list.Visibility = Visibility.Collapsed;
            loading_tb.Visibility = Visibility.Visible;
            loading_tb.Text = "Saving...";
            BackgroundWorker timer = new BackgroundWorker();
            timer.DoWork += (object sender2, DoWorkEventArgs args) => { Thread.Sleep(500); };
            timer.RunWorkerCompleted += (object sender2, RunWorkerCompletedEventArgs args) => 
            {
                AppList.ReBuildList(new_apps);
                Close();
            };
            timer.RunWorkerAsync();
        }

        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        private void GetEXEFiles(DirectoryInfo dir, List<AppItem> exe_files)
        {
            try
            {
                FileInfo[] files = dir.GetFiles();
                foreach (var i in files)
                    if (i.Extension.ToLower().Equals(".exe"))
                        exe_files.Add(new AppItem(Path.GetFileNameWithoutExtension(i.FullName), i.FullName, 4));
            }
            catch (Exception) { }
            try
            {
                DirectoryInfo[] sub = dir.GetDirectories();
                foreach (var i in sub)
                    GetEXEFiles(i, exe_files);
            }
            catch (Exception) { }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (ListBoxItem i in app_list.Items)
                ((Grid)i.Content).Width = app_list.ActualWidth - 32;
        }

        private void Filter_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (filter_tb.Text != "")
                filter_placeholder.Visibility = Visibility.Collapsed;
            else
                filter_placeholder.Visibility = Visibility.Visible;
            app_list.Items.Clear();
            foreach (var i in new_apps_for_list)
                if (((AppItem)i.Value.Tag).AppName.ToLower().Contains(filter_tb.Text.ToLower()))
                    app_list.Items.Add(i.Value);
        }

        private void SortListByName_Click(object sender, RoutedEventArgs e)
        {
            Sorter(SortBy.Name);
        }

        private void SortListByPath_Click(object sender, RoutedEventArgs e)
        {
            Sorter(SortBy.Path);
        }

        private void SortListByPriority_Click(object sender, RoutedEventArgs e)
        {
            Sorter(SortBy.Priority);
        }

        void Sorter(SortBy by)
        {
            List<ListBoxItem> tmp = new List<ListBoxItem>();
            foreach (ListBoxItem i in app_list.Items)
                tmp.Add(i);
            tmp.Sort((ListBoxItem x, ListBoxItem y) =>
            {
                switch (by)
                {
                    case SortBy.Name:
                        return ((AppItem)x.Tag).AppName.CompareTo(((AppItem)y.Tag).AppName);
                    case SortBy.Path:
                        return x.ContentStringFormat.ToLower().CompareTo(y.ContentStringFormat.ToLower());
                    case SortBy.Priority:
                        return (((ScrollBar)(((Grid)x.Content).Children[5])).Value).CompareTo(((ScrollBar)(((Grid)y.Content).Children[5])).Value);
                }
                return 0;
            });
            app_list.Items.Clear();
            foreach (ListBoxItem i in tmp)
                app_list.Items.Add(i);
        }

        private void AppList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                List<string> new_apps_list = new List<string>();
                foreach (string i in (string[])e.Data.GetData(DataFormats.FileDrop))
                    if (File.Exists(i) && Path.GetExtension(i).ToLower().Equals(".exe"))
                        new_apps_list.Add(i);
                if (new_apps_list.Count == 0)
                    MessageBox.Show("No apps in droped files", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                {
                    foreach (var i in new_apps_list)
                    {
                        if (!new_apps.ContainsKey(i))
                        {
                            var tmp = new AppItem(Path.GetFileNameWithoutExtension(i), i, 4);
                            new_apps.Add(i, tmp);
                            app_list.Items.Add(CreateItem(tmp, true));
                        }
                    }
                    MessageBox.Show("Done", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
    enum SortBy
    {
        Name = 1,
        Path = 2,
        Priority = 3
    }
}