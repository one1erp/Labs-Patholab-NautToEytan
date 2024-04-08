using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using NautToEytan;
using NautToEytan.CCOWUtils;
using NautToEytan.CCOWCOMInterfaces;


namespace NautToEytan.CCOWUtils
{

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ContextParticipant : IContextParticipant
    {
        //private ContextParticipantUserControl _participant;

        public ContextParticipant() //ContextParticipantUserControl participant)
        {
           // _participant = participant;
        }

        #region IContextParticipant Members

        /// <summary>
        /// Informs a participant in a common context session that a change to the common context data is pending.
        /// </summary>
        /// <param name="contextCoupon">The transaction within which the context changes occurred.</param>
        /// <param name="reason">The reason provided by the participant if it cannot accept the changes.</param>
        /// <returns>The decision the participant made about the changes.</returns>
        public string ContextChangesPending(int contextCoupon, ref string reason)
        {
            ContextPendingDecision pendingDecision = new ContextPendingDecision(); //_participant.RaiseContextChangesPendingEventHandler(contextCoupon, reason);
            ////back decision to Context Receiver
            reason = pendingDecision.Reason;
            return pendingDecision.Decision;
        }

        /// <summary>
        /// Informs a participant in a common context session that the result of the most recent context change survey was to accept the changes and that the common context data has indeed been set.
        /// </summary>
        /// <param name="contextCoupon">The transaction within which the context changes occurred.</param>
        public void ContextChangesAccepted(int contextCoupon)
        {
           // _participant.RaiseContextChangesAcceptedEventHandler(contextCoupon);

        }

        /// <summary>
        /// Informs a participant in a common context session that a context change transaction has been canceled.
        /// </summary>
        /// <param name="contextCoupon">The transaction within which the context changes occurred.</param>
        public void ContextChangesCanceled(int contextCoupon)
        {
           // _participant.RaiseContextChangesCanceledEventHandler(contextCoupon);
        }

        /// <summary>
        /// Informs a participant in a common context session that the session is being terminated.  
        /// </summary>
        public void CommonContextTerminated()
        {
           // _participant.RaiseCommonContextTerminatedEventHandler();
        }

        /// <summary>
        /// Provides a means for a context manager to determine whether or not a participant in a common context session is still running.  
        /// </summary>
        public void Ping()
        {
              //_participant.RaisePingEventHandler();    
        }

        #endregion
     
    }
}
