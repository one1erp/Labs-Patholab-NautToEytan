using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace NautToEytan.CCOWUtils
{

    public class ContextPendingDecision
    {
        public ContextPendingDecision()
        {
            Decision = "accept-conditional";
            Reason = "";
        }

        public string Decision { get; set; }
        public string Reason { get; set; }
    }

}
