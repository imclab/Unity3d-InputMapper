﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;


namespace ws.winx.unity
{
    public class LoaderEvtArgs : EventArgs
    {


        public readonly object data;
        

        public LoaderEvtArgs(object data)
        {
            this.data = data;

        }
    }


	public class Loader:IDisposable
	{
        public event EventHandler<LoaderEvtArgs> LoadComplete;
        public event EventHandler<LoaderEvtArgs> Error;
        public event EventHandler<LoaderEvtArgs> LoadItemComplete;


        protected bool _isRunning=false;

        private static List<WWW> _wwwList;
        private static List<WWW> _queueList;

        protected static List<WWW> queueList
        {
            get { if (_queueList == null) _queueList = new List<WWW>(); return _queueList; }
        }

        protected static List<WWW> wwwList{
            get{ if(_wwwList==null) _wwwList=new List<WWW>(); return _wwwList; }
        }

        private static MonoBehaviour _behaviour;


        protected static MonoBehaviour behaviour
        {
            get { if (_behaviour == null) { _behaviour = (new GameObject("WWWRequest")).AddComponent<MonoBehaviour>(); } return _behaviour; }
        }

        public void load()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                behaviour.StartCoroutine(check());
                if (wwwList.Count != queueList.Count)
                {
                     foreach (WWW www in queueList)
                     {
                        queueList.Add(www);
                     }
                }
            }
        }

      

        public void Add(string url){
            if (Application.isEditor)
                url = "file:///" + url;

            WWW w = new WWW(url);
            Add(w);

        }

        public void Add(WWW www)
        {
           
            wwwList.Add(www);
            queueList.Add(www);

        }


       


       
        

        IEnumerator check()
        { 
            int i;
			WWW www;
            while (true)
            {
               i=0;

               while (queueList.Count>i)
				{

					www=queueList.ElementAt(i);
                    if (www.isDone)
                    {
                        if (!String.IsNullOrEmpty(www.error))
                        {


                            if (Error != null)
                            {
                                //Error(this,new LoaderEvtArgs(www));
                                Dispatch(Error, new LoaderEvtArgs(www));
                            }
                            else
                            {
                                Debug.LogError(www.error);
                            }

							
                           

                        }else 
							if (LoadItemComplete != null)
                                Dispatch(LoadItemComplete, new LoaderEvtArgs(www));
								//LoadItemComplete(this, new LoaderEvtArgs(www));
                       
                        queueList.RemoveAt(i);
                    }

                    i++;


                }

               if (queueList.Count == 0)
               {
                   _isRunning = false;

                   if (LoadComplete != null)
                       //Dispatch(LoadComplete, new LoaderEvtArgs(wwwList));
                        LoadComplete(this, new LoaderEvtArgs(wwwList));

                    yield break; 
               }


                yield return new WaitForSeconds(0.5f);
            }
        }


        private void EndAsyncEvent(IAsyncResult iar)
        {
            var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
            var invokedMethod = (EventHandler<LoaderEvtArgs>)ar.AsyncDelegate;

            try
            {
                invokedMethod.EndInvoke(iar);
            }
            catch
            {
                // Handle any exceptions that were thrown by the invoked method
                Console.WriteLine("An event listener went kaboom!");
            }
        }


        protected void Dispatch(Delegate del,LoaderEvtArgs args)
        {
            Delegate[] delegates = del.GetInvocationList();
            foreach (Delegate d in delegates)
                ((EventHandler<LoaderEvtArgs>)d).BeginInvoke(this, args, EndAsyncEvent, null);
        }
	
public void  Dispose()
{
    if(_queueList!=null) _queueList.Clear();
    if(_wwwList!=null) _wwwList.Clear();

 	
}
}
}
