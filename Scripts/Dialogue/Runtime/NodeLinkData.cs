using System;
using UnityEngine;

namespace SaltButter.Dialogue.Runtime
{
    /// <summary>
    /// This is a data that is store for every choices
    /// It contains the base port, the target port, and the text
    /// </summary>
    [Serializable]
    public class NodeLinkData
    {
        public string BaseNodeGuid;
        public string PortName;
        public string TargetNodeGuid;

    }
}
