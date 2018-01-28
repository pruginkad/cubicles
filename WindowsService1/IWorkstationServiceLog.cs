using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using System.IO;

//[21:13:28] David Fried: http://localhost:52323/api/WorkstationServiceLog/?workstation=efjcnwmc&User=voemk&Event=cfhwb&time=1-1-2030

//[21:14:49] cubicles97: m-d-yyyy
namespace ConsoleApplication1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IWorkstationServiceLog" in both code and config file together.
    [ServiceContract(Namespace = "")]
    public interface IWorkstationServiceLog
    {
        [OperationContract]
        [WebGet]
        void WorkstationServiceLog(string Workstation, string User, string Event, DateTime Time);
    }
}
