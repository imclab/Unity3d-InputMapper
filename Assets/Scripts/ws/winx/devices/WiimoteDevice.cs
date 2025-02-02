using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ws.winx.platform;
using ws.winx.drivers;
using System.Diagnostics;
using ws.winx.input;



namespace ws.winx.devices
{
    /// <summary>
    /// The extension plugged into the Wiimote
    /// </summary>
    //[DataContract]
    public enum WiiExtensionType : byte
    {
        /// <summary>
        /// No extension
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Nunchuk extension
        /// </summary>
        Nunchuk = 0xfe,
        /// <summary>
        /// Classic Controller extension
        /// </summary>
        ClassicController = 0xfd,

        // hmm...what's 0xfc?

        /// <summary>
        /// Guitar controller from Guitar Hero
        /// </summary>
        Guitar = 0xfb
    };


    /// <summary>
    /// The mode of data reported for the IR sensor
    /// </summary>
   // [DataContract]
    public enum IRMode : byte
    {
        /// <summary>
        /// IR sensor off
        /// </summary>
        Off = 0x00,
        /// <summary>
        /// Basic mode
        /// </summary>
        Basic = 0x01,	// 10 bytes
        /// <summary>
        /// Extended mode
        /// </summary>
        Extended = 0x03,	// 12 bytes
        /// <summary>
        /// Full mode (unsupported)
        /// </summary>
        Full = 0x05,	// 16 bytes * 2 (format unknown)
    };


    /// <summary>
    /// 
    /// </summary>
   public enum PassThruMode : byte
    {
        Noone = 0x04,
        Nunchuck = 0x05,
        ClassicController = 0x07,

    }


   public enum ExtensionType : byte
   {
       None = 0x00,
       Nunchuck = 0x01,
       ClassicController = 0x02,
       ClassicControllerPro = 0x04,
       Guitar = 0x08,
       Drums = 0x10,
       MotionPlus = 0x20,
       BalancedBoard = 0x40,
       TaikoDrums = 0x80
   }

 

  



   /// <summary>
   /// The extension number of the currently plugged into the Wiimote
   /// </summary>
   public enum ExtensionNumber : long
   {


       /// <summary>
       /// No extension
       /// </summary>
       None = 0x000000000000,
       /// <summary>
       /// Nunchuk extension
       /// </summary>
       Nunchuk = 0x0000a4200000,
       /// <summary>
       /// Classic Controller extension
       /// </summary>
       ClassicController = 0x0000a4200101,
       /// <summary>
       /// Guitar controller from Guitar Hero 3/WorldTour
       /// </summary>
       Guitar = 0x0000a4200103,
       /// <summary>
       /// Drum controller from Guitar Hero: World Tour
       /// </summary>
       Drums = 0x0100a4200103,
       /// <summary>
       /// Wii Fit Balance Board controller
       /// </summary>
       BalanceBoard = 0x0000a4200402,
       /// <summary>
       /// Taiko "TaTaCon" drum controller
       /// </summary>
       TaikoDrum = 0x0000a4200111,
       /// <summary>
       /// Wii MotionPlus extension
       /// </summary>
       MotionPlus = 0x0000a4200405,
       //MotionPlusInside has 0x0100 A420 0405
       //static const QWORD MOTION_PLUS		   = 0x050420A40000ULL;
       //	static const QWORD MOTION_PLUS_DETECT  = 0x050020a60000ULL;
       //	static const QWORD MOTION_PLUS_DETECT2 = 0x050420a60000ULL;
       /// <summary>
       /// Partially inserted extension.  This is an error condition.
       /// </summary>
       ParitallyInserted = 0xffffffffffff
   };


   



	public class WiimoteDevice:JoystickDevice,IDisposable
	{


        /// <summary>
        /// 
        /// </summary>
        public class MotionPlusCalibrationInfo
        {
            public Vector3 mMaxNoise;
            public List<Vector3> mNoise;
            public Vector3 mBias;
            public Vector3 mMinNoise;
            public float slowFactor = 500 / 8192f;//deg/unit  0.05f;//   500/8192f;//deg/unit
            public float fastFactor = 2000 / 8192f;//deg/unit 0.25f;// 2000/8192f ;//deg/unit

            public bool MotionPlusCalibrated;
            public bool MotionPlusCalibrating;

