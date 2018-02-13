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
        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            {
            }

            ///////////////////////////////////////
            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        public class ImpersonationHelper
        {
            enum LogonType
            {
                Interactive = 2,
                Network = 3,
                Batch = 4,
                Service = 5,
                Unlock = 7,
                NetworkClearText = 8,
                NewCredentials = 9
            }

            enum LogonProvider
            {
                Default = 0,
                WinNT35 = 1,
                WinNT40 = 2,
                WinNT50 = 3
            }
            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool LookupAccountName([In, MarshalAs(UnmanagedType.LPTStr)] string systemName, [In, MarshalAs(UnmanagedType.LPTStr)] string accountName, IntPtr sid, ref int cbSid, StringBuilder referencedDomainName, ref int cbReferencedDomainName, out int use);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool ConvertSidToStringSid(IntPtr sid, [In, Out, MarshalAs(UnmanagedType.LPTStr)] ref string pStringSid);


            /// <summary>The method converts object name (user, group) into SID string.</summary>
            /// <param name="name">Object name in form domain\object_name.</param>
            /// <returns>SID string.</returns>
            public static string GetSidOf(string machine_name, string name)
            {
                IntPtr _sid = IntPtr.Zero; //pointer to binary form of SID string.
                int _sidLength = 0;   //size of SID buffer.
                int _domainLength = 0;  //size of domain name buffer.
                int _use;     //type of object.
                StringBuilder _domain = new StringBuilder(); //stringBuilder for domain name.
                int _error = 0;
                string _sidString = "";

                //first call of the function only returns the sizes of buffers (SDI, domain name)
                LookupAccountName(machine_name, name, _sid, ref _sidLength, _domain, ref _domainLength, out _use);
                _error = Marshal.GetLastWin32Error();

                if (_error != 122) //error 122 (The data area passed to a system call is too small) - normal behaviour.
                {
                    throw (new Exception(new Win32Exception(_error).Message));
                }
                else
                {
                    _domain = new StringBuilder(_domainLength); //allocates memory for domain name
                    _sid = Marshal.AllocHGlobal(_sidLength); //allocates memory for SID
                    bool _rc = LookupAccountName(null, name, _sid, ref _sidLength, _domain, ref _domainLength, out _use);

                    if (_rc == false)
                    {
                        _error = Marshal.GetLastWin32Error();
                        Marshal.FreeHGlobal(_sid);
                        throw (new Exception(new Win32Exception(_error).Message));
                    }
                    else
                    {
                        // converts binary SID into string
                        _rc = ConvertSidToStringSid(_sid, ref _sidString);

                        if (_rc == false)
                        {
                            _error = Marshal.GetLastWin32Error();
                            Marshal.FreeHGlobal(_sid);
                            throw (new Exception(new Win32Exception(_error).Message));
                        }
                        else
                        {
                            Marshal.FreeHGlobal(_sid);
                            return _sidString;
                        }
                    }
                }
            }

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            private extern static bool CloseHandle(IntPtr handle);

            [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
            public static void Impersonate(string domainName, string userName, string userPassword, Action actionToExecute)
            {
                SafeTokenHandle safeTokenHandle;
                try
                {
                    
                    // Call LogonUser to obtain a handle to an access token.
                    bool returnValue = LogonUser(userName, domainName, userPassword,
                        (int)LogonType.NewCredentials,
                        (int)LogonProvider.WinNT50,
                        out safeTokenHandle);
                    //Facade.Instance.Trace("LogonUser called.");

                    if (returnValue == false)
                    {
                        int ret = Marshal.GetLastWin32Error();
                        //Facade.Instance.Trace($"LogonUser failed with error code : {ret}");

                        throw new System.ComponentModel.Win32Exception(ret);
                    }

                    using (safeTokenHandle)
                    {
                        //Facade.Instance.Trace($"Value of Windows NT token: {safeTokenHandle}");
                        //Facade.Instance.Trace($"Before impersonation: {WindowsIdentity.GetCurrent().Name}");

                        // Use the token handle returned by LogonUser.
                        using (WindowsIdentity newId = new WindowsIdentity(safeTokenHandle.DangerousGetHandle()))
                        {
                            using (WindowsImpersonationContext impersonatedUser = newId.Impersonate())
                            {
                                //Facade.Instance.Trace($"After impersonation: {WindowsIdentity.GetCurrent().Name}");
                                //Facade.Instance.Trace("Start executing an action");
                                
                                actionToExecute();
                                
                                //Facade.Instance.Trace("Finished executing an action");
                            }
                        }
                        //Facade.Instance.Trace($"After closing the context: {WindowsIdentity.GetCurrent().Name}");
                    }

                }
                catch (Exception ex)
                {
                    //Facade.Instance.Trace("Oh no! Impersonate method failed.");
                    //ex.HandleException();
                    //On purpose: we want to notify a caller about the issue /Pavel Kovalev 9/16/2016 2:15:23 PM)/
                    throw;
                }
            }
        }
        static string server_name = "WIN7-VIRT";
        static string common_folder = "\\\\WIN7-VIRT\\Test";

        public static void CreateFolder(string accountName)
        {
            string folderName;
            try
            {
                accountName = @"Danil";
                folderName = common_folder + "\\" + accountName;
                //DirectorySecurity securityRules = new DirectorySecurity();
                //securityRules.AddAccessRule(new FileSystemAccessRule(@"WIN7-VIRT\Toshiba", 
                //    FileSystemRights.FullControl, AccessControlType.Allow));
                if (Directory.Exists(folderName))
                {
                    var accessControlList = Directory.GetAccessControl(folderName);
                    var accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));

                    foreach (FileSystemAccessRule rule in accessRules)
                    {
                        Console.WriteLine(rule.IdentityReference.Value);
                    }
                    Directory.Delete(folderName);
                }
                
                ImpersonationHelper.Impersonate("WIN7-VIRT", @"Toshiba", "power1", delegate
                {
                    Directory.CreateDirectory(folderName);

                    AddDirectorySecurity(@"Galia", @"\\WIN7-VIRT\Test\Danil",
                    FileSystemRights.FullControl, AccessControlType.Allow);
                });
                

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
            string asusGal = ImpersonationHelper.GetSidOf("ASUS", @"Galia");
            string virtGal = ImpersonationHelper.GetSidOf("WIN7-VIRT", @"Galia");
            var sid = new SecurityIdentifier("S-1-5-21-2713912118-2801941678-3297190592-1006");
            var sid2 = new SecurityIdentifier(virtGal);
            
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

        private static void CreateShortcut()
        {
            string userName = System.Environment.UserName;

            CreateFolder("Galia");

            string link = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                + Path.DirectorySeparatorChar + "MyFolder.lnk";
            var shell = new WshShell();
            var shortcut = shell.CreateShortcut(link) as IWshShortcut;
            shortcut.TargetPath = "\\\\WIN7-VIRT\\Test";
            //shortcut.WorkingDirectory = Application.StartupPath;
            //shortcut...
            shortcut.Save();
        }

        static void Main(string[] args)
        {
            CreateShortcut();
            return;
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
    }
}
