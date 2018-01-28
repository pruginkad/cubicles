using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace WindowsService1
{
    [DataContract]
    [XmlSerializerFormat]
    public class ServiceState
    {
        public ServiceState()
        {
        }
        public ServiceState(string state, string username)
        {
            timestamp = DateTime.Now;
            State = state;
            UserName = username;
            MachineName = System.Environment.MachineName;
        }
        [DataMember]
        public DateTime timestamp { get; set; }

        [DataMember]
        public string State { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string MachineName { get; set; }
    }
}
