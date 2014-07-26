//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
using System;
using System.Collections.Generic;
using ws.winx.devices;
using System.Runtime.InteropServices;
using System.Linq;


namespace ws.winx.platform.osx
{

    using Carbon;
    using CFAllocatorRef = System.IntPtr;
    using CFDictionaryRef = System.IntPtr;
    using CFArrayRef = System.IntPtr;
    using CFIndex = System.Int32;
    using CFRunLoop = System.IntPtr;
    using CFString = System.IntPtr;
    using CFStringRef = System.IntPtr; // Here used interchangeably with the CFString
    using CFTypeRef = System.IntPtr;
    using IOHIDDeviceRef = System.IntPtr;
    using IOHIDElementRef = System.IntPtr;
    using IOHIDManagerRef = System.IntPtr;
    using IOHIDValueRef = System.IntPtr;
    using IOOptionBits = System.IntPtr;
    using IOReturn = System.IntPtr;
    using CFNumberRef = System.IntPtr;
    using IOHIDElementCookie = System.UInt32;
    using UnityEngine; /*System.UIntPtr*/



    sealed class OSXHIDInterface : IHIDInterface
    {

#region Fields

        readonly IOHIDManagerRef hidmanager;


        readonly CFRunLoop RunLoop = CF.CFRunLoopGetMain();
        readonly CFString InputLoopMode = CF.RunLoopModeDefault;
        readonly CFArray DeviceTypes;

        NativeMethods.IOHIDDeviceCallback HandleDeviceAdded;
        NativeMethods.IOHIDDeviceCallback HandleDeviceRemoved;
        NativeMethods.IOHIDValueCallback HandleDeviceValueReceived;

        bool disposed;


        private List<IJoystickDriver> __drivers = new List<IJoystickDriver>();



        private IJoystickDriver __defaultJoystickDriver;

        JoystickDevicesCollection _joysticks;



        #endregion

#region IHIDInterface implementation
        public IJoystickDriver defaultDriver
        {
            get { if (__defaultJoystickDriver == null) { __defaultJoystickDriver = new OSXDriver(); } return __defaultJoystickDriver; }
            set { __defaultJoystickDriver = value; }

        }




        public IDeviceCollection Devices
        {

            get { return _joysticks; }

        }





        public void Update()
        {
        }

        #endregion



#region Contsructor

        public OSXHIDInterface(List<IJoystickDriver> drivers)
        {
            __drivers = drivers;


            HandleDeviceAdded = DeviceAdded;
            HandleDeviceRemoved = DeviceRemoved;
          
            CFDictionaryRef[] dictionaries;
            CFStringRef[] keys;



            keys = new CFStringRef[2];
            dictionaries = new CFDictionaryRef[3];

            keys[0] = NativeMethods.IOHIDDeviceUsagePageKey;
            keys[1] = NativeMethods.IOHIDDeviceUsageKey;

            //create 3 search patterns by Joystick,GamePad and MulitAxisController


            dictionaries[0] = CF.CFDictionaryCreate(IntPtr.Zero, keys, new IntPtr[] { new IntPtr((int)HIDPage.GenericDesktop), new IntPtr((int)HIDUsageGD.Joystick) }, 2, IntPtr.Zero, IntPtr.Zero);
            dictionaries[1] = CF.CFDictionaryCreate(IntPtr.Zero, keys, new IntPtr[] { new IntPtr((int)HIDPage.GenericDesktop), new IntPtr((int)HIDUsageGD.GamePad) }, 2, IntPtr.Zero, IntPtr.Zero);
            dictionaries[2] = CF.CFDictionaryCreate(IntPtr.Zero, keys, new IntPtr[] { new IntPtr((int)HIDPage.GenericDesktop), new IntPtr((int)HIDUsageGD.MultiAxisController) }, 2, IntPtr.Zero, IntPtr.Zero);


            DeviceTypes.Ref = CF.CFArrayCreate(IntPtr.Zero, dictionaries, 3, IntPtr.Zero);

            //create Hid manager	
            hidmanager = NativeMethods.IOHIDManagerCreate(IntPtr.Zero, IntPtr.Zero);

            //Register add/remove device handlers
            RegisterHIDCallbacks(hidmanager);

            _joysticks = new JoystickDevicesCollection();



        }
        #endregion

#region Private Members



