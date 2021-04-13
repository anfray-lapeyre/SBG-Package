using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Saving
{
    public class SavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile="save";

        private IEnumerator Start()
        {
            //Fade out completely
            //Use your own code to fade :)
            yield return GetComponent<SavingSystem>().LoadLastScene(defaultSaveFile);
            //Fade in
            //Use your own code to fade :)
        }

        private void Update()
        {
            //AutoSave
            if (UnityEngine.Input.GetKeyDown(KeyCode.F11))
            {
                Save();
            }

            //AutoLoad
            if (UnityEngine.Input.GetKeyDown(KeyCode.F12))
            {
                Load();
            }
        }

        public void Load()
        {
            GetComponent<SavingSystem>().Load(defaultSaveFile);
        }

        public void Save()
        {
            GetComponent<SavingSystem>().Save(defaultSaveFile);
        }
    }
}