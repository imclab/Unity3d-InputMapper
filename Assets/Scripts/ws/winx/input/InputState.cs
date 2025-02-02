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
using System.Runtime.Serialization;

namespace ws.winx.input
{
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_ANDROID) && !UNITY_WEBPLAYER
		[DataContract]
#endif
    public class InputState
    {
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_ANDROID) && !UNITY_WEBPLAYER
				[DataMember(Name = "InputCombinations")]
#endif
        public InputCombination[] combinations;

        ///
        public Animator animator
        {
            get
            {
                return _animator;
            }
        }

        public int hash
        {
            get
            {
                return _hash;
            }
        }

        public string name
        {
            get
            {
                return _name;
            }
            //set{ _name=value; }

        }

#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_ANDROID) && !UNITY_WEBPLAYER
                [DataMember(Name = "Name")]
#endif
              
        protected string _name;

#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_ANDROID) && !UNITY_WEBPLAYER
				[DataMember(Name = "Hash")]
#endif
        protected int _hash;
        protected Animator _animator;
        protected int _count = 0;

        public InputState(string name, int hash)
        {
            this._name = name;
            this._hash = hash;
            this.combinations = new InputCombination[2];
        }

        public void Add(InputCombination combination, int inx = -1)
        {



            if (inx > 0)
                this.combinations[inx] = combination;
            else
                this.combinations[_count++] = combination;
        }
    }
}