        // Registers callbacks for device addition and removal. These callbacks
        // are called when we run the loop in CheckDevicesMode
        void RegisterHIDCallbacks(IOHIDManagerRef hidmanager)
        {
            NativeMethods.IOHIDManagerRegisterDeviceMatchingCallback(
                hidmanager, HandleDeviceAdded, IntPtr.Zero);
            NativeMethods.IOHIDManagerRegisterDeviceRemovalCallback(
                hidmanager, HandleDeviceRemoved, IntPtr.Zero);
            NativeMethods.IOHIDManagerScheduleWithRunLoop(hidmanager,
                                                          RunLoop, InputLoopMode);

            //NativeMethods.IOHIDManagerSetDeviceMatching(hidmanager, DeviceTypes.Ref);
            NativeMethods.IOHIDManagerSetDeviceMatchingMultiple(hidmanager, DeviceTypes.Ref);
            NativeMethods.IOHIDManagerOpen(hidmanager, IOOptionBits.Zero);

            CF.CFRunLoopRunInMode(InputLoopMode, 0.0, true);
            //OpenTK.Platform.MacOS.Carbon.CF.CFRunLoopRunInMode(InputLoopMode, 0.0, true);
        }


        /// <summary>
        /// Devices the added.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="res">Res.</param>
        /// <param name="sender">Sender.</param>
        /// <param name="device">Device.</param>
        void DeviceAdded(IntPtr context, IOReturn res, IntPtr sender, IOHIDDeviceRef device)
        {
            if (NativeMethods.IOHIDDeviceOpen(device, IOOptionBits.Zero) == IOReturn.Zero

               && !Devices.ContainsKey(device))
            {

                IntPtr vendor_id = NativeMethods.IOHIDDeviceGetProperty(device, NativeMethods.IOHIDVendorIDKey);
                IntPtr product_id = NativeMethods.IOHIDDeviceGetProperty(device, NativeMethods.IOHIDProductIDKey);
                string description = NativeMethods.IOHIDDeviceGetProperty(device, NativeMethods.IOHIDProductKey).ToString();




                IJoystickDevice joyDevice = null;
               // IJoystickDevice<IAxisDetails, IButtonDetails, IDeviceExtension> joyDevice = null;


                //loop thru drivers and attach the driver to device if compatible
                foreach (var driver in __drivers)
                {
                    if ((joyDevice = driver.ResolveDevice(new HIDDeviceInfo(_joysticks.Count,vendor_id.ToInt32(), product_id.ToInt32(), device, this, ""))) != null)
                    {
                        _joysticks[device] = joyDevice;
                        joyDevice.Name=description;

                        break;
                    }
                }

                if (joyDevice == null)
                {//set default driver as resolver if no custom driver match device

                    joyDevice = defaultDriver.ResolveDevice(new HIDDeviceInfo(_joysticks.Count,vendor_id.ToInt32(), product_id.ToInt32(), device, this, ""));//always return true
					joyDevice.Name=description;

					if (joyDevice != null)
                    {
                        _joysticks[device] = joyDevice;
                       
                    }
                        else
				    {
                        Debug.LogWarning("Device PID:" + product_id.ToInt32().ToString() + " VID:" + product_id.ToInt32().ToString() + " not found compatible driver on the system.Removed!");
                    
                    }

                }



            }
        }


        /// <summary>
        /// Devices the removed.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="res">Res.</param>
        /// <param name="sender">Sender.</param>
        /// <param name="device">Device.</param>
        void DeviceRemoved(IntPtr context, IOReturn res, IntPtr sender, IOHIDDeviceRef device)
        {
            if (NativeMethods.IOHIDDeviceConformsTo(device, HIDPage.GenericDesktop, (int)HIDUsageGD.Joystick)
                 || NativeMethods.IOHIDDeviceConformsTo(device, HIDPage.GenericDesktop, (int)HIDUsageGD.GamePad)
                 || NativeMethods.IOHIDDeviceConformsTo(device, HIDPage.GenericDesktop, (int)HIDUsageGD.MultiAxisController)
             && Devices.ContainsKey(device))
            {
                UnityEngine.Debug.Log(String.Format("Joystick device {0:x} disconnected, sender is {1:x}", device, sender));
                Devices.Remove(device);

            }



            NativeMethods.IOHIDDeviceRegisterInputValueCallback(device, IntPtr.Zero, IntPtr.Zero);
            NativeMethods.IOHIDDeviceUnscheduleWithRunLoop(device, RunLoop, InputLoopMode);
        }


        #endregion






#region NativeMethods

        internal class NativeMethods
        {
            const string hid = "/System/Library/Frameworks/IOKit.framework/Versions/Current/IOKit";

