using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace NautToEytan.CCOWUtils
{

    public class SetContextResult
    {
        public string[] Reasons { get; internal set; }

        public bool NoContinue { get; internal set; }
    }

}