            // New calibration
            // Use of mNoise vector seems to accumulate error
            // TODO: find out why, fix, and use mNoise instead
            public int numCalibrationReadings;
            public double pitchSum;
            public double yawSum;
            public double rollSum;



            public double CalibrationTimeout;
            public Stopwatch watch;
            

            public Vector3 NoiseLevel;
            public Vector3 NoiseThreshold;
          
        }

        /// <summary>
        /// Current state of the MotionPlus controller
        /// </summary>

        public class MotionPlus
        {
            /// <summary>
            /// Calibration data for MontionPlus
            /// </summary>

            public MotionPlusCalibrationInfo CalibrationInfo;

            /// <summary>
            /// 
            /// </summary>
            public Vector3 RawValues;

            /// <summary>
            /// Normalized speed data
            /// <remarks>Values range between 0 - ?</remarks>
            /// </summary>

            public Vector3 Values;


            public bool Enabled = false;

            /// <summary>
            /// Yaw/Pitch/Roll rotating "quickly" (no definition for "quickly" yet...)
            /// </summary>
            public bool YawFast = false;
            public bool PitchFast = false;
            public bool RollFast = false;
            public bool DisableStatusEvent=false;





        }
      

        protected uint _rumbleBit;
        protected bool _rumble;
        protected WiiExtensionType _ExtensionType;

      

        protected bool HasMotionPlus
        {
            get { return _motionPlus != null; }
            
        }
        protected float _battery;
        protected IRMode _irmode;


        private const double NOISE_FILTER = 1.5;
        private const double CALIB_TIME = 5000;//ms


        

        //Interleave mode for use of M+ and Extension toghter
        private PassThruMode _mode = PassThruMode.Noone;

        public PassThruMode Mode
        {
            get { return _mode; }
            set { _mode = value;
            ((WiiDriver)driver).SetMode(this,value);
            }
        }


      
       

        public int numMPlusChecks = 0;

        public byte Extensions;
        
        #region IRSensor
        public class IRSensor{

            public IRSensor()
            {
                _found = false;
                _size=0;
                ir_axis_collection = new JoystickAxisCollection<IAxisDetails>(2);
            }

            protected int _size;
            protected bool _found;
            private JoystickAxisCollection<IAxisDetails> ir_axis_collection;


            public JoystickAxisCollection<IAxisDetails> Axis
            {
                get { return ir_axis_collection; }
            }

            public int Size
            {
              get { return _size; }
              set { _size = value; }
            }

            public bool Found
            {
              get { return _found; }
              set { _found = value; }
            }
        }

        #endregion

        protected IRSensor[] _IR_SENSORS;

        public IRSensor[] IR_SENSORS
        {
            get { return _IR_SENSORS; }
           
        }

        protected Vector2 _irPoint;

      
       

        protected bool[] _LED;

        public bool[] LED
        {
            get { return _LED; }
           
        }

  

        // sure, we could find this out the hard way using HID, but trust me, it's 22
        private const int REPORT_LENGTH = 22;

        public bool isAccCalibrated = false;



        protected MotionPlus _motionPlus;
        internal ExtensionNumber ExtensionForCalibration;

        public MotionPlus motionPlus
        {
            get { return _motionPlus; }
          
        }


        protected byte _DataReportType;

        public byte DataReportType
        {
            get { return _DataReportType; }
            internal set { _DataReportType = value; }
        }


        internal bool isInProccesingExtension = false;
      


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <param name="vid"></param>
        /// <param name="axes"></param>
        /// <param name="buttons"></param>
        /// <param name="leds"></param>
        /// <param name="irs"></param>
        /// <param name="driver"></param>
        public WiimoteDevice(int id,int pid,int vid, int axes, int buttons,int leds,int irs,IDriver driver)
            : base(id,pid,vid,axes,buttons,driver)
        {

            _IR_SENSORS = new IRSensor[irs];
            _irPoint = new Vector2();
                  
               for(int i=0;i<irs;i++)
                   IR_SENSORS[i]=new IRSensor();


              
            _LED = new bool[leds];

        }


     



        public IRMode irMode
        {
            get { return _irmode; }
            set { _irmode = value; }
        }


    


        public float Battery
        {
            get
            {
                return _battery;
            }
            set
            {
                _battery = value;


            }
        }


