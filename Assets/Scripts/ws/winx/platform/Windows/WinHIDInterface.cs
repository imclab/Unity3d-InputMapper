//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using ws.winx.devices;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Text;


namespace ws.winx.platform.windows
{
    public class WinHIDInterface : IHIDInterface
    {




        #region Fields
        private List<IJoystickDriver> __drivers;// = new List<IJoystickDriver>();


        private IJoystickDriver __defaultJoystickDriver;

        JoystickDevicesCollection _joysticks;

        public readonly Dictionary<IJoystickDevice, IHIDDeviceInfo> DeviceHIDInfos;

        delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);


        private const int ERROR_CLASS_ALREADY_EXISTS = 1410;

        //GUID_DEVINTERFACE_HID	Class GUID{4D1E55B2-F16F-11CF-88CB-001111000030}
        private static readonly Guid GUID_DEVINTERFACE_HID = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030"); // HID devices

        public IntPtr receiverWindowHandle;




        private WndProc m_wnd_proc_delegate;

        /// <summary>
        ///     Class of devices. This structure is a DEV_BROADCAST_DEVICEINTERFACE structure.
        /// </summary>
        public const uint DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;

        public const int DBT_DEVICEARRIVAL = 0x8000; // system detected a new device        
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // device is gone      
        public const int WM_DEVICECHANGE = 0x0219; // device change event  
        public const int ERROR_SUCCESS = 0;

        private static IntPtr notificationHandle;

