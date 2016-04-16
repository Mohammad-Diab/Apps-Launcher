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

namespace App_Launcher
{
    /// <summary>
    /// Interaction logic for ModrenMessageBox.xaml
    /// </summary>
    public partial class ModrenMessageBox : Window
    {
        ModrenMessageResult to_renern = ModrenMessageResult.Cancel;
        public static ModrenMessageResult ShowMessage(out string arguments)
        {
            ModrenMessageBox window = new ModrenMessageBox();
            window.ShowDialog();

            arguments = window.textBox.Text;
            return window.to_renern;
        }

        public ModrenMessageBox()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            to_renern = ModrenMessageResult.Ok;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            to_renern = ModrenMessageResult.Cancel;
            Close();
        }
    }

    public enum ModrenMessageResult
    {
        Yes = 1,
        No = 2,
        Ok = 3,
        Cancel = 4
    }
}
