using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Apps_Launcher
{
    /// <summary>
    /// Interaction logic for AddAPP.xaml
    /// </summary>
    public partial class AddAPP : Window
    {
        internal bool ok = false;
        public AddAPP()
        {
            InitializeComponent();
        }

        private void OKBtm_Click(object sender, RoutedEventArgs e)
        {
            if (!location.Text.Equals(string.Empty))
            {
                ok = true;
                Close();
            }
            else
                MessageBox.Show("Please select application first.", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void CancelBtm_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BrowseBtm_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Applications|*.exe";
            ofd.Title = "Select an application";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (new FileInfo(ofd.FileName).Extension.ToLower().Equals(".exe"))
                {
                    name.Text = Path.GetFileNameWithoutExtension(ofd.FileName);
                    location.Text = ofd.FileName;
                    image.Source = Applications.GetImage(ofd.FileName, 48);
                }
            }
        }
    }

    public static class Applications
    {
        static internal bool AddApplication(out string error)
        {
            AddAPP window = new AddAPP();
            window.ShowDialog();
            if (window.ok)
            {
                if (AppList.Dictionary.ContainsKey(window.location.Text.ToLower()))
                {
                    error = "App is already exist in the list.";
                    return false;
                }
                else
                    return AppList.AddItem(window.location.Text, window.name.Text, Convert.ToByte(window.priority.Value), out error);
            }
            error = "";
            return true;
        }

        static internal void EditApplication(AppItem item)
        {
            AddAPP window = new AddAPP();
            window.OK_button.Content = "Save";
            window.Title = "Edit " + item.AppName;
            window.name.Text = item.AppName;
            window.location.Text = item.Location;
            window.priority.Value = item.Priority;
            window.image.Source = GetImage(item.Location, 48);
            window.ShowDialog();
            if (window.ok)
            {
                item.AppName = window.name.Text;
                item.ChangeLocation(window.location.Text);
                item.Priority = Convert.ToByte(window.priority.Value);
                AppList.ListHasChanged = true;
            }
        }

        static internal AppItem GetApplication()
        {
            AddAPP window = new AddAPP();
            window.ShowDialog();
            if (window.ok)
                return new AppItem(window.name.Text, window.location.Text, Convert.ToByte(window.priority.Value));
            return null;
        }

        public static BitmapSource GetImage(string file_path, int width)
        {
            BitmapSource bitmap_source = null;
            bool exist = false;
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\icons_cashe\\" + (file_path.GetHashCode() + 2147483648) + ".png"))
            {
                try
                {
                    bitmap_source = new BitmapImage(new Uri(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\icons_cashe\\" + (file_path.GetHashCode() + 2147483648) + ".png"));
                    exist = true;
                }
                catch (Exception) { }
            }
            else
            {
                try
                { bitmap_source = Imaging.CreateBitmapSourceFromHIcon(Icon.ExtractAssociatedIcon(file_path).Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(48, 48)); }
                catch (Exception)
                {
                    try { bitmap_source = Imaging.CreateBitmapSourceFromHIcon(Icon.ExtractAssociatedIcon(file_path).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); }
                    catch (Exception) { }
                }
                if (!exist && bitmap_source != null)
                {
                    try
                    {
                        if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\icons_cashe\\"))
                        {
                            try { Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\icons_cashe\\"); }
                            catch (Exception) { }
                        }
                        using (FileStream b = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\icons_cashe\\" + (file_path.GetHashCode() + 2147483648) + ".png", FileMode.OpenOrCreate))
                        {
                            PngBitmapEncoder n = new PngBitmapEncoder();
                            n.Frames.Add(BitmapFrame.Create(bitmap_source));
                            n.Save(b);
                        }
                    }
                    catch (Exception) { }
                }
            }
            if (bitmap_source == null)
                bitmap_source = new BitmapImage(new Uri("pack://application:,,,/Apps Launcher;component/Images/applicaton.png"));
            return bitmap_source;
        }
    }
}
