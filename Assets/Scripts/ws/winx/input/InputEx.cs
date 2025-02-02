//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections.Generic;
using ws.winx.platform;
using ws.winx.devices;

namespace ws.winx.input
{
    public class InputExArgs : EventArgs
    {

        public InputAction action;

        public InputExArgs(InputAction action)
        {

            this.action = action;

        }
    }

    public class InputEx
    {


        public event EventHandler<InputExArgs> InputProcessed;
        public static int MAX_NUM_JOYSTICK_BUTTONS = 20;
        public static int MAX_NUM_MOUSE_BUTTONS = 7;




        public delegate object GetKeyBaseDelegate(int code);

        static Dictionary<int, bool> _isKeyState = new Dictionary<int, bool>();
        static int _numJoystickButtons = InputEx.MAX_NUM_JOYSTICK_BUTTONS;
        static int _numMouseButtons = InputEx.MAX_NUM_MOUSE_BUTTONS;
        static int _code;
        static int _codeFromGUIEvent;
        static int _lastCode;

        public static int LastCode
        {
            get { return InputEx._lastCode; }
            internal set { InputEx._lastCode = value; }
        }
        static InputAction _lastAction;
        //		static 		int _lastFrame=-1;
        static KeyCode _key;
        static KeyCode _button;
        static int _numJoysticks = 0;
        static KeyCode[] _keys = new KeyCode[]{
			KeyCode.Backspace,
			KeyCode.A,
			KeyCode.B,
			KeyCode.C,
			KeyCode.D,
			KeyCode.E,
			KeyCode.F,
			KeyCode.G,
			KeyCode.H,
			KeyCode.I,
			KeyCode.J,
			KeyCode.K,
			KeyCode.L,
			KeyCode.M,
			KeyCode.N,
			KeyCode.O,
			KeyCode.P,
			KeyCode.Q,
			KeyCode.R,
			KeyCode.S,
			KeyCode.T,
			KeyCode.U,
			KeyCode.V,
			KeyCode.W,
			KeyCode.X,
			KeyCode.Y,
			KeyCode.Z,
			KeyCode.UpArrow,
			KeyCode.DownArrow,
			KeyCode.RightArrow,
			KeyCode.LeftArrow,
			KeyCode.F1,
			KeyCode.F2,
			KeyCode.F3,
			KeyCode.F4,
			KeyCode.F5,
			KeyCode.F6,
			KeyCode.F7,
			KeyCode.F8,
			KeyCode.F9,
			KeyCode.F10,
			KeyCode.F11,
			KeyCode.F12,
			KeyCode.F13,
			KeyCode.F14,
			KeyCode.F15,
			KeyCode.Alpha0,
			KeyCode.Alpha1,
			KeyCode.Alpha2,
			KeyCode.Alpha3,
			KeyCode.Alpha4,
			KeyCode.Alpha5,
			KeyCode.Alpha6,
			KeyCode.Alpha7,
			KeyCode.Alpha8,
			KeyCode.Alpha9,
			KeyCode.RightShift,
			KeyCode.LeftShift,
			KeyCode.RightControl,
			KeyCode.LeftControl,
			KeyCode.RightAlt,
			KeyCode.LeftAlt,
			KeyCode.LeftCommand,
			KeyCode.LeftApple,
			KeyCode.LeftWindows,
			KeyCode.RightCommand,
			KeyCode.RightApple,
			KeyCode.RightWindows
		};
        private static float _lastCodeTime;
        private static volatile InputEx instance;





        /// <summary>
        /// Gets or sets the number joystick buttons.
        /// </summary>
        /// <value>The number joystick buttons.</value>
        public static int numJoystickButtons
        {
            get { return _numJoystickButtons; }
            set { _numJoystickButtons = Mathf.Min(MAX_NUM_JOYSTICK_BUTTONS, value); }
        }

