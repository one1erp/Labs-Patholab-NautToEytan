using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NautToEytan.CCOWUtils;
using NautToEytan.CCOWCOMInterfaces;
using System.Windows.Forms;
using Patholab_Common;

namespace NautToEytan
{
    public class RunProgram
    {
        #region Enum
        public enum ParticipationStateEnum : int
        {
            NotJoined = 0,
            Joined = 1,
            Suspended = 2,
            ManagementUndefined = 3
        }
        #endregion

        private ParticipationStateEnum _participationState = ParticipationStateEnum.ManagementUndefined;
        private ContextManagerHelper _contextManagerHelper = null;
        private ContextParticipant _contextParticipant = null;



        public RunProgram()
        {

        }


        public bool Join_to_Context(string ParticipantName, string Passcode)
        {
            if (_participationState != ParticipationStateEnum.Joined)
            {
                try
                {
                    _contextManagerHelper = new ContextManagerHelper();
                    _contextManagerHelper.ParticipantApplicationName = ParticipantName;
                    _contextManagerHelper.ParticipantIdentifier = Guid.NewGuid().ToString();
                    _contextParticipant = new ContextParticipant();
                    _contextManagerHelper.Passcode = Passcode;
                    _contextManagerHelper.JoinContext((IContextParticipant)_contextParticipant, false, false);
                    _participationState = ParticipationStateEnum.Joined;
                    Logger.WriteLogFile("join success");
                    return true;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    Logger.WriteLogFile("error in naut2eytan joinToContext func:  " + ex.Message);

                }
            }
            return false;
        }


        public bool Login(string programSufix,string user_name)
        {
            if (_participationState == ParticipationStateEnum.Joined)
            {
                try
                {
                    string ManageUserContextParamSufix = programSufix;//getPhraseEntry("Manage User Context Param Sufix");
                    string ManageUserContextParamValue = user_name;//getPhraseEntry("Manage User Context Param Value");
                    SendCotext(String.Format("{1}{0}", ManageUserContextParamSufix, "user.id.logon."), ManageUserContextParamValue);
                    Logger.WriteLogFile("login success");
                    return true;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    Logger.WriteLogFile("error in naut2eytan Login func:  " + ex.Message);
                }
            }
            return false;
        }

        public bool Open_Patient_File(string programSufix/*יוזר המחובר לנאוטילוס*/, string recentPatient/*תז פציינט*/)
        {
            if (recentPatient != null)
            {
                int i;
                if (int.TryParse(recentPatient, out i))
                {
                    try
                    {
                        SendCotext(String.Format("{1}{0}", programSufix, "patient.id.mrn."), recentPatient);
                        Logger.WriteLogFile("open_patient success");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                        Logger.WriteLogFile("error in naut2eytan OpenPatientFile func:  " + ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    SendCotext(String.Format("{1}{0}", programSufix, "patient.id.mrn."), "");
                    Logger.WriteLogFile("close_patient success");
                    return true;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    Logger.WriteLogFile("error in naut2eytan OpenPatientFile func:  " + ex.Message);
                }
            }
            return false;
        }
        public bool Leave_Context()
        {
            try
            {
                if (_participationState == ParticipationStateEnum.Joined)
                {
                    _contextManagerHelper.LeaveContext();
                    _contextParticipant = null;
                    _participationState = ParticipationStateEnum.NotJoined;
                    Logger.WriteLogFile("leave success");
                    return true;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                Logger.WriteLogFile("error in naut2eytan LeaveContext func:  " + ex.Message);
            }
            return false;
        }



        private void SendCotext(string contextParameterName, string contextParameterValue)
        {
            try
            {
                Dictionary<string, string> cxt = new Dictionary<string, string>();
                cxt.Add(contextParameterName, contextParameterValue);
                _contextManagerHelper.StartTransaction();
                _contextManagerHelper.SetContext(cxt);
                _contextManagerHelper.EndTransaction();
                _contextManagerHelper.PublishChangeDecision("accept");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                Logger.WriteLogFile("Error in SendCotext" + ex.Message);
            }
        }
    }
}
