namespace FeliCaAccessPlugIn
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class IObjectSafetyTLB : IObjectSafety
    {
        // ---------------------------------------------------
        // 定数
        // ---------------------------------------------------
        private const int INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x00000001;
        private const int INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x00000002;
        private const int INTERFACE_USES_DISPEX = 0x00000004;
        private const int INTERFACE_USES_SECURITY_MANAGER = 0x00000008;
        private const uint S_OK = 0x00000000;
        private const uint E_NOINTERFACE = 0x80004002;
        private const uint E_FAIL = 0x80004005;

        // ---------------------------------------------------
        // メンバ関数
        // ---------------------------------------------------
        public uint GetInterfaceSafetyOptions(ref Guid riid, ref int pdwSupportedOptions, ref int pdwEnabledOptions)
        {
            return S_OK;
        }

        public uint SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions)
        {
            return S_OK;
        }
    }
}
