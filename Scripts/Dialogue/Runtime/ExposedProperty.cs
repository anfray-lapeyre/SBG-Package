using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Dialogue.Runtime
{
    /// <summary>
    /// An Exposed Property is a name and a value.
    /// Ex : username -> Godfrey
    /// </summary>
    [Serializable]
    public class ExposedProperty
    {
        public string PropertyName = "New String";
        public string PropertyValue = "New Value";
    }
}
