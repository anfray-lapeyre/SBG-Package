using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SaltButter.Dialogue.Editor
{
    /// <summary>
    /// Stores all the data of a node visible in the editor.
    /// What will be saved in the end isn't this data, But the class available in Dialogue.Runtime.DialogueNodeData
    /// </summary>
    public class DialogueNode : Node
    {
        public string GUID;

        public string DialogueText;

        public bool EntryPoint = false;

        public string OnEnterAction;

        public string OnExitAction;

        public string condition;
    }
}