        public bool hasExtensionDevice
        {
            get
            {
                return Extensions != 0x0;
            }
           
        }


    

        public bool Rumble
        {
            get
            {
                return _rumble;
            }
            set
            {
                _rumble = value;

                _rumbleBit = _rumble ? (uint)0x1 : (uint)0x0;
            }
        }


       
        internal uint RumbleBit
        {
            get
            {
               return _rumbleBit;
            }
        }


        /// <summary>
        /// 
        /// </summary>
         internal void InitMotionPlus()
         {
             if (_motionPlus == null)
             {
                 _motionPlus = new MotionPlus();
                 _motionPlus.CalibrationInfo = new MotionPlusCalibrationInfo();
             }
             
         }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
         internal void UpdateMPlusCalibration(Vector3 values)
         {
             if (!_motionPlus.CalibrationInfo.MotionPlusCalibrating)
             {

                 UnityEngine.Debug.Log("M+ Calibrating..."); 
                 InitMPlusCalibration(values.x, values.y, values.z);


             }
             else
             {
                 _motionPlus.CalibrationInfo.mMaxNoise.x = Math.Max(_motionPlus.CalibrationInfo.mMaxNoise.x, values.x);
                 _motionPlus.CalibrationInfo.mMaxNoise.y = Math.Max(_motionPlus.CalibrationInfo.mMaxNoise.y, values.y);
                 _motionPlus.CalibrationInfo.mMaxNoise.z = Math.Max(_motionPlus.CalibrationInfo.mMaxNoise.z, values.z);
                 _motionPlus.CalibrationInfo.mMinNoise.x = Math.Min(_motionPlus.CalibrationInfo.mMinNoise.x, values.x);
                 _motionPlus.CalibrationInfo.mMinNoise.y = Math.Min(_motionPlus.CalibrationInfo.mMinNoise.y, values.y);
                 _motionPlus.CalibrationInfo.mMinNoise.z = Math.Min(_motionPlus.CalibrationInfo.mMinNoise.z, values.z);
             }




            // UnityEngine.Debug.Log(_motionPlus.CalibrationInfo.watch.ElapsedMilliseconds);




             if (_motionPlus.CalibrationInfo.CalibrationTimeout <= _motionPlus.CalibrationInfo.watch.ElapsedMilliseconds)
             {

                
             

                 calculateMPlusCalibration();


             }
             else
             {


                 _motionPlus.CalibrationInfo.pitchSum += values.x;
                 _motionPlus.CalibrationInfo.yawSum += values.z;
                 _motionPlus.CalibrationInfo.rollSum += values.y;

                 // Store the "reading" in mNoise
                 //mNoise.push_back(rates);
                 _motionPlus.CalibrationInfo.mNoise.Add(values);

               
             }

         }


        
         /// <summary>
		/// Gets the input.
        /// IMPORTANT: If button or axis is already "DOWN" returns 0
        /// GetInput is called in "edit mode" (when mapping in UI) and anykeydown for devices
        /// during combos process
		/// </summary>
		/// <returns>The input.</returns>
		 public override int GetInput(){
           // UnityEngine.Debug.Log("GetInput:" + Time.frameCount);

            //there is no  Time.frameCount in Editor (Edit mode - Editor scripts)
            if(Application.isPlaying) 
            {
                //prevents multiply update in frame
                Update();
                              
            }
            else  
                driver.Update(this);
            

          
					int num_axes = 8;//Acceleration Axis not need digital input
						
					int axis = 0;
					float joyValue=0f;
					float joyValueMax = 0f;
					
					IAxisDetails axisDetails;




                    /////////////////////////////////   HANDLE AXIS in DIGITAL MANNER //////////////////////////////

                    int dominantAxisInx = -1;

                    while (axis < num_axes)
                    {

                        if (axis < num_axes && (axisDetails = Axis[axis]) != null)
                        {

                            axisDetails = Axis[axis];
                            joyValue = axisDetails.value;

                            joyValue = joyValue < 0 ? -joyValue : joyValue;//abs

                            if (axisDetails.buttonState == JoystickButtonState.Down)
                                if (joyValueMax < joyValue)
                                {
                                    joyValueMax = joyValue;
                                    // dominantAxis = JoystickAxis.AxisV;
                                    dominantAxisInx = axis;
                                }



                        }

                        axis++;
                    }


                    // UnityEngine.Debug.Log(dominantAxisInx);
                    if (dominantAxisInx > -1)
                    {
                        //if (dominantAxis != JoystickAxis.None) {

                        //UnityEngine.Debug.Log("Count" + Axis.Count);
                        // UnityEngine.Debug.Log("dominantAxis "+dominantAxis+" state"+Axis[dominantAxis].buttonState);

                        //stick.AxisAsButtons [dominantAxis] != JoystickButtonState.Hold;

                        // index 6 and 7 are reserved for Pov Axes
                        if (dominantAxisInx == 6) //(int)JoystickAxis.AxisPovX
                        {
                            if (Axis[dominantAxisInx].value > 0)
                                return KeyCodeExtension.toCode((Joysticks)Index, dominantAxisInx, JoystickPovPosition.Right);
                            else
                                return KeyCodeExtension.toCode((Joysticks)Index, dominantAxisInx, JoystickPovPosition.Left);
                        }

                        //if (Axis[JoystickAxis.AxisPovX].buttonState == JoystickButtonState.Down)
                        //{

                        if (dominantAxisInx == 7)//(int)JoystickAxis.AxisPovY
                        {

                            if (Axis[dominantAxisInx].value > 0)
                                return KeyCodeExtension.toCode((Joysticks)Index, dominantAxisInx, JoystickPovPosition.Forward);
                            else
                                return KeyCodeExtension.toCode((Joysticks)Index, dominantAxisInx, JoystickPovPosition.Backward);
                        }

                        if (Axis[dominantAxisInx].value > 0)
                            return KeyCodeExtension.toCode((Joysticks)Index, dominantAxisInx, (int)JoystickPosition.Positive);
                        else
                            return KeyCodeExtension.toCode((Joysticks)Index, dominantAxisInx, (int)JoystickPosition.Negative);



                    }
					
					
					
					
					
					
						// A discrete POV returns specific values for left, right, etc.
						// A continuous POV returns an integer indicating an angle in degrees * 100, e.g. 18000 == 180.00 degrees.
						// The vast majority of joysticks have discrete POVs, so we'll treat all of them as discrete for simplicity.
						//					           0(Forward)
						//						         |
						//							     |
						//				 (Left)27000----- ----9000(Right)
						//						         |
						//							     |
						//							   18000(Backward)
						
						
						
					/////TODO make possible any axes to be POV (add isHatFirstAxis)
                        // Axis [JoystickAxis.AxisPovX].isHat


                    ///////////////////////////////////   HANDLE BUTTONS  //////////////////////////////

					
					int button = 0;
                    int numButtons = Buttons.Count;
                    ////DEBUG
                    //if (this is WiimoteDevice)
                    //{
                    //    UnityEngine.Debug.Log("GetInput took state:" + button_collection[button].buttonState);
                    //    if (button_collection[button].buttonState == JoystickButtonState.Down)
                    //        return KeyCodeExtension.toCode((Joysticks)ID, JoystickAxis.None, button);
                    //}



                    while (button < numButtons)
                    {

                   

                        if (Buttons[button].buttonState == JoystickButtonState.Down)
                            return KeyCodeExtension.toCode((Joysticks)Index, JoystickAxis.None, button);

                        // UnityEngine.Debug.Log("AfterbuttonState " + button_collection[0]);

                        button++;

                    }//while buttons
		
			return 0;


		}

