using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Runtime.InteropServices;

namespace FolderService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            #if (DEBUG)
            //System.Diagnostics.Debugger.Launch();
            #endif
            //FolderCreation.CreateFolder("Galia");
        }

        protected override void OnStop()
        {
        }

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);
        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);

        private enum WtsInfoClass
        {
            WTSUserName = 5,
            WTSDomainName = 7,
        }

        private static string GetUsername(int sessionId, bool prependDomain = false)
        {
            if (sessionId < 0)
            {
                return string.Empty;
            }
            IntPtr buffer;
            int strLen;
            string username = "SYSTEM";
            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) && strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory(buffer);
                if (prependDomain)
                {
                    if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer, out strLen) && strLen > 1)
                    {
                        username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
                        WTSFreeMemory(buffer);
                    }
                }
            }
            return username;
        }
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            try
            {
                int sessionId = changeDescription.SessionId;

                switch (changeDescription.Reason)
                {
                    case SessionChangeReason.SessionLogon:
                        {
                            string username = GetUsername(sessionId);
                            FolderCreation.AdminUser = Properties.Settings.Default.AdminUser;
                            FolderCreation.AdminPassword = Properties.Settings.Default.AdminPassword;
                            FolderCreation.ServerName = Properties.Settings.Default.ServerName;
                            FolderCreation.SharedFolder = Properties.Settings.Default.SharedFolder;
                            FolderCreation.CreateFolder(username, "C");
                            break;
                        }

                    case SessionChangeReason.SessionLogoff:
                        {
                            //SetState("SessionLogoff", sessionId);
                            break;
                        }

                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}