            public static readonly CFString IOHIDVendorIDKey = CF.CFSTR("VendorID");
            public static readonly CFString IOHIDVendorIDSourceKey = CF.CFSTR("VendorIDSource");
            public static readonly CFString IOHIDProductIDKey = CF.CFSTR("ProductID");
            public static readonly CFString IOHIDVersionNumberKey = CF.CFSTR("VersionNumber");
            public static readonly CFString IOHIDManufacturerKey = CF.CFSTR("Manufacturer");
            public static readonly CFString IOHIDProductKey = CF.CFSTR("Product");
            public static readonly CFString IOHIDDeviceUsageKey = CF.CFSTR("DeviceUsage");
            public static readonly CFString IOHIDDeviceUsagePageKey = CF.CFSTR("DeviceUsagePage");
            public static readonly CFString IOHIDDeviceUsagePairsKey = CF.CFSTR("DeviceUsagePairs");

            [DllImport(hid)]
            public static extern IOHIDManagerRef IOHIDManagerCreate(
                CFAllocatorRef allocator, IOOptionBits options);

            // This routine will be called when a new (matching) device is connected.
            [DllImport(hid)]
            public static extern void IOHIDManagerRegisterDeviceMatchingCallback(
                IOHIDManagerRef inIOHIDManagerRef,
                IOHIDDeviceCallback inIOHIDDeviceCallback,
                IntPtr inContext);

            [DllImport(hid)]
            public static extern void IOHIDManagerRegisterDeviceMatchingCallback(
                IOHIDManagerRef inIOHIDManagerRef,
                IntPtr inIOHIDDeviceCallback,
                IntPtr inContext);

            // This routine will be called when a (matching) device is disconnected.
            [DllImport(hid)]
            public static extern void IOHIDManagerRegisterDeviceRemovalCallback(
                IOHIDManagerRef inIOHIDManagerRef,
                IOHIDDeviceCallback inIOHIDDeviceCallback,
                IntPtr inContext);

            [DllImport(hid)]
            public static extern void IOHIDManagerRegisterDeviceRemovalCallback(
                IOHIDManagerRef inIOHIDManagerRef,
                IntPtr inIOHIDDeviceCallback,
                IntPtr inContext);

            [DllImport(hid)]
            public static extern void IOHIDManagerScheduleWithRunLoop(
                IOHIDManagerRef inIOHIDManagerRef,
                CFRunLoop inCFRunLoop,
                CFString inCFRunLoopMode);

            [DllImport(hid)]
            public static extern void IOHIDManagerSetDeviceMatching(
                IOHIDManagerRef manager,
                CFDictionaryRef matching);


            [DllImport(hid)]
            public static extern void IOHIDManagerSetDeviceMatchingMultiple(
                IOHIDManagerRef manager,
                CFArrayRef multiple);

            [DllImport(hid)]
            public static extern CFArrayRef IOHIDDeviceCopyMatchingElements(
            IOHIDDeviceRef device,
            CFDictionaryRef matching,
            IOOptionBits options);

            [DllImport(hid)]
            public static extern uint IOHIDElementGetUsage(
                IOHIDElementRef element);


            [DllImport(hid)]
            public static extern IOReturn IOHIDManagerOpen(
                IOHIDManagerRef manager,
                IOOptionBits options);

            [DllImport(hid)]
            public static extern IOReturn IOHIDDeviceOpen(
                IOHIDDeviceRef manager,
                IOOptionBits opts);

            [DllImport(hid)]
            public static extern CFTypeRef IOHIDDeviceGetProperty(
                IOHIDDeviceRef device,
                CFStringRef key);


            [DllImport(hid)]
            public static extern IOHIDElementType IOHIDElementGetType(
                IOHIDElementRef element);

            [DllImport(hid)]
            public static extern bool IOHIDDeviceConformsTo(
                IOHIDDeviceRef inIOHIDDeviceRef,  // IOHIDDeviceRef for the HID device
                HIDPage inUsagePage,      // the usage page to test conformance with
                int inUsage);         // the usage to test conformance with

            [DllImport(hid)]
            public static extern void IOHIDDeviceRegisterInputValueCallback(
                IOHIDDeviceRef device,
                IOHIDValueCallback callback,
                IntPtr context);

            [DllImport(hid)]
            public static extern void IOHIDDeviceRegisterInputValueCallback(
                IOHIDDeviceRef device,
                IntPtr callback,
                IntPtr context);

            [DllImport(hid)]
            public static extern void IOHIDDeviceScheduleWithRunLoop(
                IOHIDDeviceRef device,
                CFRunLoop inCFRunLoop,
                CFString inCFRunLoopMode);


