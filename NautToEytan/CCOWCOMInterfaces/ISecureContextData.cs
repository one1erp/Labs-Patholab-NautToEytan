using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NautToEytan.CCOWCOMInterfaces
{
    [ComVisible(true),Guid("6F530680-BC14-11D1-90B1-76C60D000000"),InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface ISecureContextData
    {
        object GetItemNames(int contextCoupon);
        void SetItemValues(int participantCoupon, object itemNames, object itemValues, int contextCoupon, string appSignature);
        object GetItemValues(int participantCoupon, object names, bool onlyChanges, int contextCoupon, string appSignature, ref string managerSignature);
    }
}
