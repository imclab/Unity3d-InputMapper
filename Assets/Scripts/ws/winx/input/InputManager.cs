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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Text;
using ws.winx.platform;
using System.ComponentModel;
using ws.winx.devices;
using System.Threading;

namespace ws.winx.input
{

    public static class InputManager
    {
       

       
       
		private static InputCombination[] __inputCombinations;
		private static InputSettings __settings;//=new InputSettings();
		private static IHIDInterface __hidInterface;//=new ws.winx.platform.windows.WinHIDInterface();
        private static List<IJoystickDriver> __drivers;



      

		internal static IHIDInterface hidInterface{
			get{ 
               
				if(__hidInterface==null){
					//if((Application.platform & (RuntimePlatform.WindowsPlayer | RuntimePlatform.WindowsEditor))!=0){

                        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                        __hidInterface = new ws.winx.platform.windows.WinHIDInterface(__drivers);
                         #endif
                        

					 #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
						__hidInterface=new ws.winx.platform.osx.OSXHIDInterface(__drivers);
                     #endif


					#if UNITY_WEBPLAYER && !UNITY_EDITOR
						__hidInterface=new ws.winx.platform.web.WebHIDInterface(__drivers);
                    #endif

                        Debug.Log(__hidInterface.GetType()+" is Initialized");
				}



				return __hidInterface; }
		}

			

		public static InputSettings Settings{
			get{  if(__settings==null) __settings=new InputSettings(); return __settings;}
		}


        public static List<T> GetJoysticks<T>()
        {
            IDeviceCollection devices = InputManager.hidInterface.Devices;

            List<T> Result = new List<T>();

            foreach (IJoystickDevice device in devices)
            {
                if (device.GetType() == typeof(T))
                {
                    Result.Add((T)device);

                }
                
            }

            return Result;
        }


		public static void AddDriver(IJoystickDriver driver){
            if(__drivers==null) __drivers= new List<IJoystickDriver>();
            __drivers.Add(driver);
		}



		/// <summary>
		/// Maps state to input.
		/// </summary>
		/// <param name="stateName">State name.</param>
		/// <param name="at">At.</param>
		/// <param name="combos">Combos.</param>
		public static void MapStateToInput(String stateName,int at=-1,params KeyCode[] combos){
			
			MapStateToInput(stateName,new InputCombination(combos),at);
			
		}


        /// <summary>
        /// Maps state to input.
        /// </summary>
        /// <param name="stateName">State name hash.</param>
        /// <param name="at">At.</param>
        /// <param name="combos">Combos.</param>
        public static void MapStateToInput(int stateNameHash, int at = -1, params KeyCode[] combos)
        {

            MapStateToInput(stateNameHash, new InputCombination(combos), at);

        }


		/// <summary>
		/// Maps state to input.
		/// </summary>
		/// <param name="stateName">State name.</param>
		/// <param name="at">At.</param>
		/// <param name="combos">Combos.</param>
		public static void MapStateToInput(String stateName,int at=-1,params int[] combos){

            MapStateToInput(stateName, new InputCombination(combos), at);
			
		}


        /// <summary>
        /// Maps state to input.
        /// </summary>
        /// <param name="stateName">State name Hash.</param>
        /// <param name="at">At.</param>
        /// <param name="combos">Combos.</param>
        public static void MapStateToInput(int stateNameHash, int at = -1, params int[] combos)
        {

            MapStateToInput(stateNameHash, new InputCombination(combos), at);

        }


        /// <summary>
        /// Maps state to input.
        /// </summary>
        /// <param name="stateName">State name Hash.</param>
        /// <param name="at">At.</param>
        /// <param name="combos">Actions. KeyExtension.Backspace.DOUBLE,...</param>
        public static void MapStateToInput(int stateNameHash, int at = -1, params InputAction[] actions)
        {

            MapStateToInput(stateNameHash, new InputCombination(actions), at);

        }


        /// <summary>
        /// Maps state to input.
        /// </summary>
        /// <param name="stateName">State name .</param>
        /// <param name="at">At.</param>
        /// <param name="combos">Actions. ex KeyExtension.Backspace.DOUBLE,...</param>
        public static void MapStateToInput(string stateName, int at = -1, params InputAction[] actions)
        {

            MapStateToInput(stateName, new InputCombination(actions), at);

        }


