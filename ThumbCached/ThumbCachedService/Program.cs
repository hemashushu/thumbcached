using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace Doms.ThumbCached.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new ThumbCachedService() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}

//Copyright (c) 2007-2009, Kwanhong Young, All rights reserved.
//mapleaves@gmail.com
//http://www.domstorage.com