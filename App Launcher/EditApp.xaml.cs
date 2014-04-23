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
using System.Windows.Shapes;
using System.Windows.Interop;
using System.ComponentModel;

namespace App_Launcher
{
    /// <summary>
    /// Interaction logic for EditApp.xaml
    /// </summary>
    public partial class EditApp : Window
    {
        bool save = false;
        static public BitmapSource[] images;
        List<AppElements> new_apps = new List<AppElements>();
        public EditApp()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AppElements app = new AddAPP().Show_Dialog();
            if (!app.app_path.Equals(string.Empty))
                if (!app.ExistedIn(new_apps, -1))
                {
                    new_apps.Add(app);
                    app_list.Items.Insert(0, CreateList(-2, app, false));
                }
                else
                    MessageBox.Show("This application is already existe in the list", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private ListBoxItem CreateList(int index, AppElements app, bool cheacked)
        {
            index = -1;
            Image img = new Image();
            img.Height = img.Width = 48;
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.Margin = OrginalList.img_thickness;

            TextBox name = new TextBox();
            name.Text = app.app_name;
            name.FontSize = 16;
            name.VerticalAlignment = VerticalAlignment.Top;
            name.Foreground = System.Windows.Media.Brushes.Crimson;
            name.Margin = OrginalList.name_thickness;
            name.TextWrapping = TextWrapping.NoWrap;
            name.Height = 21;
            name.BorderBrush = null;

            Rectangle rec = new Rectangle();
            rec.Margin = OrginalList.rectangle_thickness;
            rec.Fill = Brushes.Black;
            rec.Height = 1.3;
            rec.VerticalAlignment = VerticalAlignment.Bottom;

            CheckBox ch = new CheckBox();
            ch.HorizontalAlignment = HorizontalAlignment.Left;
            ch.VerticalAlignment = VerticalAlignment.Top;
            ch.Margin = OrginalList.ch_thickness;
            ch.IsChecked = cheacked;

            TextBlock path = new TextBlock();
            path.Text = app.app_path;
            path.HorizontalAlignment = HorizontalAlignment.Left;
            path.VerticalAlignment = VerticalAlignment.Top;
            path.Foreground = Brushes.Green;
            path.Margin = OrginalList.path_thickness;
            path.TextTrimming = TextTrimming.CharacterEllipsis;

            ScrollBar scroll = new ScrollBar();
            scroll.HorizontalAlignment = HorizontalAlignment.Right;
            scroll.VerticalAlignment = VerticalAlignment.Top;
            scroll.Height = 30;
            scroll.Width = 15;
            scroll.Margin = OrginalList.scroll_bar_thickness;
            scroll.Value = 9 - app.priority;
            scroll.Maximum = 9;
            scroll.Minimum = 0;
            scroll.SmallChange = 1;
            scroll.Scroll += scroll_Scroll;

            Run run1 = new Run("Priority: ");
            Run run2 = new Run(app.priority.ToString());
            run2.Foreground = Brushes.CadetBlue;

            TextBlock priority = new TextBlock();
            priority.HorizontalAlignment = HorizontalAlignment.Right;
            priority.VerticalAlignment = VerticalAlignment.Top;
            priority.Margin = OrginalList.priority_thickness;
            priority.Inlines.Add(run1);
            priority.Inlines.Add(run2);


            Grid grid = new Grid();
            //if (index < 0)
                try
                { img.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.Icon.ExtractAssociatedIcon(app.app_path).Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()); }
                catch (Exception)
                {
                    grid.Opacity = 0.7;
                    grid.Background = Brushes.Coral;
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/App Launcher;component/Images/deleted.png"));
                }
            //else
            //    if (images[index] != null)
            //        img.Source = images[index];
            //    else
            //    {
            //        grid.Opacity = 0.7;
            //        grid.Background = Brushes.Coral;
            //        img.Source = new BitmapImage(new Uri("pack://application:,,,/App Launcher;component/Images/deleted.png"));
            //    }
            grid.Width = app_list.ActualWidth - 25;
            grid.Children.Add(img);
            grid.Children.Add(name);
            grid.Children.Add(path);
            grid.Children.Add(ch);
            grid.Children.Add(priority);
            grid.Children.Add(scroll);
            grid.Children.Add(rec);

            ListViewItem item = new ListViewItem();
            //item.ContextMenuOpening += File_ContextMenuCreator;
            item.Width = app_list.ActualWidth - 10;
            item.ContentStringFormat = app.app_path;
            //item.ContextMenu = new ContextMenu();
            item.Content = grid;

            return item;
        }

        private void scroll_Scroll(object sender, ScrollEventArgs e)
        {
            ((Run)((TextBlock)(((Grid)((ScrollBar)sender).Parent).Children[4])).Inlines.FirstInline.NextInline).Text = (9 - e.NewValue).ToString();
        }