		/// <summary>
		/// Maps state to input.
		/// </summary>
		/// <param name="stateName">State name </param>
		/// <param name="codeCombination">Code combination.
		/// just "I" for KeyCode.I
		/// or "I"+InputAction.DOUBLE_DESIGNATOR 
		///	 or "I"+InputAction.DOUBLE_DESIGNATOR+InputAction.SPACE_DESIGNATOR+(other code);
		///   or just "I(x2)" depending of InputAction.DOUBLE_DESIGNATOR value
		/// </param>
		/// <param name="at">At.Valid:-1(next) or 0(primary) and 1(secondary)</param>
		public static void MapStateToInput(String stateName,String codeCombination,int at=-1){

			MapStateToInput(stateName,new InputCombination(codeCombination),at);

		}

        /// <summary>
        /// Maps state to input.
        /// </summary>
        /// <param name="stateNameHash">State name hash(int) </param>
        /// <param name="codeCombination">Code combination.
        /// just "I" for KeyCode.I
        /// or "I"+InputAction.DOUBLE_DESIGNATOR 
        ///	 or "I"+InputAction.DOUBLE_DESIGNATOR+InputAction.SPACE_DESIGNATOR+(other code);
        ///   or just "I(x2)" depending of InputAction.DOUBLE_DESIGNATOR value
        /// </param>
        /// <param name="at">At.Valid:-1(next) or 0(primary) and 1(secondary)</param>
        public static void MapStateToInput(int stateNameHash, String codeCombination, int at = -1)
        {

            MapStateToInput(stateNameHash, new InputCombination(codeCombination), at);

        }


        /// <summary>
        /// Maps state to input.
        /// </summary>
        /// <param name="stateName">State name.</param>
        /// <param name="combos">Combos (ex. (int)KeyCode.P,(int)KeyCode.Joystick2Button12,JoystickDevice.toCode(...))</param>
        public static void MapStateToInput(String stateName, params int[] combos)
        {
            MapStateToInput(stateName, new InputCombination(combos));
        }


        /// <summary>
        /// Maps state to input.
        /// </summary>
        /// <param name="stateName">State name hash.</param>
        /// <param name="combos">Combos (ex. (int)KeyCode.P,(int)KeyCode.Joystick2Button12,JoystickDevice.toCode(...))</param>
        public static void MapStateToInput(int stateNameHash, params int[] combos)
        {
            MapStateToInput(stateNameHash, new InputCombination(combos));
        }


        /// <summary>
        /// Maps state to input.
        /// </summary>
        /// <param name="stateName">State name hash.</param>
        /// <param name="combos">Combos (ex. KeyCodeExtension.Backspace.DOUBLE,KeyCodeExtesnion.Joystick1AxisYPositive.SINGLE)</param>
        public static void MapStateToInput(int stateNameHash, params InputAction[] actions)
        {
            MapStateToInput(stateNameHash, new InputCombination(actions));
        }



        /// <summary>
        /// Maps state to input.
        /// </summary>
        /// <param name="stateName">State name</param>
        /// <param name="combos">Combos (ex. KeyCodeExtension.Backspace.DOUBLE,KeyCodeExtesnion.Joystick1AxisYPositive.SINGLE)</param>
        public static void MapStateToInput(string stateName, params InputAction[] actions)
        {
            MapStateToInput(stateName, new InputCombination(actions));
        }


        /// <summary>
        /// Maps state to input.
        /// </summary>
        /// <param name="stateName">State name.</param>
        /// <param name="combination">Combination.</param>
        /// <param name="at">At.Valid:-1(next) or 0(primary) and 1(secondary)</param>
        public static void MapStateToInput(string stateName, InputCombination combination, int at = -1)
        {

            if (at > 2) throw new Exception("Only indexes 0(Primary) and 1(Secondary) input are allowed");

            int stateNameHash=Animator.StringToHash(stateName);
            InputState state;

            if (!Settings.stateInputs.ContainsKey(stateNameHash))
            {
                //create InputState and add to Dictionary of state inputs
                state = __settings.stateInputs[stateNameHash] = new InputState(stateName, stateNameHash);
            }
            else
            {
                state = __settings.stateInputs[stateNameHash];
            }

            state.Add(combination, at);

        }


		/// <summary>
		/// Maps state to input.
		/// </summary>
		/// <param name="stateName">State name hash.</param>
		/// <param name="combination">Combination.</param>
		/// <param name="at">At.Valid:-1(next) or 0(primary) and 1(secondary)</param>
		public static void MapStateToInput(int stateNameHash,InputCombination combination,int at=-1){

			if(at>2) throw new Exception("Only indexes 0(Primary) and 1(Secondary) input are allowed");
			
			
			InputState state;
			
			if(!Settings.stateInputs.ContainsKey(stateNameHash)){
				//create InputState and add to Dictionary of state inputs
				state=__settings.stateInputs[stateNameHash]=new InputState("GenState_"+stateNameHash,stateNameHash);
			}else{
				state=__settings.stateInputs[stateNameHash];
			}

			state.Add(combination,at);
	
		}


	

