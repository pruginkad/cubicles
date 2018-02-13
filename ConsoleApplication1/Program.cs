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

namespace ConsoleApplication1
{
    class Program
    {

        static void Main(string[] args)
        {
            string userName = System.Environment.UserName;

            string test = Environment.GetEnvironmentVariable("ALLUSERSPROFILE");
            //CreateFolder("Galia");
            CreateFolder("Danil");
            //CreateFolder("Toshiba");
            //return;
            ServiceHost serviceHost = null;
            try
            {
                serviceHost = new ServiceHost(typeof(WorkstationServiceLogImp));

                serviceHost.Open();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        private static string GetSid(string user_to_find, string machine, string Username, string Password)
        {
            try
            {
                ConnectionOptions connection = new ConnectionOptions();
                connection.Username = Username;
                connection.Password = Password;
                connection.Authority = "ntlmdomain:";

                ManagementScope scope = new ManagementScope(
                    "\\\\" + machine + "\\root\\CIMV2", connection);
                scope.Connect();

                string s_query = string.Format("SELECT * FROM Win32_UserAccount where Name='{0}'", user_to_find);
                ObjectQuery query = new ObjectQuery(s_query);

                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher(scope, query);

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("Win32_UserAccount instance");
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("Name: {0}", queryObj["Name"]);
                    Console.WriteLine("SID: {0}", queryObj["SID"]);
                    return queryObj["SID"].ToString();
                }
                
            }
            catch (ManagementException err)
            {
                Console.WriteLine("An error occurred while querying for WMI data: " + err.Message);
            }
            catch (System.UnauthorizedAccessException unauthorizedErr)
            {
                Console.WriteLine("Connection error (user name or password might be incorrect): " + unauthorizedErr.Message);
            }
            return string.Empty;
        }

        static string server_name = "WIN7-VIRT";
        static string common_folder = "\\\\" + server_name + "\\Test";

        public static void CreateFolder(string accountName)
        {
            string folderName;
            try
            {
                folderName = common_folder + "\\" + accountName;

                if (Directory.Exists(folderName))
                {
                    Directory.Delete(folderName);
                }
                
                Directory.CreateDirectory(folderName);

                AddDirectorySecurity(accountName, folderName,
                        FileSystemRights.FullControl, AccessControlType.Allow);

                string link = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                + Path.DirectorySeparatorChar + "MyFolder.lnk";
                var shell = new WshShell();
                var shortcut = shell.CreateShortcut(link) as IWshShortcut;
                shortcut.TargetPath = folderName;
                //shortcut.WorkingDirectory = Application.StartupPath;
                //shortcut...
                shortcut.Save();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void AddDirectorySecurity(string Account, string FileName, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);

            // Get a DirectorySecurity object that represents the 
            // current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();




            // Add the FileSystemAccessRule to the security settings. 
            //dSecurity.AddAccessRule(new FileSystemAccessRule(Account, Rights, ControlType));
            string sSid = GetSid(Account, server_name, "Toshiba", "power1");
            var sid = new SecurityIdentifier(sSid);

            dSecurity.AddAccessRule(
               new FileSystemAccessRule(
                   sid,
                   FileSystemRights.FullControl,
                   InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                   PropagationFlags.None,
                   AccessControlType.Allow));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);
        }

        
    }
}
