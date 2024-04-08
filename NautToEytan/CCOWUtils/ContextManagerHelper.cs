using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;
using NautToEytan;
using NautToEytan.CCOWUtils;
using NautToEytan.CCOWCOMInterfaces;
//using System.Windows.Forms;

namespace NautToEytan.CCOWUtils
{
    public class ContextManagerHelper
    {

        public enum ParticipationStateEnum : int
        {
            NotJoined = 0,
            Joined = 1,
            Suspended = 2,
            ManagementUndefined = 3
        }

        #region Constants
        private const int CRYPTO_PROVIDER_TYPE = 1; // PROV_RSA_FULL
        private const string CRYPTO_PROVIDER_NAME = "Microsoft Base Cryptographic Provider v1.0";
        private const int CRYPTO_KEY_LENGHT = 1024;

        private readonly string[] CCOW_SECURE_BINDING_PROPERTIES_NAMES = new string[] { "Technology", "PubKeyScheme", "HashAlgo" };
        private readonly string[] CCOW_SECURE_BINDING_PROPERTIES_VALUES = new string[] { "CRYPTO32", "RSA_EXPORTABLE", "MD5" };
        #endregion

        public ContextManagerHelper()
        {
            IsSecureBinding = true;
        }

        private object _CCOWContextManager;
        internal object CCOWContextManager
        {
            get
            {
                if (_CCOWContextManager == null)
                {
                    try
                    {
                        Type objContextManagerType = Type.GetTypeFromProgID("CCOW.ContextManager");
                        _CCOWContextManager = Activator.CreateInstance(objContextManagerType) as IContextManager;
                    }
                    catch(Exception ex)
                    { 
                        throw new Exception(String.Format("Context Manager not found, error: {0}", ex));
                    }

                }
                return _CCOWContextManager;
            }
            set
            {
                _CCOWContextManager = value;
            }
        }

        private IContextManager ContextManager { get { return CCOWContextManager as IContextManager; } }
        private ISecureBinding SecureBinding { get { return CCOWContextManager as ISecureBinding; } }
        private ISecureContextData SecureContextData { get { return CCOWContextManager as ISecureContextData; } }
        private IContextData ContextData { get { return CCOWContextManager as IContextData; } }
        public string Passcode { get; set; }
        public string ParticipantApplicationName { get; set; }
        public string ParticipantIdentifier { get; set; }
        public bool IsSecureBinding { get; set; }
        public int LastUsedParticipantCoupon { get; set; }
        public int CommittedContextCoupon { get; set; }
        public int NotCommittedContextCoupon { get; set; }
        private RSACryptoServiceProvider _participantCryptoProvider;
        private RSACryptoServiceProvider _managerCryptoProvider;

        public int JoinContext(IContextParticipant part, bool survey, bool waitJoin)
        {
          
            string fullTitle = String.Format("{0}#{1}", ParticipantApplicationName, ParticipantIdentifier);
            //  LastUsedParticipantCoupon = this.ContextManager.JoinCommonContext(part, fullTitle, survey, waitJoin);
              LastUsedParticipantCoupon = ContextManager.JoinCommonContext(part, fullTitle, survey, waitJoin);

            if (IsSecureBinding)
            {
                CspParameters cryptoParams = new CspParameters(CRYPTO_PROVIDER_TYPE, CRYPTO_PROVIDER_NAME);
                cryptoParams.KeyNumber = (int)KeyNumber.Signature;
                _participantCryptoProvider = new RSACryptoServiceProvider(CRYPTO_KEY_LENGHT, cryptoParams);
                _managerCryptoProvider = new RSACryptoServiceProvider(CRYPTO_KEY_LENGHT, cryptoParams);

                //Validate Context Manager signature
                string binderPublicKey = null;
                string mac = SecureBinding.InitializeBinding(LastUsedParticipantCoupon, CCOW_SECURE_BINDING_PROPERTIES_NAMES, CCOW_SECURE_BINDING_PROPERTIES_VALUES, ref binderPublicKey);
                byte[] byteHash = ComputeHash(binderPublicKey + Passcode);
                string hash = ByteArray2HexString(byteHash);

                
                if (mac.ToLowerInvariant() != hash.ToLowerInvariant())
                    throw new Exception("CCOW Secure binding failed");
                
                //Participant signature
                var publicKey = _participantCryptoProvider.ExportCspBlob(false);
                string bindeePublicKey = ByteArray2HexString(publicKey);
                byte[] byteMac1 = ComputeHash(bindeePublicKey + Passcode);
                string mac1 = ByteArray2HexString(byteMac1);
                var access = (string[])SecureBinding.FinalizeBinding(LastUsedParticipantCoupon, bindeePublicKey, mac1);

                // Forming manager crypto provider
                byte[] managerKey = HexString2ByteArray(binderPublicKey);
                _managerCryptoProvider.ImportCspBlob(managerKey);
            }

            return LastUsedParticipantCoupon;
        }

        public int StartTransaction()
        {
           NotCommittedContextCoupon = this.ContextManager.StartContextChanges(LastUsedParticipantCoupon);
           return NotCommittedContextCoupon;
        }

        public SetContextResult EndTransaction()
        {
            SetContextResult result = new SetContextResult();
            bool noContinue = false;
            result.Reasons = (string[])this.ContextManager.EndContextChanges(NotCommittedContextCoupon, ref noContinue);
            result.NoContinue = noContinue;
            return result;
        }

