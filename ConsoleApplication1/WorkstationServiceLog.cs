using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Web;

namespace ConsoleApplication1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "WorkstationServiceLog" in both code and config file together.
    public class WorkstationServiceLogImp : IWorkstationServiceLog
    {
        public void WorkstationServiceLog(string Workstation, string User, string Event, DateTime Time)
        {
            string s = string.Format("{0},{1},{2},{3}",Workstation,User,Event,Time);
            Console.WriteLine(s);
        }
    }
}
