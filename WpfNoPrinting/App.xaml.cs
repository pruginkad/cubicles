using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace WpfNoPrinting
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Process proc = Process.GetCurrentProcess();
            int count = Process.GetProcesses().Where(p =>
                             p.ProcessName == proc.ProcessName).Count();
            if (count > 1)
            {
                App.Current.Shutdown();
            }
        }
    }
}