        public void SetContext(Dictionary<string, string> ctx)
        {

            // calculating signature
            StringBuilder encryptedByParticipant = new StringBuilder();
            encryptedByParticipant.Append(LastUsedParticipantCoupon);
            ctx.Keys.ToArray().Aggregate(encryptedByParticipant, (currentNames, item) => currentNames.Append(item));
            ctx.Values.ToArray().Aggregate(encryptedByParticipant, (currentNames, item) => currentNames.Append(item));
            encryptedByParticipant.Append(NotCommittedContextCoupon);

            var participantHash = ComputeHash(encryptedByParticipant.ToString());
            var participantSign = _participantCryptoProvider.SignHash(participantHash, "MD5");
            var appSignature = ByteArray2HexString(ReverseSignature(participantSign));

            // Set ContextData
            if (IsSecureBinding)
            {
                this.SecureContextData.SetItemValues(LastUsedParticipantCoupon, ctx.Keys.ToArray(), ctx.Values.ToArray(), NotCommittedContextCoupon, appSignature);
            }
            else
            {
                this.ContextData.SetItemValues(LastUsedParticipantCoupon, ctx.Keys.ToArray(), ctx.Values.ToArray(), NotCommittedContextCoupon);
            }
        }

        public void PublishChangeDecision(string decision)
        {
            ContextManager.PublishChangesDecision(NotCommittedContextCoupon, decision);
            CommittedContextCoupon = NotCommittedContextCoupon;
        }

        public void Suspend()
        {
            ContextManager.SuspendParticipation(LastUsedParticipantCoupon);
        }

        public void Resume(bool wait)
        {
            ContextManager.ResumeParticipation(LastUsedParticipantCoupon, wait);
        }

        public void UndoTransaction()
        {
            ContextManager.UndoContextChanges(NotCommittedContextCoupon);
        }

        public int GetMostRecentContextCoupon()
        {
            CommittedContextCoupon = ContextManager.MostRecentContextCoupon;
            return CommittedContextCoupon;
        }

        public IDictionary<string, string> GetContext(int curContextCoupon, bool onlyChanges)
        {
            var context = new Dictionary<string, string>();
            object[] values;
            if (IsSecureBinding)
            {
                string[] names = (string[])SecureContextData.GetItemNames(curContextCoupon);
                string contextSignature = GenerateContextDataRequestSignature(names, onlyChanges, curContextCoupon);
                string managerSignature = null;
                values = (object[])SecureContextData.GetItemValues(LastUsedParticipantCoupon, names, onlyChanges, curContextCoupon, contextSignature, ref managerSignature);
                if (!ValidateContextDataManagerSignature(values, managerSignature, curContextCoupon))
                    throw new Exception("Invalid CCOW Context Manager signature");
            }
            else
            {
                string[] names = (string[])ContextData.GetItemNames(curContextCoupon);
                values = (object[])this.ContextData.GetItemValues(names, onlyChanges, curContextCoupon);
            }

            for (int i = 0; i < values.Length; i += 2)
            {
                string name = values[i] == null ? null : values[i].ToString();
                if (String.IsNullOrEmpty(name))
                    continue;
                context[name] = values[i + 1] == null ? String.Empty : values[i + 1].ToString();
            }

            return context;
        }

        public void LeaveContext()
        {
            this.ContextManager.LeaveCommonContext(LastUsedParticipantCoupon);
        }

        #region cryptography-related private methods
        private string GenerateContextDataRequestSignature(string[] names, bool onlyChanges, int contextCoupon)
        {
            StringBuilder encryptedByParticipant = new StringBuilder();
            encryptedByParticipant.Append(LastUsedParticipantCoupon);
            foreach (var name in names)
            {
                encryptedByParticipant.Append(name);
            }
            encryptedByParticipant.Append(onlyChanges ? "1" : "0");
            encryptedByParticipant.Append(contextCoupon);

            var participantHash = ComputeHash(encryptedByParticipant.ToString());
            var participantSign = _participantCryptoProvider.SignHash(participantHash, "MD5");
            var participantSignature = ByteArray2HexString(ReverseSignature(participantSign));
            return participantSignature;
        }

        private bool ValidateContextDataManagerSignature(object[] values, string signature, int contextCoupon)
        {
            StringBuilder encryptedByManager = new StringBuilder();
            foreach (var value in values)
            {
                encryptedByManager.Append(value);
            }
            encryptedByManager.Append(contextCoupon);

            byte[] managerData = String2Data(encryptedByManager.ToString());
            byte[] managerSignature = ReverseSignature(HexString2ByteArray(signature));
            return _managerCryptoProvider.VerifyData(managerData, "MD5", managerSignature);
        }

        private static string ByteArray2HexString(byte[] data)
        {
            var sb = new StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("x2"));
            return sb.ToString();
        }
        private static byte[] ComputeHash(string data)
        {
            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            return hashProvider.ComputeHash(String2Data(data));
        }
        private static byte[] String2Data(string data)
        {
            Encoding dataEncoding = new UnicodeEncoding();
            return dataEncoding.GetBytes(data);
        }
        private static byte[] HexString2ByteArray(string hex)
        {
            var result = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length / 2; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }
        
        /// <summary>
        /// Windows CryptoAPI and CCOW Standard use signatures with the byte order, reversed 
        /// </summary>
        /// <returns></returns>
        private byte[] ReverseSignature(byte[] signature)
        {
            var reversedSign = new byte[signature.Length];
            for (int i = 0; i < signature.Length; i++)
                reversedSign[i] = signature[signature.Length - i - 1];
            return reversedSign;
        }
        #endregion
    }
}
