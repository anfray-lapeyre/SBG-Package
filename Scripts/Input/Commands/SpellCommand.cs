using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SaltButter.Inputs.Command
{

    public class SpellCommand : BaseCommand
    {
        protected bool pressed = false;
        override public void execute(object value)
        {

            InputValue input = value as InputValue;

            pressed = (input.Get<float>() > 0.05f);
            //Debug.Log("SPELL : " + pressed);
        }

        public bool isPressed()
        {
            return pressed;
        }

        public new static string getType()
        {
            return "SaltButter.Inputs.Command.SpellCommand";
        }
    }
}