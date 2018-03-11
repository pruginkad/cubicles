using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Management;
using System.Diagnostics;
using Microsoft.Win32;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SystemEvents.SessionEnding += SessionEndingEvtHandler;
        }

        private void SessionEndingEvtHandler(object sender, SessionEndingEventArgs e)
        {
            e.Cancel = true;
            label1.Text = "SessionEndingEvtHandler";
        }

        private const int WM_QUERYENDSESSION = 0x0011;
        private bool isShuttingDown = false;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_QUERYENDSESSION)
            {
                isShuttingDown = true;
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }
        private void frmLogin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isShuttingDown)
            {
                if (MessageBox.Show(this, "The application is still running, are you sure you want to exit?",
                "Confirm Closing by Windows Shutdown", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                {
                    e.Cancel = false;
                }
                else
                    e.Cancel = true;
            }
        }
        public String getUsername()
        {
            string username = string.Empty;
            try
            {
                // Define WMI scope to look for the Win32_ComputerSystem object
                ManagementScope ms = new ManagementScope("\\\\.\\root\\cimv2");
                ms.Connect();

                ObjectQuery query = new ObjectQuery
                        ("SELECT * FROM Win32_ComputerSystem");
                ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher(ms, query);

                // This loop will only run at most once.
                foreach (ManagementObject mo in searcher.Get())
                {
                    // Extract the username
                    username += mo["UserName"].ToString();
                }
            }
            catch (Exception)
            {
                // The system currently has no users who are logged on
                // Set the username to "SYSTEM" to denote that
                username = "SYSTEM";
            }
            return username;
        } // end String getUsername()

        public static string EVENTS_SOURCE = "WS_EVENT";
        public static EventLog m_EventLog = new EventLog("AService1Events", System.Environment.MachineName, EVENTS_SOURCE);
        EventInstance myInfoEvent = new EventInstance(0, 0, EventLogEntryType.Information);

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] insertStrings = { "fff" };
            byte[] binaryData = { };
            //m_EventLog.WriteEvent(myInfoEvent, binaryData, insertStrings);

            System.Diagnostics.EventLog.Delete("AService1Events");
            this.Text = getUsername();   
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown || isShuttingDown)
                e.Cancel = true;
        }
    }
}
