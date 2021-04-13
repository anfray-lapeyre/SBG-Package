using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Saving
{
    public interface ISaveable
    {
        object CaptureState();


        void RestoreState(object state);

    }
}