         // calculate the bias and std of angulr speeds
         // set mBias and mNoiseLevel
         internal void calculateMPlusCalibration()
         {
             int n = _motionPlus.CalibrationInfo.mNoise.Count;
            
             _motionPlus.CalibrationInfo.mBias.x = (float)_motionPlus.CalibrationInfo.pitchSum / n;
             _motionPlus.CalibrationInfo.mBias.y = (float)_motionPlus.CalibrationInfo.rollSum / n;
             _motionPlus.CalibrationInfo.mBias.z = (float)_motionPlus.CalibrationInfo.yawSum / n;

             _motionPlus.CalibrationInfo.MotionPlusCalibrating = false;
             _motionPlus.CalibrationInfo.MotionPlusCalibrated = true;

             UnityEngine.Debug.Log("Motion Plus Calibrated"); 

         }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
         internal void InitMPlusCalibration(float x, float y, float z)
         {
             _motionPlus.CalibrationInfo.watch = new Stopwatch();
             _motionPlus.CalibrationInfo.watch.Start();
             _motionPlus.CalibrationInfo.mNoise = new List<Vector3>();
           

             _motionPlus.CalibrationInfo.MotionPlusCalibrating = true;
             _motionPlus.CalibrationInfo.CalibrationTimeout = CALIB_TIME;
             

             _motionPlus.CalibrationInfo.numCalibrationReadings = 0;
             _motionPlus.CalibrationInfo.pitchSum = 0;
             _motionPlus.CalibrationInfo.yawSum = 0;
             _motionPlus.CalibrationInfo.rollSum = 0;

             _motionPlus.CalibrationInfo.mMaxNoise = new Vector3();
             _motionPlus.CalibrationInfo.mBias = new Vector3();
             _motionPlus.CalibrationInfo.mMinNoise = new Vector3();



             _motionPlus.CalibrationInfo.mMaxNoise.x = _motionPlus.CalibrationInfo.mBias.x = _motionPlus.CalibrationInfo.mMinNoise.x = x;
             _motionPlus.CalibrationInfo.mMaxNoise.y = _motionPlus.CalibrationInfo.mBias.y = _motionPlus.CalibrationInfo.mMinNoise.y = y;
             _motionPlus.CalibrationInfo.mMaxNoise.z = _motionPlus.CalibrationInfo.mBias.z = _motionPlus.CalibrationInfo.mMinNoise.z = z;

            

         }

