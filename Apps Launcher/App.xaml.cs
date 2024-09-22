using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Apps_Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class ApplicationInfo : Application
    {
        Stream st;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher");
                st = File.Open(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Apps Launcher\\dummmy_file", FileMode.OpenOrCreate);
                AppList.SuccessfulStartUp = true;
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                    MessageBox.Show("You can run only one instance at a time.", "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(ex.Message, "Apps Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
                AppList.SuccessfulStartUp = false;
                StartupUri = null;
                Shutdown();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try { st.Close(); }
            catch (Exception) { }
        }
    }
}
