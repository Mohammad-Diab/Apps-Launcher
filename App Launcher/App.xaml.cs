using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace App_Launcher
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
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\App Launcher"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\App Launcher");
                st = File.Open(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\App Launcher\\dummmy_file", FileMode.OpenOrCreate);
                AppList.SuccessfulStartUp = true;
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    MessageBox.Show("You can run only one instance at a time.", "App Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
                    AppList.SuccessfulStartUp = false;
                    Shutdown();
                }
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try { st.Close(); }
            catch (Exception) { }
        }
    }
}
