using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Apps_Launcher
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedIndex == 0)
            {
                Properties.Settings.Default.HideToRight = false;
                AppList.Layout = HideLayout.HideToLeft;
            }
            else
            {
                AppList.Layout = HideLayout.HideToRight;
                Properties.Settings.Default.HideToRight = true;
            }
            Properties.Settings.Default.Save();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppList.Layout == HideLayout.HideToRight)
                comboBox.SelectedIndex = 1;
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
