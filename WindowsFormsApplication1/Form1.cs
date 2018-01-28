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

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
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
    }
}