		//[Not tested] idea for expansion
		public static void PlayStateOnInputUp(Animator animator,int stateNameHash,int layer=0,float normalizedTime=0f){
					if(InputManager.GetInputUp(stateNameHash)) 
						animator.Play(stateNameHash,layer,normalizedTime); 
		}

		public static void PlayStateOnInputDown(Animator animator,int stateNameHash,int layer=0,float normalizedTime=0f){
			if(InputManager.GetInputDown(stateNameHash)) 
				animator.Play(stateNameHash,layer,normalizedTime); 
		}


		public static void CrossFadeStateOnInputUp(Animator animator,int stateNameHash,float transitionDuration=0.5f,int layer=0,float normailizeTime=0f){
				if(InputManager.GetInputUp(stateNameHash)) 
				animator.CrossFade(stateNameHash,transitionDuration,layer,normailizeTime);

		}

		public static void CrossFadeStateOnInputDown(Animator animator,int stateNameHash,float transitionDuration=0.5f,int layer=0,float normailizeTime=0f){
			if(InputManager.GetInputDown(stateNameHash)) 
				animator.CrossFade(stateNameHash,transitionDuration,layer,normailizeTime);
    	}
  
		
		
	#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
		/// <summary>
		/// Loads the Input settings from InputSettings.xml and deserialize into OO structure.
		/// Create your .xml settings with InputMapper Editor
		/// </summary>
		public static InputSettings loadSettings(String path="InputSettings.xml"){
			XmlReaderSettings xmlSettings=new XmlReaderSettings();
			xmlSettings.CloseInput=true;
			xmlSettings.IgnoreWhitespace=true;


			
			//DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<int,InputCombination[]>),"Inputs","");
			DataContractSerializer serializer = new DataContractSerializer(typeof(InputSettings),"Inputs","");

			
			using(XmlReader reader=XmlReader.Create(path,xmlSettings))
			{

				__settings=(InputSettings)serializer.ReadObject(reader);

			}
			


			return __settings;
		}
#endif




		#if (UNITY_WEBPLAYER	|| UNITY_EDITOR) && !UNITY_STANDALONE
		/// <summary>
		/// Loads the Input settings from InputSettings.xml and deserialize into OO structure.
		/// Create your .xml settings with InputMapper Editor
		/// </summary>
        public static IEnumerator loadSettings(String path)
        {
             XmlReaderSettings xmlSettings=new XmlReaderSettings();
			xmlSettings.CloseInput=true;
			xmlSettings.IgnoreWhitespace=true;

            if (Application.isEditor)
                path = "file:///" + path;

            WWW www = new WWW(path);
          // UnityEngine.Debug.Log(path);
            while (!www.isDone)
            {
                yield return null;
            }

            
          

            if (www.error != null)
            {
                UnityEngine.Debug.LogError(www.error);
                yield break;
            }

            loadSettingsFromText(www.text);

           

          yield break;
          
        }
#endif

