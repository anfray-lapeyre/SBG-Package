using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SaltButter.Core
{
    public class Subject : MonoBehaviour
    {

        protected Observer[] observers;
        protected int numObservers = 0;
        /// <summary>
        /// Will initialize the observer array;
        /// </summary>
        virtual protected void Awake()
        {
            if (observers == null)
                observers = new Observer[20];
        }

        /// <summary>
        /// Adds an observer to the list of objects to notify
        /// </summary>
        /// <param name="_observer"></param>
        virtual public void addObserver(Observer _observer)
        {
            if (observers == null)
            {
                Awake();
            }
            if (_observer.ID < 0)
            {
                //Debug.Log("numObservers : " + numObservers + " observer asking for addition : " + _observer.name + " parent : " + ((_observer.transform.parent != null) ? _observer.transform.parent.gameObject.name : "null"));

                observers[numObservers] = _observer;
                _observer.ID = numObservers;
                _observer.subject = this;
                numObservers++;
                //_observer.waitingforButtontoUnpress = true;
            }
            else
            {
                Debug.Log("This Observer is already observing a subject ! ");
            }
        }

        /// <summary>
        /// Will remove an observer from the array and replace it with the lase one on the array to avoid having blanks 
        /// </summary>
        /// <param name="_observer"></param>
        virtual public void removeObserver(Observer _observer)
        {
            if (_observer.ID >= 0)
            {
                //We get the observers ID
                int newID = _observer.ID;
                numObservers--;
                //Then replace it with the last observer
                observers[newID] = observers[numObservers];
                //And change its own ID
                observers[newID].ID = newID;
                //And we reset the one removed
                _observer.subject = null;
                _observer.ID = -1;
            }
        }

        /// <summary>
        /// Sends a notification to all observers
        /// </summary>
        /// <param name="notifiedEvent"></param>
        virtual public void Notify(object notifiedEvent)
        {
            for (int i = numObservers - 1; i >= 0; i--)
            {
                observers[i].OnNotify(this.gameObject, notifiedEvent);
            }

        }

        /// <summary>
        /// Enables the us of Invoke without having to consider the timescale
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="delay"></param>
        protected void InvokeRealTime(string functionName, float delay)
        {
            StartCoroutine(InvokeRealTimeHelper(functionName, delay));
        }


        private IEnumerator InvokeRealTimeHelper(string functionName, float delay)
        {
            float timeElapsed = 0f;
            while (timeElapsed < delay)
            {
                timeElapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            SendMessage(functionName);
        }
    }
}
