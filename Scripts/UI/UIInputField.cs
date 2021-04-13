using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.UI
{
    public class UIInputField : UIButton
    {
        public TMPro.TextMeshProUGUI text;
        public UIStateMachine stateMachine;

        bool listensToKeyboard = false;
        // Start is called before the first frame update
        void Start()
        {
            text.text = "LOCALHOST";
        }

        // Update is called once per frame
        void Update()
        {
            if (!isActive || actualState != SELECTED || !listensToKeyboard)
            {
                return;
            }
            //If we reach here, the text is active, so we add an underscore
            if (text.text[text.text.Length - 1] != '_')
            {
                text.text += '_';
            }

            if (text.text.Length < 16)
            {
                KeyCode a;
                for (int i = 0; i < KeyCode.Z - KeyCode.A + 1; i++)
                {
                    a = (KeyCode)(KeyCode.A + i);
                    if (Input.GetKeyDown(a))
                    {
                        //We delete the underscore
                        text.text = text.text.Substring(0, text.text.Length - 1);
                        text.text += a.ToString();
                        //And then put it back
                        text.text += "_";
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Backspace) && text.text.Length > 0)
            {
                //We delete the underscore and last character
                text.text = text.text.Substring(0, text.text.Length - 2);
                //Then add underscore back
                text.text += "_";
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                GiveBackAuthority();
            }
        }


        override public void ExecuteFunction()
        {
            if (isActive)
            {
                stateMachine.StopObserving();
                colorTransition(pressedColor);
                Invoke("setListen", 0.1f);
            }
        }

        public void setListen()
        {
            listensToKeyboard = true;
        }
        private void GiveBackAuthority()
        {
            //If we aren't active, we make sure the undescore isn't here anymore
            if (text.text[text.text.Length - 1] == '_')
            {
                text.text = text.text.Substring(0, text.text.Length - 1);
            }
            stateMachine.StartObserving();
            listensToKeyboard = false;
            colorTransition(selectedColor);

        }

        override public void unPress()
        {
        }
    }
}