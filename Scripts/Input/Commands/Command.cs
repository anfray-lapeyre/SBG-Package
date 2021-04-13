using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Inputs.Command
{

    public class BaseCommand : MonoBehaviour
    {
        
        virtual public void execute(object value) { }


        static public string getType()
        {
            return "SaltButter.Inputs.Command.Command";
        }
    }
}
