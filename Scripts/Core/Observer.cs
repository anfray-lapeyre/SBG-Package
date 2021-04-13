using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Core
{
    public class Observer : MonoBehaviour
    {

        /*
         * * Note that Observer class is available to listen to only ONE subject. 
         *  The ID is unique, and marks the place of the Observer in the Subject's array
         * It is therefore not duplicable
         * 
         */

        /// <summary>
        /// Behaviour when the subject notifies the Observer
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="notifiedEvent"></param>
        virtual public void OnNotify(GameObject entity, object notifiedEvent) { }

        /// <summary>
        /// Each Observer has an ID and a reference to the subject it listens to
        /// </summary>
        [HideInInspector]
        public int ID = -1;
        public Subject subject;

        /// <summary>
        /// Removes itself from the subject's observer list before getting destroyed.
        /// </summary>
        virtual protected void OnDestroy()
        {
            if (subject != null)
            {
                subject.removeObserver(this);
            }
        }

        /// <summary>
        /// Enables using Invoke without thinking about timescale
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
