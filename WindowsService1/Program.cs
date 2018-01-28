using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace WindowsService1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public static AService1 g_AService = null;
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            g_AService = new AService1();
            ServicesToRun = new ServiceBase[] 
			{ 
				g_AService
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
