﻿namespace FeliCaAccessPlugIn
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    [ComImport, Guid("CB5BDC81-93C1-11CF-8F20-00805F2CD064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IObjectSafety
    {
        [PreserveSig]
        uint GetInterfaceSafetyOptions(
                ref Guid riid,
                [MarshalAs(UnmanagedType.U4)] ref int pdwSupportedOptions,
                [MarshalAs(UnmanagedType.U4)] ref int pdwEnabledOptions);

        [PreserveSig]
        uint SetInterfaceSafetyOptions(
                ref Guid riid,
                [MarshalAs(UnmanagedType.U4)] int dwOptionSetMask,
                [MarshalAs(UnmanagedType.U4)] int dwEnabledOptions);
    }
}