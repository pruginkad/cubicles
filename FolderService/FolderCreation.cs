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
        
      
        static string common_folder ()
        {
            return "\\\\" + ServerName
                + "\\" + SharedFolder;
        }

        public static void CreateRoot(string DiskName)
        {
            string folderName = string.Format(@"\\{0}\{1}$\{2}",
                FolderCreation.ServerName, DiskName, FolderCreation.SharedFolder);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }


            AddDirectorySecurity(AdminUser, folderName,
                    FileSystemRights.FullControl, AccessControlType.Allow);

            ShareFolderPermission(DiskName + @":\" + SharedFolder,
                SharedFolder, 
                FolderCreation.ServerName, FolderCreation.AdminUser, FolderCreation.AdminPassword);
        }

        public static void CreateFolder(string accountName, string DiskName)
        {
            try
            {
                string folderName = string.Format(@"\\{0}\{1}$\{2}\{3}",
                FolderCreation.ServerName, DiskName, FolderCreation.SharedFolder, accountName);


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
                    Console.WriteLine("Found Win32_UserAccount instance");
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

        static public void ShareFolderPermission(string FolderPath, string ShareName,
                                string machine, string Username, string Password)
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

                // Calling Win32_Share class to create a shared folder
                
                ManagementClass managementClass = new ManagementClass(scope, new ManagementPath("Win32_Share"), new ObjectGetOptions());
                // Get the parameter for the Create Method for the folder
                ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");
                ManagementBaseObject outParams;
                // Assigning the values to the parameters
                inParams["Description"] = "Root Shared";
                inParams["Name"] = ShareName;
                inParams["Path"] = FolderPath;
                inParams["Type"] = 0x0;
                // Finally Invoke the Create Method to do the process
                outParams = managementClass.InvokeMethod("Create", inParams, null);
                // Validation done here to check sharing is done or not
                if ((uint)(outParams.Properties["ReturnValue"].Value) != 0)
                {
                    Console.WriteLine("Folder might be already in share or unable to share the directory");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
