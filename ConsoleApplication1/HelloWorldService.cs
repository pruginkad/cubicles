using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.IO;
using System.Configuration;

namespace WindowsService1
{
    [ServiceContract(Namespace = "")]
    [XmlSerializerFormat]
    public interface IHelloWorldService
    {
        [OperationContract]
        [WebGet]
        string GetDataCenter();

        [OperationContract]
        [WebGet]
        string SetDataCenter(string newIp, string redirect);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream Form();

        [OperationContract]
        string SetStatus(ServiceState status);

        [OperationContract]
        [WebGet]
        [ServiceKnownType(typeof(ServiceState))] //UserDefined types you should add manually
        List<ServiceState> GetObject();
    }

    public class HelloWorldService : IHelloWorldService
    {
        AService1 GetService()
        {
            return Program.g_AService;
        }
        
        //http://localhost:8000/DataCenter/GetDataCenter
        public string GetDataCenter()
        {
            string s = string.Empty;
            try
            {
                s = Properties.Settings.Default.DataCenter.ToString();
            }
            catch (Exception ex)
            {
            }
            return s;
        }

        //http://localhost:8000/DataCenter/SetDataCenter?newIp=127.0.0.1
        public string SetDataCenter(string newIp, string redirect)
        {
            try
            {
                Properties.Settings.Default.DataCenter = newIp;
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

        public Stream Form()
        {
            string html = Properties.Resources.index.Replace("newip_val", Properties.Settings.Default.DataCenter);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return new MemoryStream(Encoding.UTF8.GetBytes(html));
        }

        static Dictionary<string, Dictionary<string, ServiceState>> m_states = new Dictionary<string, Dictionary<string, ServiceState>>();

        public string SetStatus(ServiceState status)
        {
            lock (m_states)
            {
                m_states[status.MachineName][status.State] = status;
                return "OK";
            }
        }

        public List<ServiceState> GetObject()
        {
            List<ServiceState> list = new List<ServiceState>();
            lock (GetService().m_states)
            {
                foreach (KeyValuePair<string, ServiceState> item in GetService().m_states)
                {
                    list.Add(item.Value);
                }
                return list;
            }
        }
    }
}
