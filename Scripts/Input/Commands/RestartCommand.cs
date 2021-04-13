using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SaltButter.Inputs.Command
{

    public class RestartCommand : BaseCommand
    {
        private bool pressed = false;
        override public void execute(object value)
        {
            InputValue input = value as InputValue;
            //This is a button type so we can use isPressed

            pressed = (input.Get<float>() > 0.05f);
            //Debug.Log("RESTART : " + pressed);
        }

        public bool isPressed()
        {
            return pressed;
        }

        public new static string getType()
        {
            return "SaltButter.Inputs.Command.RestartCommand";
        }


    }
}
