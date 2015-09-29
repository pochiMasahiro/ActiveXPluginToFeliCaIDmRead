/*
 * nfc_sample_01.cs
 * Copyright 2009,2011 Sony Corporation
 */
using System;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace FeliCaAccessPlugIn
{
    [Guid("7d972707-49c9-4813-8361-ebed3f7d0d88")]
    [ComSourceInterfaces(typeof(FelicaEvent))]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("FelicaAccessPlugIn.Access")]
    public class Access : IObjectSafetyTLB, FelicaMethod
    {
        public const Int32 EXIT_FAILED = 1;
        public const Int32 EXIT_SUCCEED = 0;
        public const Int32 BUFSIZ = 512;

        bool bRet = false;

        private static felica_nfc_dll_wrapper FeliCaNfcDllWrapperClass =
            new felica_nfc_dll_wrapper();

        public event DefaultEventHandler FelicaError;
        public event DefaultEventHandler NotifyIDm;

        public void Init()
        {
            String msg_str_of_find = "find";
            String msg_str_of_enable = "enable";

            FelicaError("Start\n");

            UInt32 card_find_message = RegisterWindowMessage(msg_str_of_find);
            if (card_find_message == 0)
            {
                FelicaError("Failed: RegisterWindowMessage\n");
                ErrorRoutine();
            }

            UInt32 card_enable_message = RegisterWindowMessage(msg_str_of_enable);
            if (card_enable_message == 0)
            {
                FelicaError("Failed: RegisterWindowMessage\n");
                ErrorRoutine();
            }

            MessageHandler messageHandler
                = new MessageHandler(card_find_message, card_enable_message);
            ListenerWindow lw = new ListenerWindow(messageHandler);

            lw.handler += new MessageReceivedEventHandler(messageHandler.messageHandlerFunc);

            bRet = lw.WatchMessage(card_find_message);
            if (bRet == false)
            {
                FelicaError("Failed: WatchMessage\n");
                ErrorRoutine();
            }

            bRet = lw.WatchMessage(card_enable_message);
            if (bRet == false)
            {
                FelicaError("Failed: WatchMessage\n");
                ErrorRoutine();
            }

            bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcInitialize();
            if (bRet == false)
            {
                FelicaError("Failed: FeliCaLibNfcInitialize\n");
                ErrorRoutine();
            }

            StringBuilder port_name = new StringBuilder("USB0");
            bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcOpen(port_name);
            if (bRet == false)
            {
                FelicaError("Failed: FeliCaLibNfcOpen\n");
                ErrorRoutine();
            }

            bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcSetPollCallbackParameters(
                lw.Handle,
                msg_str_of_find,
                msg_str_of_enable);
            if (bRet == false)
            {
                FelicaError("Failed: FeliCaLibNfcSetPollCallbackParameters\n");
                ErrorRoutine();
            }

            const Int32 DEVICE_TYPE_NFC_18092_212_424K = 0x00000006;
            UInt32 target_device = DEVICE_TYPE_NFC_18092_212_424K;
            bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcStartPollMode(target_device);
            if (bRet == false)
            {
                FelicaError("Failed: FeliCaLibNfcStartPollMode\n");
                ErrorRoutine();
            }

            FelicaError("MessageLoopStart\n");
            Application.Run(lw);

            if (messageHandler.bRet == false)
            {
                ErrorRoutine();
            }

            getIDm();

            UInt32 RE_NOTIFICATION_SAME_DEVICE = 0x00;
            UInt32 stop_mode = RE_NOTIFICATION_SAME_DEVICE;
            bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcStopDevAccess(stop_mode);
            if (bRet == false)
            {
                FelicaError("Failed: FeliCaLibNfcStopDevAccess\n");
                ErrorRoutine();
            }

            bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcStopPollMode();
            if (bRet == false)
            {
                FelicaError("Failed: FeliCaLibNfcStopPollMode\n");
                ErrorRoutine();
            }

            bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcClose();
            if (bRet == false)
            {
                FelicaError("Failed: FeliCaLibNfcClose\n");
                ErrorRoutine();
            }

            bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcUninitialize();
            if (bRet == false)
            {
                FelicaError("Failed: FeliCaLibNfcUninitialize\n");
                ErrorRoutine();
            }
            FelicaError("Finish\n");
        }

        [DllImport("User32.dll")]
        extern static UInt32 RegisterWindowMessage(String lpString);

        private bool getIDm()
        {
            byte[] command_packet_data = new byte[] { 0x06, 0x00, 0xff, 0xff, 0x00, 0x00 };	// Command for Mifare ultralight, &H30: 16byte Reading command, &H0: Start address
            UInt16 command_packet_data_length = 6;
            byte[] response_packet_data = new byte[512];
            UInt16 response_packet_data_length = 0x00;
            bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcThru(
                command_packet_data,
                command_packet_data_length,
                response_packet_data,
                ref response_packet_data_length);
            if (bRet == false)
            {
                FelicaError("Failed: FeliCaLibNfcThru\n");
                ErrorRoutine();
                return false;
            }

            String idm = null;
            for (Int32 idx = 2; idx < 10; idx++)
            {
                idm += response_packet_data[idx].ToString("X2");
            }
            NotifyIDm(idm);
            FelicaError("READ IDM");

            UInt32 RE_NOTIFICATION_SAME_DEVICE = 0x00;
            UInt32 stop_mode = RE_NOTIFICATION_SAME_DEVICE;
            bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcStopDevAccess(stop_mode);
            if (bRet == false)
            {
                FelicaError("Failed: FeliCaLibNfcStopDevAccess\n");
                ErrorRoutine();
                return false;
            }
            return true;
        }

        public void ErrorRoutine()
        {
            UInt32[] error_info = new UInt32[2] { 0, 0 };
            FeliCaNfcDllWrapperClass.FeliCaLibNfcGetLastError(error_info);
            FelicaError(String.Format("error_info[0]: 0x{0:X8}\nerror_info[1]: 0x{1:X8}\n", error_info[0], error_info[1]));

            FeliCaNfcDllWrapperClass.FeliCaLibNfcClose();
            FeliCaNfcDllWrapperClass.FeliCaLibNfcUninitialize();
            return;
        }
    }

    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
    public delegate void DefaultEventHandler(String message);

    public class MessageReceivedEventArgs : EventArgs
    {
        private readonly Message _message;
        public MessageReceivedEventArgs(Message message) { _message = message; }
        public Message Message { get { return _message; } }
    }

    public class ListenerWindow : Form
    {
        private const Int32 MAX_MESSAGES = 2;
        public event MessageReceivedEventHandler handler;
        private UInt32[] messageSet = new UInt32[MAX_MESSAGES];
        private Int32 registeredMessage = 0;
        private MessageHandler message;

        public ListenerWindow(MessageHandler _message)
        {
            message = _message;
        }

        public bool WatchMessage(UInt32 message)
        {
            if (registeredMessage < messageSet.Length)
            {
                messageSet[registeredMessage] = message;
                registeredMessage++;
                return true;
            }
            else
            {
                return false;
            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                const Int32 WS_EX_TOOLWINDOW = 0x80;
                const Int64 WS_POPUP = 0x80000000;
                const Int32 WS_VISIBLE = 0x10000000;
                const Int32 WS_SYSMENU = 0x80000;
                const Int32 WS_MAXIMIZEBOX = 0x10000;

                CreateParams cp = base.CreateParams;
                cp.ExStyle = WS_EX_TOOLWINDOW;
                cp.Style = unchecked((Int32)WS_POPUP) |
                    WS_VISIBLE | WS_SYSMENU | WS_MAXIMIZEBOX;
                cp.Width = 0;
                cp.Height = 0;

                return cp;
            }
        }
        protected override void WndProc(ref Message m)
        {
            bool handleMessage = false;
            for (Int32 i = 0; i < registeredMessage; i++)
            {
                if (messageSet[i] == m.Msg)
                {
                    handleMessage = true;
                }
            }

            if (handleMessage && handler != null)
            {
               handler(null, new MessageReceivedEventArgs(m));
            }
            base.WndProc(ref m);
            return;
        }
    }

    [ComSourceInterfaces(typeof(FelicaEvent))]
    [ClassInterface(ClassInterfaceType.None)]
    public class MessageHandler
    {
        public bool bRet;
        private UInt32 target_number;
        private UInt32 card_find_message;
        private UInt32 card_enable_message;
        private static felica_nfc_dll_wrapper FeliCaNfcDllWrapperClass =
            new felica_nfc_dll_wrapper();

        public event DefaultEventHandler FelicaError;
        public event DefaultEventHandler NotifyIDm;

        public MessageHandler(
            UInt32 findMsg,
            UInt32 enableMsg)
        {
            card_find_message = findMsg;
            card_enable_message = enableMsg;
        }

        public void messageHandlerFunc(object sender, MessageReceivedEventArgs e)
        {
            bRet = false;
            if (e.Message.Msg == card_find_message)
            {
                IntPtr pDevInfo = e.Message.LParam;
                IntPtr pDeviceData_A;
                if (IntPtr.Size == 8)
                {
                    pDeviceData_A = (IntPtr)((Int64)pDevInfo
                        + (Int64)Marshal.OffsetOf(typeof(DEVICE_INFO), "dev_info"));
                }
                else
                {
                    pDeviceData_A = (IntPtr)((Int32)pDevInfo
                        + (Int32)Marshal.OffsetOf(typeof(DEVICE_INFO), "dev_info"));
                }

                DEVICE_DATA_NFC_18092_212_424K DeviceData_A =
                    (DEVICE_DATA_NFC_18092_212_424K)
                    Marshal.PtrToStructure(pDeviceData_A,
                    typeof(DEVICE_DATA_NFC_18092_212_424K));

                target_number = DeviceData_A.target_number;
                bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcStartDevAccess(target_number);
                if (bRet == false)
                {
                    FelicaError("Failed: FeliCaLibNfcStartDevAccess\n");
                    Application.Exit();
                    return;
                }
            }
            else if (e.Message.Msg == card_enable_message)
            {
                Application.Exit();
                bRet = true;
                return;
            }
            return;
        }
    }
}
