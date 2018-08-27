using System;
using System.Collections.Generic;

namespace KISS
{
    class Program
    {
        static int Main(string[] args)
        {
            var largs = new List<string>();
            if (args.Length > 0)
                largs.AddRange(args);
            else
                largs.Add("/f:kissprops.xml");
            
            ArgumentHelper.processProps(largs);
            bool ReturnSuccess = ArgumentHelper.processScripts(largs);
            if (ReturnSuccess)
                return 0;
            else
                return 400;
        }
    }
}
