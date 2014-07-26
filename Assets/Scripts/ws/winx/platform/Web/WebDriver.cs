﻿#if UNITY_WEBPLAYER	
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ws.winx.devices;
using UnityEngine;
using System.Reflection;


namespace ws.winx.platform.web
{
	public class WebDriver:IJoystickDriver
	{
        protected bool _isReady=true;
        protected WebHIDBehaviour _webHidBehavior;
        protected IHIDInterface _hidInterface;

        public IJoystickDevice ResolveDevice(IHIDDeviceInfo info)
        {
            _webHidBehavior = GameObject.Find("WebHIDBehaviourGO").GetComponent<WebHIDBehaviour>();
            _webHidBehavior.PositionUpdateEvent += new EventHandler<WebMessageArgs>(onPositionUpdate);

            _hidInterface = info.hidInterface;

            JoystickDevice joystick;

            int numAxes = info.numAxes; //info.Extension.axes;
            int numButtons = info.numButtons; //info.Extension.buttons;

         
              joystick = new JoystickDevice(info.index, info.PID, info.VID, Math.Max(8,numAxes),numButtons, this);
              joystick.Name = info.Name;

                    

                    int index = 0;

                    for (; index < numButtons; index++)
                    {
                        joystick.Buttons[index] = new ButtonDetails();
                    }

             
                    for (index = 0; index < numAxes; index++)
                    {
                                             
                          joystick.Axis[index] = new AxisDetails();
                       
                    }




                    return joystick;

        }


         protected void onPositionUpdate(object sender,WebMessageArgs args){

             Json.GamePadInfo info = Json.Deserialize(args.Message) as Json.GamePadInfo;

             UnityEngine.Debug.Log(args + " " + info.index);
             int i=0;
            
             JoystickDevice device = _hidInterface.Devices[info.index] as JoystickDevice;

             UnityEngine.Debug.Log(device);
             UnityEngine.Debug.Log(info.axes);
             UnityEngine.Debug.Log(info.axes.Count+" "+device.Axis.Count);


            
             PropertyInfo pInfo;
             UnityEngine.Debug.Log("buttons" + info.buttons);
             UnityEngine.Debug.Log("buttons" + info.buttons.Count);
             foreach (var obj in info.buttons)
             {

                 pInfo = obj.GetType().GetProperty("value");

                // UnityEngine.Debug.Log("has value:" + (pInfo != null)+" type "+obj.GetType());

                 if (pInfo != null)
                     device.Buttons[i++].value = Convert.ToSingle(pInfo.GetValue(obj, null));
                 else
                     device.Buttons[i++].value = Convert.ToSingle(obj);

             }

             i = 0;
             foreach (var obj in info.axes){

               // UnityEngine.Debug.Log(obj.GetType());
                 device.Axis[i++].value =Convert.ToSingle(Math.Min(1f, Math.Max(-1, Convert.ToSingle(obj))));
               //  UnityEngine.Debug.Log("axes value:" +device.Axis[i-1].value);
             }

           

              _isReady = true;
      //    var b = buttons[i];
      //var val = controller.buttons[i];
      //var pressed = val == 1.0;
      //if (typeof(val) == "object") {
      //  pressed = val.pressed;
      //  val = val.value;
      //}

             //UPDATING AXIS BUTTONS
         }

         public void Update(IJoystickDevice joystick)
        {
            if (_isReady)
            {
                _isReady = false;
                _webHidBehavior.joyGetPosEx(joystick.ID);
            }


            
        }






        #region ButtonDetails
        public sealed class ButtonDetails : IButtonDetails
        {

            #region Fields

            float _value;
            uint _uid;
            JoystickButtonState _buttonState;

            #region IDeviceDetails implementation


            public uint uid
            {
                get
                {
                    return _uid;
                }
                set
                {
                    _uid = value;
                }
            }




            public JoystickButtonState buttonState
            {
                get { return _buttonState; }
            }



