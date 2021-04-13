using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SaltButter.Dialogue.Runtime
{

    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] string action;
        [SerializeField] UnityEvent onTrigger;

        /// <summary>
        /// The DialogueTrigger only triggers the onTrigger UnityEvent the actions set as a parameter.
        /// </summary>
        /// <param name="_action"></param>
        public void Trigger(string _action)
        {
            if(action == _action)
            {
                onTrigger.Invoke();
            }
        }
    }
}
