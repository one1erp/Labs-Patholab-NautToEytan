using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NautToEytan.CCOWCOMInterfaces
{
   [ComVisible(true),Guid("F933331D-91C6-11D2-AB9F-4471FBC00000"),InterfaceType(ComInterfaceType.InterfaceIsDual)]
	internal interface ISecureBinding
	{
		string InitializeBinding(int bindeeCoupon, object propertyNames, object propertyValues, ref string binderPublicKey);
		object FinalizeBinding(int bindeeCoupon, string bindeePublicKey, string mac);
	}
}

