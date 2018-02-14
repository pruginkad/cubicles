using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.AccessControl;
using System.IO;
using System.Management;
using IWshRuntimeLibrary;
using System.Security.Principal;

namespace FolderService
{
    class FolderCreation
    {
        static public string AdminUser
        {
            get;set;
        }
        static public string AdminPassword
        {
            get;set;
        }
        static public string ServerName
        {
            get;set;
        }
        static public string SharedFolder
        {
            get;set;
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

        
        static string common_folder ()
        {
            return "\\\\" + ServerName
                + "\\" + SharedFolder;
        }

        public static void CreateFolder(string accountName)
        {
            try
            {
                string folderName = common_folder() + "\\" + accountName;

                if (Directory.Exists(folderName))
                {
                    //Directory.Delete(folderName);
                }

                Directory.CreateDirectory(folderName);

                AddDirectorySecurity(accountName, folderName,
                        FileSystemRights.FullControl, AccessControlType.Allow);


                string link = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                link = link.Replace(Environment.UserName, accountName);
                
                link = link + Path.DirectorySeparatorChar + "MyFolder.lnk";
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


            string sSid = GetSid(Account,
                ServerName,
                AdminUser, 
                AdminPassword);
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
