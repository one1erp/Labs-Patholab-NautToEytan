using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NautToEytan.CCOWCOMInterfaces
{
    [ComVisible(true),Guid("41126C5E-A069-11D0-808F-00A0240943E4"),InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IContextManager
    {
        int MostRecentContextCoupon { get; }
        int JoinCommonContext([In, MarshalAs(UnmanagedType.IDispatch)] object contextParticipant, string sApplicationTitle, bool survey, bool wait);
        void LeaveCommonContext(int participantCoupon);
        int StartContextChanges(int participantCoupon);
        object EndContextChanges(int contextCoupon, ref bool noContinue);
        void UndoContextChanges(int contextCoupon);
        void PublishChangesDecision(int contextCoupon, string decision);
        void SuspendParticipation(int participantCoupon);
        void ResumeParticipation(int participantCoupon, bool wait);
    }
}
