using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace WindowsService1
{
    public class LocalWorkstationState : ILocalWorkstationState
    {
        AService1 GetService()
        {
            return Program.g_AService;
        }

        //http://localhost:8000/DataCenter/GetApiPath
        public string GetApiPath()
        {
            GetService().ReadApiPath();
            return AService1.API_PATH;
        }

        public string SetMode(string mode)
        {
            if (string.IsNullOrEmpty(mode) ||
                !mode.ToLower().Contains("active") ||
                mode.ToLower().Contains("standby"))
            {
                return "Wrong mode";
            }
            try
            {
                GetService().MODE = mode;
            }
            catch (Exception ex)
            {
                return "Error:" + ex.Message;
            }
            return "Ok";
        }

        public string SetApiPath(string apiPath, string redirect)
        {
            try
            {
                Properties.Settings.Default.ASPDataCenter = apiPath;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                return "ERROR";
            }
            if (!string.IsNullOrEmpty(redirect))
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Redirect;
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Location", redirect);
            }

            return "OK";
        }

        public List<ServiceState> GetEvents(int pastNumberOfMinutes, string username, string eventname)
        {
            List<ServiceState> list = new List<ServiceState>();

            IEnumerable<EventLogEntry> coll = AService1.m_EventLog.Entries.OfType<EventLogEntry>();
            if (pastNumberOfMinutes > 0)
            {
                TimeSpan span = TimeSpan.FromMinutes(pastNumberOfMinutes);
                DateTime dt = DateTime.Now - span;
                coll = AService1.m_EventLog.Entries.
                    Cast<EventLogEntry>().Where(t => t.TimeGenerated > dt);

            }

            coll = coll.OrderByDescending(time => time.TimeGenerated);

            var jsSerializer = new JavaScriptSerializer();
            foreach (EventLogEntry e in coll)
            {
                try
                {
                    if (e.ReplacementStrings.Count() > 0)
                    {
                        ServiceState state = jsSerializer.Deserialize<ServiceState>(e.ReplacementStrings[0]);
                        state.timestamp = state.timestamp.ToLocalTime();
                        if (!string.IsNullOrEmpty(username))
                        {
                            if (!state.UserName.ToLower().Contains(username.ToLower()))
                            {
                                continue;
                            }
                        }

                        if (!string.IsNullOrEmpty(eventname))
                        {
                            if (!state.State.ToLower().Contains(eventname.ToLower()))
                            {
                                continue;
                            }
                        }
                        list.Add(state);
                        if (pastNumberOfMinutes == 0)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            
            return list;
        }
    }
}
