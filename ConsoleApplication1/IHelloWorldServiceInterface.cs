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

}
