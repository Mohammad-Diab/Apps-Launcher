using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace App_Launcher
{
    /// <summary>
    /// Interaction logic for AddAPP.xaml
    /// </summary>
    public partial class AddAPP : Window
    {
        bool ok = false;
        public AddAPP()
        {
            InitializeComponent();
        }

        internal AppElements Show_Dialog()
        {
            ShowDialog();
            AppElements app = new AppElements("", "", 0);
            if (ok)
            {
                app.app_name = name.Text;
                app.app_path = location.Text;
                int.TryParse(priority.Text, out app.priority);
            }
            return app;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!location.Text.Equals(string.Empty))
            {
                ok = true;
                Close();
            }
            else
                System.Windows.MessageBox.Show("Please select application first.", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Applications|*.exe";
            ofd.Title = "Select an application";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (new FileInfo(ofd.FileName).Extension.ToLower().Equals(".exe"))
                {
                    name.Text = ofd.SafeFileName.Substring(0, ofd.SafeFileName.Length - 4);
                    location.Text = ofd.FileName;
                }
            }
        }

        internal AppElements Show_Dialog(AppElements old, out bool x)
        {
            Title = "Edit " + old.app_name;
            name.Text = old.app_name;
            location.Text = old.app_path;
            priority.Text = old.priority.ToString();
            ShowDialog();
            x = false;
            AppElements app = new AppElements(old.app_name, old.app_path, old.priority);
            if (ok)
            {
                app.app_name = name.Text;
                app.app_path = location.Text;
                int.TryParse(priority.Text, out app.priority);
                x = true;
            }
            return app;
        }
    }
}