        private void File_ContextMenuCreator(object sender, ContextMenuEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            List<int> index = new List<int>();
            for (int i = 0; i < app_list.SelectedItems.Count; i++)
                for (int j = 0; j < app_list.Items.Count; j++)
                    if (app_list.SelectedItems[i] == app_list.Items[j])
                    {
                        index.Add(j);
                        break;
                    }
            for (int i = index.Count - 1; i > -1; i--)
            {
                app_list.Items.RemoveAt(i);
                new_apps.RemoveAt(i);
            }
        }

        private void DeleteChecked_Click(object sender, RoutedEventArgs e)
        {
            for (int i = app_list.Items.Count - 1; i > -1; i--)
                if (((CheckBox)((Grid)((ListBoxItem)app_list.Items[i]).Content).Children[3]).IsChecked == true)
                {
                    app_list.Items.RemoveAt(i);
                    new_apps.RemoveAt(i);
                }
        }

        private void AddFrom_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Select directory to search in it for application:";
            if (fbd.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK))
            {
                DirectoryInfo dir = new DirectoryInfo(fbd.SelectedPath);
                List<AppElements> exe_files = new List<AppElements>();
                GetEXEFiles(dir, exe_files);
                if (exe_files.Count > 0)
                {
                    List<AppElements> items = new AddFrom().Show_Dialog(exe_files);
                    if (items.Count > 0)
                    {
                        int added = 0;
                        foreach (var i in items)
                        {
                            if (!i.ExistedIn(new_apps, -1))
                            {
                                added++;
                                app_list.Items.Insert(0, CreateList(-7, i, false));
                                new_apps.Add(i);
                            }
                        }
                        if (added == 0)
                            MessageBox.Show("No apps have been added.", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        else if (added < items.Count)
                            MessageBox.Show("Some applications is already existed in list and they have been skiped.", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        else
                            MessageBox.Show("All applications have been added sccessfully.", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                    MessageBox.Show("No applications in this location.", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void GetEXEFiles(DirectoryInfo dir, List<AppElements> exe_files)
        {
            try
            {
                FileInfo[] files = dir.GetFiles();
                foreach (var i in files)
                    if (i.Extension.ToLower().Equals(".exe"))
                        exe_files.Add(new AppElements(i.Name.Substring(0, i.Name.Length - 4), i.FullName, 0));
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
            {
                ((Grid)i.Content).Width = app_list.ActualWidth - 25;
                i.Width = app_list.ActualWidth - 10;
            }
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
                if (((CheckBox)((Grid)i.Content).Children[3]).IsChecked == false)
                    ((CheckBox)((Grid)i.Content).Children[3]).IsChecked = true;
                else
                    ((CheckBox)((Grid)i.Content).Children[3]).IsChecked = false;
        }

        internal List<AppElements> Show_Dialog()
        {
            ShowDialog();
            if (save)
            {
                new_apps.Clear();
                foreach (ListBoxItem i in app_list.Items)
                {
                    int pr;
                    int.TryParse(((Run)((TextBlock)((Grid)i.Content).Children[4]).Inlines.FirstInline.NextInline).Text, out pr);
                    new_apps.Add(new AppElements(((TextBox)((Grid)i.Content).Children[1]).Text, ((TextBlock)((Grid)i.Content).Children[2]).Text, pr));
                }
                return new_apps;
            }
            return OrginalList.orginal_list;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            save = true;
            Close();
        }

        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            images = new BitmapSource[OrginalList.orginal_list.Count];
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync(/*images*/);
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            processbar.Value = TaskbarItemInfo.ProgressValue = 0;
            for (int i = 0; i < OrginalList.orginal_list.Count; i++)
            {
                app_list.Items.Add(CreateList(i, OrginalList.orginal_list[i], false));
                new_apps.Add(OrginalList.orginal_list[i]);
                processbar.Visibility = Visibility.Collapsed;
                IsHitTestVisible = true;
                TaskbarItemInfo.ProgressValue += 1.0 / OrginalList.orginal_list.Count;
                processbar.Value += 100.0 / OrginalList.orginal_list.Count;
            }
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //processbar.Value = (e.ProgressPercentage * 50.0) / (OrginalList.orginal_list.Count + 0.0);
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //BitmapSource[] images = (BitmapSource[])e.Argument;
            //for (int i = 0; i < OrginalList.orginal_list.Count; i++)
            //{
            //    try
            //    { images[i] = (Imaging.CreateBitmapSourceFromHIcon(System.Drawing.Icon.ExtractAssociatedIcon(OrginalList.orginal_list[i].app_path).Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions())); }
            //    catch (Exception)
            //    {
            //        images[i] = null;
            //    }
            //    ((BackgroundWorker)sender).ReportProgress(i);
            //}
        }
    }
}