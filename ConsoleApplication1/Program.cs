using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.InteropServices;
using System.ServiceModel.Web;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost serviceHost = null;
            try
            {
                serviceHost = new ServiceHost(typeof(WorkstationServiceLogImp));

                serviceHost.Open();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
