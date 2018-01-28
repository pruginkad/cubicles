using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;

namespace WindowsService1
{
    [ServiceContract(Namespace = "")]
    public interface ILocalWorkstationState
    {
        [OperationContract]
        [WebGet]
        string GetApiPath();

        [OperationContract]
        [WebGet]
        string SetApiPath(string apiPath, string redirect);

        [OperationContract]
        [WebGet]
        [ServiceKnownType(typeof(ServiceState))]
        List<ServiceState> GetEvents(int pastNumberOfMinutes, string username, string eventname);
    }

}
