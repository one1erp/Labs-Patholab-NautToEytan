using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NautToEytan.CCOWCOMInterfaces
{
    [
        ComVisible(true),
        Guid("3E3DD272-998E-11D0-808D-00A0240943E4"),
        InterfaceType(ComInterfaceType.InterfaceIsDual)
    ]
    public interface IContextParticipant
    {
        string ContextChangesPending(int contextCoupon, ref string reason);
        void ContextChangesAccepted(int contextCoupon);
        void ContextChangesCanceled(int contextCoupon);
        void CommonContextTerminated();
        void Ping();
    }
}
