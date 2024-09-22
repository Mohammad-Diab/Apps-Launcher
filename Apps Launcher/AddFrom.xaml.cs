using System;
using System.Collections.Generic;
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

namespace Apps_Launcher
{
    /// <summary>
    /// Interaction logic for AddFrom.xaml
    /// </summary>
    public partial class AddFrom : Window
    {
        List<AppItem> items = new List<AppItem>();
        List<AppItem> orginal = new List<AppItem>();

        Thickness name_thickness = new Thickness(85, 5, 100, 0);
        Thickness path_thickness = new Thickness(115, 28, 100, 0);
        Thickness ch_thickness = new Thickness(10, 5, 0, 0);
        Thickness img_thickness = new Thickness(31, 0, 0, 5);
        Thickness rectangle_thickness = new Thickness(10, 0, 10, 1);
        Thickness scroll_bar_thickness = new Thickness(0, 13, 10, 0);
        Thickness priority_thickness = new Thickness(0, 20, 32, 0);
        int checked_count = 0;

        bool accept = false;
        public AddFrom()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var i in orginal)
                app_list.Items.Add(CreateList(i, true));
            total_apps.Text = "total: " + app_list.Items.Count + (app_list.Items.Count == 1 ? " item " : " items ");
            checked_count = app_list.Items.Count;
            checked_apps.Text = checked_count + (checked_count == 1 ? " item " : " items ") + "checked, ";
        }

        internal List<AppItem> Show_Dialog(List<AppItem> exe_files)
        {
            foreach (var i in exe_files)
                orginal.Add(i);
            ShowDialog();
            if (accept)
            {
                foreach (ListBoxItem i in app_list.Items)
                {
                    if (((CheckBox)i.Content).IsChecked == true)
                    {
                        byte pr;
                        byte.TryParse(((Run)((TextBlock)((Grid)((CheckBox)i.Content).Content).Children[3]).Inlines.FirstInline.NextInline).Text, out pr);
                        items.Add(new AppItem(((TextBox)((Grid)((CheckBox)i.Content).Content).Children[1]).Text, ((TextBox)((Grid)((CheckBox)i.Content).Content).Children[2]).Text, pr));
                    }
                }
            }
            return items;
        }

        private ListBoxItem CreateList(AppItem app, bool cheacked)
        {
            Image img = new Image();
            img.Height = img.Width = 48;
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.Margin = img_thickness;
            img.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.Icon.ExtractAssociatedIcon(app.Location).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            TextBox name = new TextBox();
            name.Text = app.AppName;
            name.FontSize = 16;
            name.Background = Brushes.Transparent;
            name.VerticalAlignment = VerticalAlignment.Top;
            name.Foreground = Brushes.Crimson;
            name.Margin = name_thickness;
            name.TextWrapping = TextWrapping.NoWrap;
            name.Height = 21;
            name.BorderBrush = Brushes.Transparent;

            Rectangle rec = new Rectangle();
            rec.Margin = rectangle_thickness;
            rec.Fill = Brushes.Black;
            rec.Height = 1.3;
            rec.VerticalAlignment = VerticalAlignment.Bottom;

            TextBox path = new TextBox();
            path.IsReadOnly = true;
            path.Text = app.Location;
            path.HorizontalAlignment = HorizontalAlignment.Left;
            path.VerticalAlignment = VerticalAlignment.Top;
            path.Foreground = Brushes.Green;
            path.Background = Brushes.Transparent;
            path.BorderBrush = Brushes.Transparent;
            path.Margin = path_thickness;
            path.TextWrapping = TextWrapping.NoWrap;

            Run run1 = new Run("Priority: ");
            Run run2 = new Run(app.Priority.ToString());
            run2.Foreground = Brushes.CadetBlue;

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
            scroll.Scroll += (object sender, ScrollEventArgs e) => { run2.Text = (9 - e.NewValue).ToString(); };

            TextBlock priority = new TextBlock();
            priority.HorizontalAlignment = HorizontalAlignment.Right;
            priority.VerticalAlignment = VerticalAlignment.Top;
            priority.Margin = priority_thickness;
            priority.Inlines.Add(run1);
            priority.Inlines.Add(run2);


            Grid grid = new Grid();
            grid.Children.Add(img);
            grid.Children.Add(name);
            grid.Children.Add(path);
            grid.Children.Add(priority);
            grid.Children.Add(scroll);
            grid.Children.Add(rec);

            CheckBox ch = new CheckBox();
            ch.Width = app_list.ActualWidth - 40;
            ch.HorizontalAlignment = HorizontalAlignment.Left;
            ch.VerticalAlignment = VerticalAlignment.Top;
            ch.Margin = ch_thickness;
            ch.IsChecked = cheacked;
            ch.Content = grid;

            ch.Checked += (object sender, RoutedEventArgs args) =>
            {
                checked_count++;
                checked_apps.Text = checked_count + (checked_count == 1 ? " item " : " items ") + "checked, ";
            };

            ch.Unchecked += (object sender, RoutedEventArgs args) =>
            {
                checked_count--;
                checked_apps.Text = checked_count + (checked_count == 1 ? " item " : " items ") + "checked, ";
            };

            ListViewItem item = new ListViewItem();
            item.ContentStringFormat = app.Location;
            item.ToolTip = app.Location;
            item.Content = ch;

            return item;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (ListBoxItem i in app_list.Items)
            {
                ((CheckBox)i.Content).Width = app_list.ActualWidth - 40;
                ((Grid)((CheckBox)i.Content).Content).Width = app_list.ActualWidth - 55;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            accept = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxItem i in app_list.Items)
                ((CheckBox)((Grid)i.Content).Children[3]).IsChecked = true;
        }

        private void UncheckAll_Click(object sender, RoutedEventArgs e)
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
    }
}