        public static void loadSettingsFromText(string text,bool readBOM=true)
        {
            XmlReaderSettings xmlSettings = new XmlReaderSettings();
            xmlSettings.CloseInput = true;
            xmlSettings.IgnoreWhitespace = true;
            StringReader stringReader = new StringReader(text);
            if(readBOM)
            stringReader.Read();//skip BOM
            using (XmlReader reader = XmlReader.Create(stringReader, xmlSettings))
            {
                __settings = new InputSettings();

                int key;

                InputAction action;
                List<InputAction> actions = null;
                InputCombination[] combinations = null;
                string name;
                InputState state;
                int i;
                //XmlNameTable nameTable = reader.NameTable;
                //XmlNamespaceManager nsManager = new XmlNamespaceManager(nameTable);
                //nsManager.AddNamespace("d1p1", "http://schemas.datacontract.org/2004/07/ws.winx.input");

                reader.ReadToFollowing("d1p1:doubleDesignator");
                __settings.doubleDesignator = reader.ReadElementContentAsString();


                __settings.longDesignator = reader.ReadElementContentAsString();


                __settings.spaceDesignator = reader.ReadElementContentAsString();




                __settings.singleClickSensitivity = reader.ReadElementContentAsFloat();


                __settings.doubleClickSensitivity = reader.ReadElementContentAsFloat();


                __settings.longClickSensitivity = reader.ReadElementContentAsFloat();


                __settings.combinationsClickSensitivity = reader.ReadElementContentAsFloat();

                if (reader.ReadToFollowing("d2p1:KeyValueOfintInputState"))
                {


                    do
                    {
                        reader.ReadToDescendant("d2p1:Key");

                        key = reader.ReadElementContentAsInt();




                        if (reader.ReadToFollowing("d1p1:InputCombination"))
                        {

                            combinations = new InputCombination[2];
                            i = 0;

                            do
                            {
                                if (reader.GetAttribute("i:nil") == null)
                                {


                                    if (reader.ReadToDescendant("d1p1:InputAction"))
                                    {
                                        actions = new List<InputAction>();

                                        do
                                        {
                                            reader.ReadToDescendant("d1p1:Code");

                                            action = new InputAction(reader.ReadElementContentAsString());

                                            actions.Add(action);

                                        } while (reader.ReadToNextSibling("d1p1:InputAction"));


                                    }




                                    combinations[i++] = new InputCombination(actions);

                                    reader.Read();//read </InputCombination>

                                }



                            } while (reader.ReadToNextSibling("d1p1:InputCombination"));



                        }

                        reader.ReadToFollowing("d1p1:Name");
                        name = reader.ReadElementContentAsString();
                        state = new InputState(name, key);
                        state.combinations = combinations;
                        __settings.stateInputs[key] = state;


                        reader.Read();//</d2p1:KeyValueOfintInputState>

                    } while (reader.ReadToNextSibling("d2p1:KeyValueOfintInputState"));
                }
            }

            stringReader.Close();
        }



        static IEnumerator WaitAndPrint(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            Debug.Log("WaitAndPrint " + Time.time);
        }

		#if (UNITY_EDITOR || UNITY_WEBPLAYER) && !UNITY_STANDALONE
		public static IEnumerator saveSettings(String path){

			//TODO manual serialization

			WWWForm wwwForm=new WWWForm();
			//wwwForm.AddField(

			WWW www=new WWW(path);


			yield return www;
		}
		#endif


		#if (UNITY_STANDALONE || UNITY_EDITOR)&& !UNITY_WEBPLAYER
		/// <summary>
		/// Saves the settings to InputSettings.xml.
		/// </summary>
		public static void saveSettings(String path){

			//DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<int,InputCombination[]>),"Inputs","");
			 
			DataContractSerializer serializer = new DataContractSerializer(typeof(InputSettings),"Inputs","");
			

			XmlWriterSettings xmlSettings=new XmlWriterSettings();
			xmlSettings.Indent=true;
			xmlSettings.CloseOutput=true;//this would close stream after write 
		//	xmlSettings.IndentChars="\t";
		//	xmlSettings.NewLineOnAttributes = false;
		//	xmlSettings.OmitXmlDeclaration = true;





			using(XmlWriter writer=XmlWriter.Create(path,xmlSettings))
			{
				//serializer.WriteObject(writer, __settings.stateInputs);
				serializer.WriteObject(writer,__settings);

				//Write the XML to file and close the writer.
				writer.Flush();
				writer.Close(); 


			}



		}

		#endif     
      



      

        //public void resetMap(){
        //}

		/// <summary>
		/// Gets the input of real or virutal axis(2keys used as axis) mapped to State.
		/// </summary>
		/// <returns>The input.</returns>
		/// <param name="stateNameHash">State name hash.</param>
		/// <param name="fromRange">From range.</param>
		/// <param name="toRange">To range.</param>
		/// <param name="sensitivity">Sensitivity.</param>
		/// <param name="dreadzone">Dreadzone.</param>
		/// <param name="gravity">Gravity.</param>
		public static float GetInput(int stateNameHash,float sensitivity=0.3f,float dreadzone=0.1f,float gravity=0.3f){
			__inputCombinations=__settings.stateInputs[stateNameHash].combinations;


            return __inputCombinations[0].GetAxis(sensitivity, dreadzone, gravity) + (__inputCombinations.Length == 2 && __inputCombinations[1] != null ? __inputCombinations[1].GetAxis(sensitivity, dreadzone, gravity) : 0);

		}