            public float value
            {
                get
                {
                    return _value;
                    //return (_buttonState==JoystickButtonState.Hold || _buttonState==JoystickButtonState.Down);
                }
                set
                {

                    _value = value;

                    //  UnityEngine.Debug.Log("Value:" + _value);

                    //if pressed==TRUE
                    //TODO check the code with triggers
                    if (value > 0)
                    {
                        if (_buttonState == JoystickButtonState.None
                            || _buttonState == JoystickButtonState.Up)
                        {

                            _buttonState = JoystickButtonState.Down;



                        }
                        else
                        {
                            //if (buttonState == JoystickButtonState.Down)
                            _buttonState = JoystickButtonState.Hold;

                        }


                    }
                    else
                    { //
                        if (_buttonState == JoystickButtonState.Down
                            || _buttonState == JoystickButtonState.Hold)
                        {
                            _buttonState = JoystickButtonState.Up;
                        }
                        else
                        {//if(buttonState==JoystickButtonState.Up){
                            _buttonState = JoystickButtonState.None;
                        }

                    }
                }
            }
            #endregion
            #endregion

            #region Constructor
            public ButtonDetails(uint uid = 0) { this.uid = uid; }
            #endregion






        }

        #endregion

        #region AxisDetails
        public sealed class AxisDetails : IAxisDetails
        {

            #region Fields
            float _value;
            int _uid;
            int _min;
            int _max;
            JoystickButtonState _buttonState = JoystickButtonState.None;
            bool _isNullable;
            bool _isHat;
            bool _isTrigger;


            #region IAxisDetails implementation

            public bool isTrigger
            {
                get
                {
                    return _isTrigger;
                }
                set
                {
                    _isTrigger = value;
                }
            }


            public int min
            {
                get
                {
                    return _min;
                }
                set
                {
                    _min = value;
                }
            }


            public int max
            {
                get
                {
                    return _max;
                }
                set
                {
                    _max = value;
                }
            }


            public bool isNullable
            {
                get
                {
                    return _isNullable;
                }
                set
                {
                    _isNullable = value;
                }
            }


            public bool isHat
            {
                get
                {
                    return _isHat;
                }
                set
                {
                    _isHat = value;
                }
            }


            #endregion


            #region IDeviceDetails implementation


            public uint uid
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }


            #endregion

            public JoystickButtonState buttonState
            {
                get { return _buttonState; }
            }
            public float value
            {
                get { return _value; }
                set
                {

                    if (value == 0)
                    {
                        if (_buttonState == JoystickButtonState.Down
                            || _buttonState == JoystickButtonState.Hold)
                        {

                            //axis float value isn't yet update so it have value before getting 0
                            if (_value > 0)//0 come after positive values
                                _buttonState = JoystickButtonState.PosToUp;
                            else
                                _buttonState = JoystickButtonState.NegToUp;

                        }
                        else
                        {//if(buttonState==JoystickButtonState.Up){
                            _buttonState = JoystickButtonState.None;
                        }


                    }
                    else
                    //!!! value can jump from >0 to <0 without go to 0(might go to "Down" directly for triggers axis)
                    {
                        if (_value > 0 && value < 0)
                        {
                            _buttonState = JoystickButtonState.PosToUp;
                        }
                        else if (_value < 0 && value > 0)
                        {
                            _buttonState = JoystickButtonState.NegToUp;
                        }
                        else if (_buttonState == JoystickButtonState.None
                           || _buttonState == JoystickButtonState.PosToUp || _buttonState == JoystickButtonState.NegToUp)
                        {

                            _buttonState = JoystickButtonState.Down;

                        }
                        else
                        {
                            _buttonState = JoystickButtonState.Hold;
                        }


                    }




                    _value = value;

                    //UnityEngine.Debug.Log("ButtonState:"+_buttonState+"_value:"+_value);

                }//set
            }

            #endregion

        }

        #endregion
    }
}
#endif