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

namespace App_Launcher
{
    /// <summary>
    /// Interaction logic for AddFrom.xaml
    /// </summary>
    public partial class AddFrom : Window
    {
        List<AppElements> items = new List<AppElements>();
        List<AppElements> orginal = new List<AppElements>();
        bool accept = false;
        public AddFrom()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var i in orginal)
                app_list.Items.Add(CreateList(i, true));
        }

        internal List<AppElements> Show_Dialog(List<AppElements> exe_files)
        {
            foreach (var i in exe_files)
                orginal.Add(i);
            ShowDialog();
            if (accept)
            {
                foreach (ListBoxItem i in app_list.Items)
                {
                    if (((CheckBox)((Grid)i.Content).Children[3]).IsChecked == true)
                    {
                        int pr;
                        int.TryParse(((Run)((TextBlock)((Grid)i.Content).Children[4]).Inlines.FirstInline.NextInline).Text,out pr);
                        items.Add(new AppElements(((TextBox)((Grid)i.Content).Children[1]).Text, ((TextBlock)((Grid)i.Content).Children[2]).Text, pr));
                    }
                }
            }
            return items;
        }

        private ListBoxItem CreateList(AppElements app, bool cheacked)
        {
            Image img = new Image();
            img.Height = img.Width = 48;
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.Margin = OrginalList.img_thickness;
            img.Source  = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.Icon.ExtractAssociatedIcon(app.app_path).Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

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
            grid.Width = app_list.ActualWidth - 25;
            grid.Children.Add(img);
            grid.Children.Add(name);
            grid.Children.Add(path);
            grid.Children.Add(ch);
            grid.Children.Add(priority);
            grid.Children.Add(scroll);
            grid.Children.Add(rec);

            ListViewItem item = new ListViewItem();
            item.ContentStringFormat = app.app_path;
            item.ToolTip = app.app_path;
            item.Content = grid;

            return item;
        }

        private void scroll_Scroll(object sender, ScrollEventArgs e)
        {
            ((Run)((TextBlock)(((Grid)((ScrollBar)sender).Parent).Children[4])).Inlines.FirstInline.NextInline).Text = (9 - e.NewValue).ToString();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (ListBoxItem i in app_list.Items)
                ((Grid)i.Content).Width = app_list.ActualWidth - 25;
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