        /// <summary>
        /// Gets the number joysticks.
        /// </summary>
        /// <value>The number joysticks.</value>
        public static int numJoysticks
        {
            get { return _numJoysticks; }

        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public static KeyCode key
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets the button.
        /// </summary>
        /// <value>The button.</value>
        public static KeyCode button
        {
            get { return _button; }
        }



        /// <summary>
        /// Gets or sets the array of KeyCode keys that could be used in the InputMapping
        /// </summary>
        /// <value>The keys.</value>
        public static KeyCode[] keys
        {
            get { return _keys; }
            set { _keys = value; }
        }


        /// <summary>
        /// Sets the keys from string array. Ex. string[]{"A","UpArrow","F1",...}
        /// </summary>
        /// <value>The string keys.</value>
        public static String[] stringKeys
        {
            set
            {
                if (value != null && value.Length < 1)
                    return;
                int len = value.Length;
                _keys = new KeyCode[len];
                for (int i = 0; i < len; i++)
                {
                    _keys[i] = (KeyCode)Enum.Parse(typeof(KeyCode), value[i]);
                }
            }
        }


        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>The code.</value>
        public static int code
        {
            get { return _code; }

        }

        public static bool GetAnyKeyDown(int id)
        {

            return Input.anyKeyDown || InputManager.Devices.GetDeviceAt(id).GetAnyKeyDown();

        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ws.winx.input.InputEx"/> any key down.
        /// from any Joystick (not to be used in 2plr hot seat)
        /// </summary>
        /// <value><c>true</c> if any key down; otherwise, <c>false</c>.</value>
        public static bool anyKeyDown
        {
            get
            {
                return Input.anyKeyDown || anyKeyDownOnAny();
            }
        }

        /// <summary>
        /// any key on any joystick device
        /// </summary>
        /// <returns></returns>
        static bool anyKeyDownOnAny()
        {
            IDeviceCollection devices = InputManager.Devices;

            foreach (IDevice device in devices)
                if (device.GetAnyKeyDown())
                    return true;

            return false;
        }

        public int numMouseButtons
        {
            get { return _numMouseButtons; }
            set { _numMouseButtons = Mathf.Min(MAX_NUM_MOUSE_BUTTONS, value); }
        }






        /// <summary>
        /// Proccess input information returned in Event in onGUI
        /// Useful for building Input mapper editor in "Edit mode"
        /// </summary>
        /// <param name="e">Event dispatched inside onGUI.</param>
        public static void processGUIEvent(Event e)
        {

            _codeFromGUIEvent = 0;//KeyCode.None;

            if (e.isKey)
            {

                //if event is KeyDown and ((if is in keystate tracking should be false (keyup)) or if new not in key state tracking)
                if (e.type == EventType.KeyDown && ((_isKeyState.ContainsKey((int)e.keyCode) && !_isKeyState[(int)e.keyCode]) || !_isKeyState.ContainsKey((int)e.keyCode)))
                {
                    _codeFromGUIEvent = (int)e.keyCode;

                    //Debug.Log (e.type+" "+e.keyCode);
                    _isKeyState[_codeFromGUIEvent] = true;
                }
                else if (e.type == EventType.KeyUp)
                {
                    _isKeyState[(int)e.keyCode] = false;

                }


                e.Use();


            }
            else if (e.isMouse)
            {
                //Debug.Log(e.type+" "+e.keyCode);

                if (e.type == EventType.MouseDown)
                {
                    _button = (KeyCode)Enum.Parse(typeof(KeyCode), "Mouse" + e.button);
                    _codeFromGUIEvent = (int)_button;

                    _isKeyState[_codeFromGUIEvent] = true;
                }
                else if (e.type == EventType.MouseUp)
                {

                    _isKeyState[(int)Enum.Parse(typeof(KeyCode), "Mouse" + e.button)] = false;
                }


                e.Use();
            }

        }



        //!!! IMPORTANT: Input happen every frame. If there is no refresh from the hardware device 
        // Input give same values just states are refreshed from DOWN->HOLD (value=1 stay) and UP->NONE (value=0 stay)

        /// <summary>
        /// Gets the axis.
        /// </summary>
        /// <returns>The axis.</returns>
        /// <param name="action">Action.</param>
        public static float GetAxis(InputAction action)
        {
            int code = action.code;


            if (action.fromAny)
            {//first device that have value not equal 0 or return 0
                IDeviceCollection devices = InputManager.Devices;
                float axisValue;

                foreach (IDevice device in devices)
                    if ((axisValue = device.GetAxis(code)) != 0)
                        return axisValue;

                return 0;

            }
            else
            {
                int index = KeyCodeExtension.toJoystickID(code);
                if (InputManager.Devices.ContainsIndex(index))
					return InputManager.Devices.GetDeviceAt(index).GetAxis(code);
                else
                    return 0;
            }
        }




        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <returns><c>true</c>, if key was gotten, <c>false</c> otherwise.</returns>
        /// <param name="code">Code.</param>
        /// <param name="fromAny">If set to <c>true</c> from any.</param>
        public static bool GetKey(int code, bool fromAny = false)
        {

            if (code < KeyCodeExtension.MAX_KEY_CODE)
            {
                if (Application.isPlaying)/*&& ! not procceing of GUI Event is used*/
                    return Input.GetKey((KeyCode)code);
                else
                    return _isKeyState[code];

            }
            else
            {
                if (fromAny)
                {
                    IDeviceCollection devices = InputManager.Devices;

                    foreach (IDevice device in devices)
                        if (device.GetKey(code))
                            return true;

                    return false;

                }
                else
                {
                    int index = KeyCodeExtension.toJoystickID(code);
                    if (InputManager.Devices.ContainsIndex(index))

                        return InputManager.Devices.GetDeviceAt(index).GetKey(code);
                    else
                        return false;

                }
            }
        }


        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <returns><c>true</c>, if key was gotten, <c>false</c> otherwise.</returns>
        /// <param name="action">Action.</param>
        public static bool GetKey(InputAction action)
        {
            return GetKey(action.code, action.fromAny);

        }



        /// <summary>
        /// Gets the key up.
        /// </summary>
        /// <returns><c>true</c>, if key up was gotten, <c>false</c> otherwise.</returns>
        /// <param name="action">Action.</param>
        public static bool GetKeyUp(InputAction action)
        {
            int code = action.code;

            if (code < KeyCodeExtension.MAX_KEY_CODE)
            {
                return Input.GetKeyUp((KeyCode)code);
            }
            else
            {
                if (action.fromAny)
                {
                    IDeviceCollection devices = InputManager.Devices;

                    foreach (IDevice device in devices)
                        if (device.GetKeyUp(code))
                            return true;

                    return false;

                }
                else
                {
                    int index = KeyCodeExtension.toJoystickID(code);
                    if (InputManager.Devices.ContainsIndex(index))

                        return InputManager.Devices.GetDeviceAt(index).GetKeyUp(code);
                    else
                        return false;


                }


            }
        }

        public static bool GetKeyDown(InputAction action)
        {

            int code = action.code;
            if (code < KeyCodeExtension.MAX_KEY_CODE)
            {
                return Input.GetKeyDown((KeyCode)code);
            }
            else
            {


                if (action.fromAny)
                {
                    IDeviceCollection devices = InputManager.Devices;
                    foreach (IDevice device in devices)
                        if (device.GetKeyDown(code))
                            return true;

                    return false;

                }
                else
                {
                    int index = KeyCodeExtension.toJoystickID(code);
                    if (InputManager.Devices.ContainsIndex(index))

                        return InputManager.Devices.GetDeviceAt(index).GetKeyDown(code);
                    else
                        return false;


                }
            }
        }

        protected static int GetGUIKeyboardInput()
        {
            return _codeFromGUIEvent;
        }


        /// <summary>
        /// Process runtime input
        ///  Useful for building Input mapper
        /// </summary>
        protected static int GetKeyboardInput()
        {

            KeyCode keyCode;


            _button = _key = 0;//KeyCode.None; 

            _numJoysticks = Input.GetJoystickNames().Length;

            int numKeys = _keys.Length;
            int maxLoops = Mathf.Max(numKeys, _numJoystickButtons);




            for (int i = 0; i < maxLoops; i++)
            {

                //check for mouse clicks
                if (i < _numMouseButtons)
                {

                    if (Input.GetMouseButtonDown(i))
                    {
                        _button = (KeyCode)Enum.Parse(typeof(KeyCode), "Mouse" + i);
                        return (int)_button;

                    }
                }

                //check for key clicks
                if (i < numKeys)
                {
                    keyCode = _keys[i];//
                    if (Input.GetKeyDown(keyCode))
                    {
                        _key = (KeyCode)keyCode;
                        return (int)keyCode;

                    }
                }


                //check for joysticks clicks
                //						if(i<_numJoysticks){
                //
                //					        //Let Unity handle JoystickButtons
                //							for(int j=0;j<_numJoystickButtons;j++){
                //								code=(KeyCode)Enum.Parse(typeof(KeyCode),"Joystick"+i+"Button"+j);
                //								if(Input.GetKeyDown(code)){
                //									_button=code;
                //									_code=(int)code;
                //									return;
                //								}
                //							}
                //
                //
                //
                //						}




            }




            return 0;



        }

        public static InputAction GetInput()
        {

            bool isPlaying = Application.isPlaying;

            float time = isPlaying ? Time.time : Time.realtimeSinceStartup;


            //prioterize joysticks
            IDeviceCollection devices = InputManager.Devices;




            foreach (IDevice device in devices)
            {

                //If 
                if ((_code = device.GetInput()) != 0)
                {
                    Debug.Log("Get Input Joy" + device.Index + " " + KeyCodeExtension.toEnumString(_code)+"frame:"+Time.frameCount);
                    return processInput(_code, time);
                }
            }


            if (isPlaying)
                _code = GetKeyboardInput();
            else
                _code = GetGUIKeyboardInput();



            _lastAction = processInput(_code, time);

            return _lastAction;
        }

       

        /// <summary>
        /// Check if InputActin happened
        /// </summary>
        /// <returns>true/false</returns>
        /// <param name="action">InputAction to be compared with input</param>
       internal static bool GetAction(InputAction action)
        {

            if (action.type == InputActionType.SINGLE)
            {
                if (InputEx.GetKeyDown(action))
                {
                    Debug.Log("Single <" + InputActionType.SINGLE);
                    _lastCode = action.code;
                    return true;
                }

                return false;
            }


            if (action.type == InputActionType.DOUBLE)
            {
                if (InputEx.GetKeyDown(action))
                {
                    if (_lastCode != action.code)
                    {//first click

                        _lastCode = action.code;
                        action.startTime = Time.time;
                        Debug.Log("First Click" + Time.time + ":" + action.startTime + " going for " + InputActionType.DOUBLE);
                        return false;
                    }
                    else
                    {//InputEx.LastCode==_pointer.Current.code //second click
                        //check time diffrence if less then
                        if (Time.time - action.startTime < InputAction.DOUBLE_CLICK_SENSITIVITY)
                        {

                            _lastCode = 0;//???
                            action.startTime = 0;
                            Debug.Log("Double " + Time.time + ":" + action.startTime + "<" + InputActionType.DOUBLE);

                            return true;
                        }

                       // Debug.Log("Lost Double " + Time.time + ":" + action.startTime + "<" + InputActionType.DOUBLE);
                    }
                }

                return false;
            }


            if (action.type == InputActionType.LONG)
            {
                if (InputEx.GetKey(action))
                {//if hold
                    if (_lastCode != action.code)
                    {

                        _lastCode = action.code;

                        action.startTime = Time.time;

                        return false;
                    }
                    else
                    {//InputEx.LastCode==_pointer.Current.code //hold
                        //check time diffrence if less then
                        if (Time.time - action.startTime >= InputAction.LONG_CLICK_SENSITIVITY)
                        {

                            _lastCode = 0;//KeyCode.None;
                            action.startTime = 0;
                            Debug.Log("Long " + (Time.time - action.startTime) + " " + InputActionType.LONG);

                            return true;
                        }
                    }
                }

                return false;

            }


            return false;


        }


     
        /// <summary>
        /// Processes the input code into InputAction
        /// </summary>
        /// <returns>InputAction (single,double,long) or null.</returns>
        /// <param name="code">Code.</param>
        /// <param name="time">Time.</param>
        internal static InputAction processInput(int code, float time)
        {


            InputAction action = null;

           // Debug.Log ("process "+code);

            if (code != 0)
            {//=KeyCode.None 


                if (_lastCode == 0)
                {//=KeyCode.None
                    //save key event and time needed for double check
                    _lastCode = code;
                    _lastCodeTime = time;

                    Debug.Log("Last code " + KeyCodeExtension.toEnumString(_lastCode));
                    //	Debug.Log("Take time "+_lastCodeTime);
                }
                else
                {
                    //if new pressed key is different then the last
                    if (_lastCode != code)
                    {
                        //consturct string from lastCode
                        action = new InputAction(_lastCode, InputActionType.SINGLE);

                        //take new pressed code as lastCode
                        _lastCode = code;

                       Debug.Log("Single " + time + ":" + _lastCodeTime + " " + InputActionType.SINGLE);



                    }
                    else
                    {


                        if (time - _lastCodeTime < InputAction.DOUBLE_CLICK_SENSITIVITY)
                        {
                            action = new InputAction(_lastCode, InputActionType.DOUBLE);
                            _lastCode = 0;//KeyCode.None;
                            Debug.Log("Double " + time + ":" + _lastCodeTime + "<" + InputActionType.DOUBLE);
                        }


                    }

                }

            }
            else
            {



                if (_lastCode != 0)
                {//=KeyCode.None
                    //if key is still down and time longer then default long time click => display long click
                    if (InputEx.GetKey(_lastCode))
                    {
                        if (time - _lastCodeTime >= InputAction.LONG_CLICK_SENSITIVITY)
                        {
                            action = new InputAction(_lastCode, InputActionType.LONG);
                            _lastCode = 0;//KeyCode.None;
                           Debug.Log("Long " + (time - _lastCodeTime) + " <" + InputActionType.LONG);
                        }
                    }
                    else
                    {//time wating for double click activity passed => display last code
                        if (time - _lastCodeTime >= InputAction.DOUBLE_CLICK_SENSITIVITY)
                        {
                            action = new InputAction(_lastCode, InputActionType.SINGLE);
                            _lastCode = 0;//KeyCode.None;

                           Debug.Log("Single after wating Double time pass " + (time - _lastCodeTime) + " " + InputActionType.SINGLE);
                        }

                    }



                }

            }
            return action;
        }







    }
}

