using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Configuration;
using System.Reflection;


namespace FolderService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            //
            InitializeComponent();
            this.serviceProcessInstaller1.Password = Properties.Settings.Default.AdminPassword;
            this.serviceProcessInstaller1.Username = @".\" + Properties.Settings.Default.AdminUser;
        }
    }
}
