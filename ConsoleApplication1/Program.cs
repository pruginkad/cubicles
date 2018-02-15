using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.InteropServices;
using System.ServiceModel.Web;
using IWshRuntimeLibrary;
using System.IO;


using System.Management;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Permissions;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;
using System.Security;
using System.IO.Pipes;
using System.ComponentModel;
using FolderService;

namespace ConsoleApplication1
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                string username = System.Environment.UserName;

                FolderCreation.AdminUser = Properties.Settings.Default.AdminUser;
                FolderCreation.AdminPassword = Properties.Settings.Default.AdminPassword;
                FolderCreation.ServerName = Properties.Settings.Default.ServerName;
                FolderCreation.SharedFolder = Properties.Settings.Default.SharedFolder;

                Console.WriteLine("Start Impersonate With:" + System.Environment.MachineName +"|"+
                    FolderCreation.AdminUser + "|" + FolderCreation.AdminPassword);

                ImpersonationHelper.Impersonate(System.Environment.MachineName, FolderCreation.AdminUser, FolderCreation.AdminPassword, delegate
                {
                    Console.WriteLine("Impersonation OK");
                    FolderCreation.CreateRoot("C");
                    FolderCreation.CreateFolder(username, "C");
                });
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }
    }
}