        public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        public static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);
        public const int KEY_READ = 0x20019;
        public const string REGSTR_VAL_JOYOEMNAME = "OEMName";


        #endregion


        #region IHIDInterface implementation


        /// <summary>
        /// add or get default driver (Overall driver for unhanlded devices by other specialized driver)
        /// </summary>
        public IJoystickDriver defaultDriver
        {
            get { if (__defaultJoystickDriver == null) { __defaultJoystickDriver = new WinMMDriver(); } return __defaultJoystickDriver; }
            set { __defaultJoystickDriver = value; }

        }




        IDeviceCollection IHIDInterface.Devices
        {

            get { return _joysticks; }

        }






        public void Update()
        {
            Enumerate();
        }

        #endregion

        //public static readonly Guid GUID_DEVCLASS_HIDCLASS = new Guid(0x745a17a0, 0x74d3, 0x11d0, 0xb6, 0xfe, 0x00, 0xa0, 0xc9, 0x0f, 0x57, 0xda);
        //public static readonly Guid GUID_DEVCLASS_USB = new Guid(0x36fc9e60, 0xc465, 0x11cf, 0x80, 0x56, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);



        //private static readonly Guid GuidDevinterfaceUSBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices




        #region Constructor
        public WinHIDInterface(List<IJoystickDriver> drivers)
        {
            __drivers = drivers;
            _joysticks = new JoystickDevicesCollection();
            DeviceHIDInfos = new Dictionary<IJoystickDevice, IHIDDeviceInfo>();

            //Timer aTimer = new Timer(3000);
            // aTimer.Elapsed += new ElapsedEventHandler(enumerateTimedEvent);
            //  aTimer.Enabled = true;

            Enumerate();


            receiverWindowHandle = CreateReceiverWnd();

            if (receiverWindowHandle != IntPtr.Zero)
                RegisterHIDDeviceNotification(receiverWindowHandle);
        }
        #endregion


        // Specify what you want to happen when the Elapsed event is raised.
        //private void enumerateTimedEvent(object source, ElapsedEventArgs e)
        //{
        //    Update();

        //}




        /// <summary>
        /// Registers a window to receive notifications when USB devices are plugged or unplugged.
        /// </summary>
        /// <param name="windowHandle">Handle to the window receiving notifications.</param>
        public void RegisterHIDDeviceNotification(IntPtr windowHandle)
        {
            DEV_BROADCAST_DEVICEINTERFACE dbi = new DEV_BROADCAST_DEVICEINTERFACE
            {
                dbcc_size = 0,

                dbcc_devicetype = (int)DBT_DEVTYP_DEVICEINTERFACE,

                dbcc_reserved = 0,

                dbcc_classguid = GUID_DEVINTERFACE_HID.ToByteArray()

            };




            dbi.dbcc_size = Marshal.SizeOf(dbi);
            IntPtr buffer = Marshal.AllocHGlobal(dbi.dbcc_size);
            Marshal.StructureToPtr(dbi, buffer, true);

            notificationHandle = UnsafeNativeMethods.RegisterDeviceNotification(windowHandle, buffer, 0);


        }


        /// <summary>
        /// Creates window that would receive plug in/out device events
        /// </summary>
        /// <returns></returns>
        IntPtr CreateReceiverWnd()
        {

            IntPtr wndHnd = IntPtr.Zero;
            m_wnd_proc_delegate = CustomWndProc;

            // Create WNDCLASS
            WNDCLASS wind_class = new WNDCLASS();
            wind_class.lpszClassName = "InputManager Device Change Notification Reciver Wnd";
            wind_class.lpfnWndProc = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(m_wnd_proc_delegate);

            UInt16 class_atom = UnsafeNativeMethods.RegisterClassW(ref wind_class);

            int last_error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();

            if (class_atom == 0 && last_error != ERROR_CLASS_ALREADY_EXISTS)
            {
                Exception e = new System.Exception("Could not register window class");

                UnityEngine.Debug.LogException(e);

                return IntPtr.Zero;
            }


            try
            {
                // Create window
                wndHnd = UnsafeNativeMethods.CreateWindowExW(
                    0,
                    wind_class.lpszClassName,
                    String.Empty,
                    0,
                    0,
                    0,
                    0,
                    0,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero
                    );
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }


            return wndHnd;

        }

        /// <summary>
        /// Custom receiver window procedure where WM_MESSAGES are handled (WM_DEVICECHANGE)
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        protected IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            int devType = 0;

            if (msg == WM_DEVICECHANGE)
            {

                if (lParam != IntPtr.Zero)
                    devType = Marshal.ReadInt32(lParam, 4);

                switch ((int)wParam)
                {
                    case DBT_DEVICEREMOVECOMPLETE:

                        if (devType == WinHIDInterface.DBT_DEVTYP_DEVICEINTERFACE)
                        {
                            try
                            {
                                string name = String.Empty;
                                HIDDeviceInfo deviceInfo = GetHIDDeviceInfo(PointerToDevicePath(lParam));

                                IJoystickDevice device = _joysticks[new IntPtr(deviceInfo.PID)];
                                if (device != null)
                                {
                                    name = device.Name;
                                    this._joysticks.Remove(new IntPtr(deviceInfo.PID));
                                    this.DeviceHIDInfos.Remove(device);

                                    UnityEngine.Debug.Log("WinHIDInterface: " + name + " Removed");
                                }



                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogException(e);
                            }
                        }







                        break;
                    case DBT_DEVICEARRIVAL:
                        if (devType == WinHIDInterface.DBT_DEVTYP_DEVICEINTERFACE)
                        {
                            try
                            {



                                HIDDeviceInfo deviceInfo = GetHIDDeviceInfo(PointerToDevicePath(lParam));

                                string name = ReadRegKey(HKEY_CURRENT_USER, @"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\VID_" + deviceInfo.VID.ToString("X4") + "&PID_" + deviceInfo.PID.ToString("X4"), REGSTR_VAL_JOYOEMNAME);



                                UnityEngine.Debug.Log("WinHIDInterface: " + name + " Connected");

                                ResolveDevice(deviceInfo);
                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogException(e);
                            }
                        }

                        break;
                }
            }

            return UnsafeNativeMethods.DefWindowProcW(hWnd, msg, wParam, lParam);
        }



        /// <summary>
        /// Convert (WM_DEVICECHANGE)WM_MESSAGE pointer to data structure
        /// </summary>
        /// <param name="lParam"></param>
        /// <returns></returns>
        protected string PointerToDevicePath(IntPtr lParam)
        {
            DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface =
                               new DEV_BROADCAST_DEVICEINTERFACE();
            DEV_BROADCAST_HDR devBroadcastHeader = new DEV_BROADCAST_HDR();
            Marshal.PtrToStructure(lParam, devBroadcastHeader);

            Int32 stringSize = Convert.ToInt32((devBroadcastHeader.dbch_size - 32) / 2);
            Array.Resize(ref devBroadcastDeviceInterface.dbcc_name, stringSize);
            Marshal.PtrToStructure(lParam, devBroadcastDeviceInterface);
            return new String(devBroadcastDeviceInterface.dbcc_name, 0, stringSize);
        }


        /// <summary>
        /// Unregisters the window for USB device notifications
        /// </summary>
        public static void UnregisterHIDDeviceNotification()
        {
            if (notificationHandle != IntPtr.Zero)
                UnsafeNativeMethods.UnregisterDeviceNotification(notificationHandle);

            notificationHandle = IntPtr.Zero;
        }








        void Enumerate()
        {


            uint deviceCount = 0;
            var deviceSize = (uint)Marshal.SizeOf(typeof(RawInputDeviceList));

            // first call retrieves the number of raw input devices
            var result = UnsafeNativeMethods.GetRawInputDeviceList(
                IntPtr.Zero,
                ref deviceCount,
                deviceSize);



            if ((int)result == -1)
            {
                // call failed, 
                UnityEngine.Debug.LogError("WinHIDInterface failed to enumerate devices");

                return;
            }
            else if (deviceCount == 0)
            {
                // call failed, 
                UnityEngine.Debug.LogError("WinHIDInterface found no HID devices");

                return;
            }






            // allocates memory for an array of Win32.RawInputDeviceList
            IntPtr ptrDeviceList = Marshal.AllocHGlobal((int)(deviceSize * deviceCount));

            result = UnsafeNativeMethods.GetRawInputDeviceList(
                ptrDeviceList,
                ref deviceCount,
                deviceSize);



            if ((int)result != -1)
            {
                RawInputDeviceList rawInputDeviceList;
                // enumerates array of Win32.RawInputDeviceList,
                // and populates array of managed RawInputDevice objects
                for (var index = 0; index < deviceCount; index++)
                {

                    rawInputDeviceList = (RawInputDeviceList)Marshal.PtrToStructure(
                        new IntPtr((ptrDeviceList.ToInt32() +
                                (deviceSize * index))),
                        typeof(RawInputDeviceList));



                    if (rawInputDeviceList.DeviceType == RawInputDeviceType.HumanInterfaceDevice)
                        ResolveDevice(GetHIDDeviceInfo(rawInputDeviceList));

                }
            }

            Marshal.FreeHGlobal(ptrDeviceList);

        }



        /// <summary>
        /// Get Value of the Registry Key
        /// </summary>
        /// <param name="rootKey"></param>
        /// <param name="keyPath"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        private string ReadRegKey(UIntPtr rootKey, string keyPath, string valueName)
        {
            UIntPtr hKey;

            if (UnsafeNativeMethods.RegOpenKeyEx(rootKey, keyPath, 0, KEY_READ, out hKey) == 0)
            {
                uint size = 1024;
                uint type;
                string keyValue = null;
                StringBuilder keyBuffer = new StringBuilder((int)size);

                if (UnsafeNativeMethods.RegQueryValueEx(hKey, valueName, 0, out type, keyBuffer, ref size) == 0)
                    keyValue = keyBuffer.ToString();

                UnsafeNativeMethods.RegCloseKey(hKey);

                return (keyValue);
            }

            return String.Empty;  // Return null if the value could not be read
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="devicePath"></param>
        /// <returns></returns>
        protected HIDDeviceInfo GetHIDDeviceInfo(string devicePath)
        {

            string[] Parts = devicePath.Split('#');

            if (Parts.Length >= 3)
            {
                // string DevType = Parts[0].Substring(Parts[0].IndexOf(@"?\") + 2);//HID
                string DeviceInstanceId = Parts[1];

                String[] VID_PID_Parts = DeviceInstanceId.Split('&');


                //if we need in later code expansion
                // string DeviceUniqueID = Parts[2];//{fas232fafs2345faf}



                // string RegPath = @"SYSTEM\CurrentControlSet\Enum\" + DevType + "\\" + DeviceInstanceId + "\\" + DeviceUniqueID;


                //return ReadRegKey(HKEY_LOCAL_MACHINE, RegPath, "FriendlyName")+ReadRegKey(HKEY_LOCAL_MACHINE, RegPath, "DeviceDesc");





                string name = ReadRegKey(HKEY_CURRENT_USER, @"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" + DeviceInstanceId, REGSTR_VAL_JOYOEMNAME);






                //!!! deviceHandle set to IntPtr.Zero (think not needed in widows)
                return new HIDDeviceInfo(_joysticks.Count,Convert.ToInt32(VID_PID_Parts[0].Replace("VID_", ""), 16), Convert.ToInt32(VID_PID_Parts[1].Replace("PID_", ""), 16), IntPtr.Zero, this, devicePath, name);
            }

            return null;
        }


        protected HIDDeviceInfo GetHIDDeviceInfo(RawInputDeviceList rawInputDeviceList)
        {



            DeviceInfo deviceInfo = GetDeviceInfo(rawInputDeviceList.DeviceHandle);
            string devicePath = GetDevicePath(rawInputDeviceList.DeviceHandle);

            string name = ReadRegKey(HKEY_CURRENT_USER, @"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" + "VID_" + deviceInfo.HIDInfo.VendorID.ToString("X4") + "&PID_" + deviceInfo.HIDInfo.ProductID.ToString("X4"), REGSTR_VAL_JOYOEMNAME);


            return new HIDDeviceInfo(_joysticks.Count,Convert.ToInt32(deviceInfo.HIDInfo.VendorID), Convert.ToInt32(deviceInfo.HIDInfo.ProductID), rawInputDeviceList.DeviceHandle, this, devicePath, name);

            //this have problems with   
            // return GetHIDDeviceInfo(GetDevicePath(rawInputDeviceList.DeviceHandle));
        }






        private static IntPtr GetDeviceData(IntPtr deviceHandle, RawInputDeviceInfoCommand command)
        {
            uint dataSize = 0;
            var ptrData = IntPtr.Zero;

            UnsafeNativeMethods.GetRawInputDeviceInfo(
                deviceHandle,
                command,
                ptrData,
                ref dataSize);

            if (dataSize == 0) return IntPtr.Zero;

            ptrData = Marshal.AllocHGlobal((int)dataSize);

            var result = UnsafeNativeMethods.GetRawInputDeviceInfo(
                deviceHandle,
                command,
                ptrData,
                ref dataSize);

            if (result == 0)
            {
                Marshal.FreeHGlobal(ptrData);
                return IntPtr.Zero;
            }

            return ptrData;
        }

        private static string GetDevicePath(IntPtr deviceHandle)
        {
            var ptrDeviceName = GetDeviceData(
                deviceHandle,
                RawInputDeviceInfoCommand.DeviceName);

            if (ptrDeviceName == IntPtr.Zero)
            {
                return string.Empty;
            }

            var deviceName = Marshal.PtrToStringAnsi(ptrDeviceName);
            Marshal.FreeHGlobal(ptrDeviceName);
            return deviceName;
        }

        private static DeviceInfo GetDeviceInfo(IntPtr deviceHandle)
        {
            var ptrDeviceInfo = GetDeviceData(
                deviceHandle,
                RawInputDeviceInfoCommand.DeviceInfo);

            if (ptrDeviceInfo == IntPtr.Zero)
            {
                return new DeviceInfo();
            }

            DeviceInfo deviceInfo = (DeviceInfo)Marshal.PtrToStructure(
                ptrDeviceInfo, typeof(DeviceInfo));

            Marshal.FreeHGlobal(ptrDeviceInfo);
            return deviceInfo;
        }




        /// <summary>
        /// Try to attach compatible driver based on device info
        /// </summary>
        /// <param name="deviceInfo"></param>
        protected void ResolveDevice(HIDDeviceInfo deviceInfo)
        {

            IJoystickDevice joyDevice = null;

            //loop thru drivers and attach the driver to device if compatible
            if (__drivers != null)
                foreach (var driver in __drivers)
                {
                    joyDevice = driver.ResolveDevice(deviceInfo);
                    if (joyDevice != null)
                    {
                        //do not allow duplicates
                         if(_joysticks.ContainsKey(new IntPtr(deviceInfo.PID))) return;

                        AddDeviceToHIDInterface(joyDevice, deviceInfo);
                        Debug.Log("Device PID:" + deviceInfo.PID + " VID:" + deviceInfo.VID + " attached to " + driver.GetType().ToString());

                        break;
                    }
                }

            if (joyDevice == null)
            {//set default driver as resolver if no custom driver match device
                joyDevice = defaultDriver.ResolveDevice(deviceInfo);


                if (joyDevice != null)
                {
                    //do not allow duplicates
                    if (_joysticks.ContainsKey(new IntPtr(deviceInfo.PID))) return;

                    AddDeviceToHIDInterface(joyDevice, deviceInfo);

                    Debug.Log("Device PID:" + deviceInfo.PID + " VID:" + deviceInfo.VID + " attached to " + __defaultJoystickDriver.GetType().ToString() + " Path:" + deviceInfo.DevicePath + " Name:" + joyDevice.Name);

                }
                else
                {
                    Debug.LogWarning("Device PID:" + deviceInfo.PID + " VID:" + deviceInfo.VID + " not found compatible driver thru WinHIDInterface!");

                }

            }


        }

        private void AddDeviceToHIDInterface(IJoystickDevice joyDevice, HIDDeviceInfo deviceInfo)
        {
           
            _joysticks[new IntPtr(deviceInfo.PID)] = joyDevice;

            DeviceHIDInfos[joyDevice] = deviceInfo;
            joyDevice.Name = ReadRegKey(HKEY_CURRENT_USER, @"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" + "VID_" + deviceInfo.VID.ToString("X4") + "&PID_" + deviceInfo.PID.ToString("X4"), REGSTR_VAL_JOYOEMNAME);

        }









        #region Structures

        public enum RawInputDeviceType : uint
        {
            Mouse = 0,
            Keyboard = 1,
            HumanInterfaceDevice = 2
        }

        public enum RawInputDeviceInfoCommand : uint
        {
            PreparsedData = 0x20000005,
            DeviceName = 0x20000007,
            DeviceInfo = 0x2000000b,
        }






        [StructLayout(LayoutKind.Explicit)]
        public struct DeviceInfo
        {
            [FieldOffset(0)]
            public int Size;
            [FieldOffset(4)]
            public int Type;
            [FieldOffset(8)]
            public DeviceInfoMouse MouseInfo;
            [FieldOffset(8)]
            public DeviceInfoKeyboard KeyboardInfo;
            [FieldOffset(8)]
            public DeviceInfoHID HIDInfo;
        }

        public struct DeviceInfoMouse
        {
            public uint ID;
            public uint NumberOfButtons;
            public uint SampleRate;
        }

        public struct DeviceInfoKeyboard
        {
            public uint Type;
            public uint SubType;
            public uint KeyboardMode;
            public uint NumberOfFunctionKeys;
            public uint NumberOfIndicators;
            public uint NumberOfKeysTotal;
        }

        public struct DeviceInfoHID
        {
            public uint VendorID;
            public uint ProductID;
            public uint VersionNumber;
            public ushort UsagePage;
            public ushort Usage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RawInputDeviceList
        {
            public IntPtr DeviceHandle;
            public RawInputDeviceType DeviceType;
        }


        // Struct for parameters of the WM_DEVICECHANGE message
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_VOLUME
        {
            public int dbcv_size;
            public int dbcv_devicetype;
            public int dbcv_reserved;
            public int dbcv_unitmask;
        }



        [StructLayout(LayoutKind.Sequential)]
        internal class DEV_BROADCAST_HDR
        {
            internal Int32 dbch_size;
            internal Int32 dbch_devicetype;
            internal Int32 dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class DEV_BROADCAST_DEVICEINTERFACE
        {
            internal Int32 dbcc_size;
            internal Int32 dbcc_devicetype;
            internal Int32 dbcc_reserved;
            [MarshalAs(UnmanagedType.ByValArray,
           ArraySubType = UnmanagedType.U1,
           SizeConst = 16)]
            internal Byte[] dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            internal Char[] dbcc_name;
        }




        [System.Runtime.InteropServices.StructLayout(
              System.Runtime.InteropServices.LayoutKind.Sequential,
              CharSet = System.Runtime.InteropServices.CharSet.Unicode
              )]
        public struct WNDCLASS
        {
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public string lpszClassName;
        }

        #endregion


        #region UnsafeNativeMethods

        public static class UnsafeNativeMethods
        {

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern int RegCloseKey(
                UIntPtr hKey);

            // This signature will not get an entire REG_BINARY value. It will stop at the first null byte.
            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
            public static extern int RegQueryValueEx(
                UIntPtr hKey,
                string lpValueName,
                int lpReserved,
                out uint lpType,
                System.Text.StringBuilder lpData,
                ref uint lpcbData);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
            public static extern int RegOpenKeyEx(
              UIntPtr hKey,
              string subKey,
              int ulOptions,
              int samDesired,
              out UIntPtr hkResult);



            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            public static extern System.UInt16 RegisterClassW(
                [System.Runtime.InteropServices.In] ref WNDCLASS lpWndClass
                );

            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr CreateWindowExW(
                 UInt32 dwExStyle,
                 [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			string lpClassName,
                 [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			string lpWindowName,
                 UInt32 dwStyle,
                 Int32 x,
                 Int32 y,
                 Int32 nWidth,
                 Int32 nHeight,
                 IntPtr hWndParent,
                 IntPtr hMenu,
                 IntPtr hInstance,
                 IntPtr lpParam
                 );

            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            public static extern System.IntPtr DefWindowProcW(
                IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam
                );

            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyWindow(
                IntPtr hWnd
                );


            [DllImport("kernel32.dll")]
            public static extern uint GetLastError();
            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32", EntryPoint = "SetWindowsHookEx")]
            public static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hmod, IntPtr dwThreadId);

            [DllImport("user32", EntryPoint = "UnhookWindowsHookEx")]
            public static extern int UnhookWindowsHookEx(IntPtr hHook);

            [DllImport("user32", EntryPoint = "CallNextHookEx")]
            public static extern int CallNextHook(IntPtr hHook, int ncode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetCurrentThreadId();

            [DllImport("user32.dll")]
            public static extern System.IntPtr GetActiveWindow();

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

            [DllImport("user32.dll")]
            public static extern bool UnregisterDeviceNotification(IntPtr handle);




            [DllImport("User32.dll", SetLastError = true)]
            public static extern uint GetRawInputDeviceList(
                IntPtr pRawInputDeviceList,
                ref uint uiNumDevices,
                uint cbSize);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint GetRawInputDeviceInfo(
                IntPtr hDevice,
                RawInputDeviceInfoCommand uiCommand,
                IntPtr data,
                ref uint size);

        }





        #endregion


        #region IntPtrEqualityComparer
        // Simple equality comparer to allow IntPtrs as keys in dictionaries
        // without causing boxing/garbage generation.
        // Seriously, Microsoft, shouldn't this have been in the BCL out of the box?
        class IntPtrEqualityComparer : IEqualityComparer<IntPtr>
        {
            public bool Equals(IntPtr x, IntPtr y)
            {
                return x == y;
            }

            public int GetHashCode(IntPtr obj)
            {
                return obj.GetHashCode();
            }
        }
        #endregion




        #region JoystickDevicesCollection

        /// <summary>
        /// Defines a collection of JoystickAxes.
        /// </summary>
        public sealed class JoystickDevicesCollection : IDeviceCollection
        {
            #region Fields
            readonly Dictionary<IntPtr, IJoystickDevice> JoystickDevices;
            // readonly Dictionary<IntPtr, IJoystickDevice<IAxisDetails, IButtonDetails, IDeviceExtension>> JoystickDevices;

            readonly Dictionary<int, IntPtr> JoystickIDToDevice;


            List<IJoystickDevice> _iterationCacheList;//
            bool _isEnumeratorDirty = true;

            #endregion

            #region Constructors

            internal JoystickDevicesCollection()
            {
                JoystickDevices = new Dictionary<IntPtr, IJoystickDevice>(new IntPtrEqualityComparer());
              
                JoystickIDToDevice = new Dictionary<int, IntPtr>();

            }

            #endregion

#region Public Members

            public IJoystickDevice FindBy(int pid)
            {
                return JoystickDevices.Where(z => z.Value.PID == pid).FirstOrDefault().Value;
            }


#region IDeviceCollection implementation

            public void Remove(IntPtr device)
            {
                JoystickIDToDevice.Remove(JoystickDevices[device].ID);
                JoystickDevices.Remove(device);

                _isEnumeratorDirty = true;
            }


            public void Remove(int inx)
            {
                IntPtr device = JoystickIDToDevice[inx];
                JoystickIDToDevice.Remove(inx);
                JoystickDevices.Remove(device);

                _isEnumeratorDirty = true;
            }




            public IJoystickDevice this[int ID]
            //public IJoystickDevice<IAxisDetails, IButtonDetails, IDeviceExtension> this[int index]
            {
                get { return JoystickDevices[JoystickIDToDevice[ID]]; }
                //				internal set { 
                //
                //							JoystickIndexToDevice [JoystickDevices.Count]=
                //							JoystickDevices[]
                //						}
            }



            public IJoystickDevice this[IntPtr pidPointer]
            {
                get { return JoystickDevices[pidPointer]; }
                internal set
                {
                    JoystickIDToDevice[value.ID] = pidPointer;
                    JoystickDevices[pidPointer] = value;

                    _isEnumeratorDirty = true;

                }
            }


            public bool ContainsKey(int key)
            {
                return JoystickIDToDevice.ContainsKey(key);
            }

            public bool ContainsKey(IntPtr key)
            {
                return JoystickDevices.ContainsKey(key);
            }

			public void Clear(){
				JoystickIDToDevice.Clear();
				JoystickDevices.Clear();
			}

            public System.Collections.IEnumerator GetEnumerator()
            {
                if (_isEnumeratorDirty)
                {
                    _iterationCacheList = JoystickDevices.Values.ToList<IJoystickDevice>();
                    _isEnumeratorDirty = false;


                }

                return _iterationCacheList.GetEnumerator();

            }


            /// <summary>
            /// Gets a System.Int32 indicating the available amount of JoystickDevices.
            /// </summary>
            public int Count
            {
                get { return JoystickDevices.Count; }
            }

            #endregion

            #endregion







        }
        #endregion;






        public void Dispose()
        {
            UnityEngine.Debug.Log("Try to dispose notificationHandle");
            UnregisterHIDDeviceNotification();

            UnityEngine.Debug.Log("Try to dispose receiverWindowHandle");


            if (receiverWindowHandle != IntPtr.Zero)
            {
                UnityEngine.Debug.Log("Destroy Receiver" + UnsafeNativeMethods.DestroyWindow(receiverWindowHandle));
                receiverWindowHandle = IntPtr.Zero;

                //
            }

            _joysticks.Clear();
            DeviceHIDInfos.Clear();

            if(__drivers!=null)
            __drivers.Clear();

            UnityEngine.Debug.Log("Dispose WinHIDInterface");
        }


    }

}

#endif