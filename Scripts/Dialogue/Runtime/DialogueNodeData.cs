using SaltButter.Core;
using System;
using System.Collections.Generic;
using UnityEngine;



namespace SaltButter.Dialogue.Runtime
{
    /// <summary>
    /// This is the Data that will be stored for every node
    /// </summary>
    [Serializable]
    public class DialogueNodeData
    {
        public string GUID;
        public string DialogueText;
        public Vector2 Position;
        public string OnEnterAction;
        public string OnExitAction;
        public Condition condition;
        public bool CheckCondition(IEnumerable<IPredicateEvaluator> evaluators)
        {
             return condition.Check(evaluators);
        }
    }
}
