using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Core
{
    public class makeObjectPersistentScript : MonoBehaviour
    {

        private static makeObjectPersistentScript instance = null;
        public static makeObjectPersistentScript Instance
        {
            get { return instance; }
        }


        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                instance = this;
            }
            transform.parent = null;
            DontDestroyOnLoad(this.gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