		/// <summary>
		/// Gets the input.
		/// </summary>
		/// <returns><c>true</c>, if input happen, <c>false</c> otherwise.</returns>
		/// <param name="stateNameHash">State name hash.</param>
		/// <param name="atOnce" default="false">Affect only in combo inputs!!!(default=false)Function returns true when combination pressed in row  If set to <c>true</c> function return true when all keys/buttons are pressed.</param>
		public static bool GetInput(int stateNameHash,bool atOnce=false){
			__inputCombinations=__settings.stateInputs[stateNameHash].combinations;
            return __inputCombinations[0].GetInput(atOnce) || (__inputCombinations.Length == 2 && __inputCombinations[1] != null && __inputCombinations[1].GetInput(atOnce));
        }

		/// <summary>
		/// Gets the input up.
		/// </summary>
		/// <returns><c>true</c>, if input binded to state happen, <c>false</c> otherwise.</returns>
		/// <param name="stateNameHash">State name hash.</param>
		public static bool GetInputUp(int stateNameHash){
			__inputCombinations=__settings.stateInputs[stateNameHash].combinations;
            return __inputCombinations[0].GetInputUp() || (__inputCombinations.Length == 2 && __inputCombinations[1] != null && __inputCombinations[1].GetInputUp());
		}

		/// <summary>
		/// Gets the input down.
		/// </summary>
		/// <returns><c>true</c>, if input binded to state down happen, <c>false</c> otherwise.</returns>
		/// <param name="stateNameHash">State name hash.</param>
		
		public static bool GetInputDown(int stateNameHash){
			__inputCombinations=__settings.stateInputs[stateNameHash].combinations;
            return __inputCombinations[0].GetInputDown() || (__inputCombinations.Length == 2 && __inputCombinations[1] != null && __inputCombinations[1].GetInputDown());
		}



		/// <summary>
		/// Log states - inputs values to console
		/// </summary>
		public static string Log(){
			StringBuilder content=new StringBuilder();
			int i;
			//primary,secondary...
			InputCombination[] combinations;

			foreach (var keyValuPair in __settings.stateInputs)
			{
				content.AppendLine("Name:"+keyValuPair.Value.name+" Key:"+keyValuPair.Key);
				combinations=keyValuPair.Value.combinations;

				for(i=0;i<combinations.Length;i++){
					if(combinations[i]!=null)
					content.AppendLine(i+": " +combinations[i].ToString());
				}

				content.AppendLine();
				 

			}


						return content.ToString();

		}



        public static void Dispose(){


            if (__hidInterface != null)
            {
                __hidInterface.Dispose();
                __hidInterface = null;
            }

        }



		#region Settings

		#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
		[DataContract]
#endif
		public class InputSettings{



			
			#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
			[DataMember(Order=4)]
			#endif
			public float singleClickSensitivity{
				get{ return InputAction.SINGLE_CLICK_SENSITIVITY; }
				set{ InputAction.SINGLE_CLICK_SENSITIVITY=value; }

			}

			#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
			[DataMember(Order=5)]
			#endif
			public float doubleClickSensitivity{
				get{ return InputAction.DOUBLE_CLICK_SENSITIVITY; }
				set{ InputAction.DOUBLE_CLICK_SENSITIVITY=value; }
				
			}

			#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
			[DataMember(Order=6)]
			#endif
			public float longClickSensitivity{
				get{ return InputAction.LONG_CLICK_SENSITIVITY; }
				set{ InputAction.LONG_CLICK_SENSITIVITY=value; }
				
			}

			#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
			[DataMember(Order=7)]
			#endif
			public float combinationsClickSensitivity{
				get{ return InputAction.COMBINATION_CLICK_SENSITIVITY; }
				set{ InputAction.COMBINATION_CLICK_SENSITIVITY=value; }
				
			}

			#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
			[DataMember(Order=1)]
			#endif
			public string doubleDesignator{
				get{ return InputAction.DOUBLE_DESIGNATOR; }
				set{ InputAction.DOUBLE_DESIGNATOR=value; }
				
			}

			#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
			[DataMember(Order=2)]
			#endif
			public string longDesignator{
				get{ return InputAction.LONG_DESIGNATOR; }
				set{ InputAction.LONG_DESIGNATOR=value; }
				
			}

			#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
			[DataMember(Order=3)]
			#endif
			public string spaceDesignator{
				get{ return InputAction.SPACE_DESIGNATOR.ToString(); }
				set{ InputAction.SPACE_DESIGNATOR=value[0]; }
				
			}

			#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
			[DataMember(Name="StateInputs",Order=8)]
			#endif
			protected Dictionary<int,InputState> _stateInputs;
			
			public Dictionary<int,InputState> stateInputs{
				get {return _stateInputs;}
			}


		   public InputSettings(){
					_stateInputs=new Dictionary<int,InputState>();
		   }
		}
		#endregion



      
    }
}