            [DllImport(hid)]
            public static extern bool IOHIDElementHasNullState(
                IOHIDElementRef element);

            [DllImport(hid)]
            public static extern void IOHIDDeviceUnscheduleWithRunLoop(
                IOHIDDeviceRef device,
                CFRunLoop inCFRunLoop,
                CFString inCFRunLoopMode);

            [DllImport(hid)]
            public static extern uint /*IOHIDElementCookie*/ IOHIDElementGetCookie(
                IOHIDElementRef element);

            [DllImport(hid)]
            public static extern IOHIDElementRef IOHIDValueGetElement(IOHIDValueRef @value);

            [DllImport(hid)]
            public static extern CFIndex IOHIDValueGetIntegerValue(IOHIDValueRef @value);

            [DllImport(hid)]
            public static extern double IOHIDValueGetScaledValue(
                IOHIDValueRef @value,
                IOHIDValueScaleType type);


            [DllImport(hid)]
            public static extern HIDPage IOHIDElementGetUsagePage(IOHIDElementRef elem);

            [DllImport(hid)]
            public static extern CFIndex IOHIDElementGetLogicalMin(IOHIDElementRef element);

            [DllImport(hid)]
            public static extern CFIndex IOHIDElementGetLogicalMax(IOHIDElementRef element);


            [DllImport(hid)]
            public static extern CFIndex IOHIDValueGetLength(IOHIDValueRef value);



            public delegate void IOHIDDeviceCallback(IntPtr ctx, IOReturn res, IntPtr sender, IOHIDDeviceRef device);
            public delegate void IOHIDValueCallback(IntPtr ctx, IOReturn res, IntPtr sender, IOHIDValueRef val);





        }

        internal enum IOHIDValueScaleType
        {
            Physical, // [device min, device max]
            Calibrated // [-1, +1]
        }

        internal enum HIDPage
        {
            GenericDesktop = 0x01,

        }

        // Generic desktop usage
        internal enum HIDUsageGD
        {

            Joystick = 0x04, /* Application Collection */
            GamePad = 0x05, /* Application Collection */
            MultiAxisController = 0x08, /* Application Collection */
            Hatswitch = 0x39, /* Dynamic Value */

        }

        internal enum IOHIDElementType
        {
            kIOHIDElementTypeInput_Misc = 1,
            kIOHIDElementTypeInput_Button = 2,
            kIOHIDElementTypeInput_Axis = 3,
            kIOHIDElementTypeInput_ScanCodes = 4,
            kIOHIDElementTypeOutput = 129,
            kIOHIDElementTypeFeature = 257,
            kIOHIDElementTypeCollection = 513
        };




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
            //readonly Dictionary<IntPtr, IJoystickDevice<IAxisDetails, IButtonDetails, IDeviceExtension>> JoystickDevices;

            //readonly List<IntPtr> JoystickIndexToDevice;
            readonly Dictionary<int, IntPtr> JoystickIDToDevice;

            List<IJoystickDevice> _iterationCacheList;
            bool _isEnumeratorDirty = true;

            #endregion

#region Constructors

            internal JoystickDevicesCollection()
            {
                JoystickDevices = new Dictionary<IntPtr, IJoystickDevice>(new IntPtrEqualityComparer());
                //JoystickDevices = new Dictionary<IntPtr, IJoystickDevice<IAxisDetails, IButtonDetails, IDeviceExtension>>(new IntPtrEqualityComparer());
               // JoystickIndexToDevice = new List<IntPtr>();
                JoystickIDToDevice = new Dictionary<int, IntPtr>();

				
            }

            #endregion

#region Public Members

#region IDeviceCollection implementation

            public void Remove(IntPtr device)
            {
                JoystickIDToDevice.Remove(JoystickDevices[device].ID);
                JoystickDevices.Remove(device);
            }


            public void Remove(int inx)
            {
                IntPtr device = JoystickIDToDevice[inx];
                JoystickIDToDevice.Remove(inx);
                JoystickDevices.Remove(device);
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

            public IJoystickDevice this[IntPtr device]
            //public IJoystickDevice<IAxisDetails, IButtonDetails, IDeviceExtension> this[IntPtr device]
            {
                get { return JoystickDevices[device]; }
                internal set
                {
                    JoystickIDToDevice[value.ID] = device;
                    JoystickDevices[device] = value;

                }
            }


			public bool ContainsKey (int key)
			{ 
              
				return JoystickIDToDevice.ContainsKey(key);
			}



            public bool ContainsKey(IntPtr key)
            {
                return JoystickDevices.ContainsKey(key);
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




        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

#endif