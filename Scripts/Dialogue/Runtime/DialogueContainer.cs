using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SaltButter.Dialogue.Runtime
{


    /// <summary>
    /// This is the whole data that will be saved for a dialogue.
    /// It contains every node, choices and exposed properties of a dialogue.
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 0)]

    public class DialogueContainer : ScriptableObject
    {
        public Rect blackBoardRect= new Rect(0,0,200,300);
        public Rect minimapRect= new Rect(0,0,200,200);
        
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
        public List<DialogueNodeData> dialogueNodeData = new List<DialogueNodeData>();
        public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();

       
        internal DialogueNodeData GetNextNode(string gUID, string portName)
        {
            var nodelink = NodeLinks.Find(x => x.BaseNodeGuid == gUID && x.PortName == portName);
            return dialogueNodeData.Find(x => x.GUID == nodelink.TargetNodeGuid);
        }

        internal List<string> GetChoicesForNode(string gUID)
        {

            var nodelinks = NodeLinks.FindAll(x => x.BaseNodeGuid == gUID);
            List<string> choices = new List<string>();

            foreach(NodeLinkData link in nodelinks)
            {
                choices.Add(link.PortName);
            }
            return choices;
        }

        internal List<NodeLinkData> GetChoicesForNodeAsLinks(string gUID)
        {
            return NodeLinks.FindAll(x => x.BaseNodeGuid == gUID);
        }

        internal DialogueNodeData GetNode(string gUID)
        {
            return dialogueNodeData.Find(x => x.GUID == gUID);
        }

        public DialogueNodeData GetRootNode()
        {
            var entryPoint = NodeLinks.Find(x => x.PortName == "START");

            DialogueNodeData wantedNode = dialogueNodeData.Find(x => x.GUID == entryPoint.TargetNodeGuid);
            return wantedNode;
        }

        internal string GetExposedProperty(string v)
        {
            foreach(ExposedProperty p in ExposedProperties)
            {
                if (p.PropertyName == v)
                {
                    return p.PropertyValue;
                }
            }
            //If we reach here, we found nothing, we return the base text with { }
            return '{'+v+'}';
        }

        public bool SetExposedProperty(string name, string value, bool save=false)
        {
            foreach (ExposedProperty p in ExposedProperties)
            {
                if (p.PropertyName == name)
                {
                    p.PropertyValue = value;
                    if (save)
                    {
                        Editor.GraphSaveUtility.SaveGraph(this);
                    }
                    return true;
                }
            }
            //If we reach here, we found nothing, we return false

            return false;
        }
    }

}
