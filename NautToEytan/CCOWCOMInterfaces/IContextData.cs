using System.Runtime.InteropServices;

namespace NautToEytan.CCOWCOMInterfaces
{
    [
        ComVisible(true),
        Guid("2AAE4991-A1FC-11D0-808F-00A0240943E4"),
        InterfaceType(ComInterfaceType.InterfaceIsDual)
    ]
    internal interface IContextData
    {
        object GetItemNames(int contextCoupon);
        void DeleteItems(int participantCoupon, object names, int contextCoupon);
        void SetItemValues(int participantCoupon, object itemNames, object itemValues, int contextCoupon);
        object GetItemValues(object names, bool onlyChanges, int contextCoupon);
    }
}