        /// <summary>
        /// Set the LEDs on the Wiimote
        /// </summary>
        /// <param name="inx">led index(0 to 3)</param>
         public void SetLEDs(int inx)
         {
             if (inx < 0 || inx > 3) return;

             ((WiiDriver)driver).SetLEDs(this, 1 << inx);
         }


         /// <summary>
         /// Toggle rumble
         /// </summary>
         /// <param name="on">On or off</param>
         internal void SetRumble(bool on)
         {
             ((WiiDriver)driver).SetRumble(this,on);
         }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector2 GetIRPoint(){



            switch (this.irMode)
            {
                case IRMode.Basic:
                    if (this.IR_SENSORS[0].Found && this.IR_SENSORS[1].Found)
                    {
                        _irPoint.x = (this.IR_SENSORS[0].Axis[JoystickAxis.AxisX].value + this.IR_SENSORS[1].Axis[JoystickAxis.AxisX].value) / 2;
                        _irPoint.y = (this.IR_SENSORS[0].Axis[JoystickAxis.AxisY].value + this.IR_SENSORS[1].Axis[JoystickAxis.AxisY].value) / 2;

                    }
                    break;
                case IRMode.Extended:
                    
                    float sumX=0f;
                    float sumY=0f;
                    int len=IR_SENSORS.Length;
                    int activSensors = 0;

                    for (int i=0; i < len; i++)
                    {
                        if(IR_SENSORS[i].Found){
                            activSensors++;
                            sumX+=IR_SENSORS[i].Axis[0].value;
                            sumY+=IR_SENSORS[i].Axis[1].value;
                        }
                    }

                    if(activSensors>0){
                        _irPoint.x=sumX/activSensors;
                        _irPoint.y=sumY/activSensors;
                    }





                 break;
            }



return _irPoint;
          
           
        }

        /// <summary>
        /// Request Disconnaection
        /// </summary>
        public void Disconnect()
        {
          
        }

        public void Dispose()
        {
            Disconnect();
        }



        internal void Reset()
        {
            int num_axes = Axis.Count;//Acceleration Axis not need digital input

            int axis = 0;
          

            IAxisDetails axisDetails;




            /////////////////////////////////   RESET AXIS  //////////////////////////////


            while (axis < num_axes)
            {

                if (axis < num_axes && (axisDetails = Axis[axis]) != null)
                {

                    axisDetails = Axis[axis];
                    axisDetails.value = 0f;
                    
                }

                axis++;
            }


          



            ///////////////////////////////////   RESET BUTTONS  //////////////////////////////


            int button = 0;
            int numButtons = Buttons.Count;
       


            while (button < numButtons)
            {



                Buttons[button].value = 0f;

                button++;

            }//while buttons
        }
    }
}
