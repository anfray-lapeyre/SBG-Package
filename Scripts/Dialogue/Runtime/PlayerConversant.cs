using SaltButter.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace SaltButter.Dialogue.Runtime
{
    //This script needs to be put on the player
    public class PlayerConversant : MonoBehaviour
    {
        DialogueContainer currentDialogue=null;
        AIConversant currentConversant = null;
        DialogueNodeData currentNode=null;

        public event Action OnConversationUpdated;

        /// <summary>
        /// Starts a Dialogue based on an AIConversant data
        /// </summary>
        /// <param name="_currentConversant"></param>
        /// <param name="_currentDialogue"></param>
        public void StartDialogue(AIConversant _currentConversant,DialogueContainer _currentDialogue)
        {
            if (_currentDialogue != null && _currentConversant != null)
            {
                currentDialogue = _currentDialogue;
                currentConversant = _currentConversant;
                currentNode = currentDialogue.GetRootNode();
                if(OnConversationUpdated != null)
                    OnConversationUpdated();
                TriggerEnterAction();
            }

        }

        /// <summary>
        /// Quits the dialogues (close the UI)
        /// </summary>
        public void QuitDialogue()
        {
            currentDialogue = null;
            TriggerExitAction();
            currentConversant = null;
            currentNode = null;
            OnConversationUpdated();
        }


        public bool isActive()
        {
            return currentDialogue != null;
        }

        /// <summary>
        /// Sets an exposed property to the wanted value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="save"></param>
        /// <returns></returns>
        public bool SetExposedProperty(string name, string value, bool save =false)
        {
            return currentDialogue.SetExposedProperty(name, value, save);
        }

        /// <summary>
        /// Gets the text from the current node from current dialogue
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            if (currentNode == null)
            {
                return string.Empty;
            }
            string text = currentNode.DialogueText;
            return ReplaceWithExposedPropety(text);
        }

        /// <summary>
        /// Gets the AiConversant's name
        /// </summary>
        /// <returns></returns>
        public string GetCurrentConversant()
        {
            return currentConversant.CharacterName;
        }

        /// <summary>
        /// Replaces the exposed property when we get the text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string ReplaceWithExposedPropety(string text)
        {
            string finalText = "";
            //We make sure that at least once, both of the chars are contained and they are in the right order
            if (text.Contains("{") && text.Contains("}") && text.IndexOf('{') < text.IndexOf('}'))
            {

                //If so, we start by extracting the first part of the string (before the {)
                int startIndex = text.IndexOf('{');
                int endIndex = text.IndexOf('}');
                while (text.Contains("{") && text.Contains("}") && startIndex < endIndex)
                {
                    finalText += text.Substring(0, startIndex);
                    finalText += currentDialogue.GetExposedProperty(text.Substring(startIndex + 1, endIndex - startIndex - 1));
                    //We crop the text
                    text = text.Substring(endIndex + 1);
                    //And get the new indexes
                    startIndex = text.IndexOf('{');
                    endIndex = text.IndexOf('}');
                }
                //When we finish, we have a final value in 'text' that does not contain any value to replace, so we add it
                finalText += text;
            }
            else
            {
                finalText = text;
            }

            return finalText;
        }

        /// <summary>
        /// Goes to the next node base on a choice
        /// </summary>
        /// <param name="choice"></param>
        public void Next(string choice)
        {
            List<string> choicesWithoutExposed = GetChoices(false);
            List<string> choices = new List<string>();
            for (int i = 0; i < choicesWithoutExposed.Count; i++)
            {
                choices.Add(ReplaceWithExposedPropety(choicesWithoutExposed[i]));
            }

            for (int i=0;i<=choices.Count;i++) 
            {

                if (choices[i] == choice)
                {
                    TriggerExitAction();
                    currentNode = currentDialogue.GetNextNode(currentNode.GUID, choicesWithoutExposed[i]);
                    TriggerEnterAction();
                    OnConversationUpdated();
                    return;
                }

            }
            //In case we reach here
            OnConversationUpdated();
        }

        /// <summary>
        /// Filters the choices that will be shown based on the conditions set on nodes.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAndFilterChoices()
        {
            //We filter the authorized links based on set conditions in the dialogue
            List<NodeLinkData> links = FilterOnCondition(currentDialogue.GetChoicesForNodeAsLinks(currentNode.GUID));

            //Then we translate them to choice strings
            List<string> choices = new List<string>();
            foreach (NodeLinkData data in links)
            {
                choices.Add(data.PortName);
            }
            return choices;
        }
        

        /// <summary>
        /// Gets all the possible choices
        /// </summary>
        /// <param name="exposed"></param>
        /// <returns></returns>
        public List<string> GetChoices(bool exposed = true)
        {
            List<string> choices = GetAndFilterChoices();

            if (!exposed)
                return choices;
            for(int i=0;i<choices.Count;i++)
            {
                choices[i] = ReplaceWithExposedPropety(choices[i]);
            }
            return choices;
        }

        /// <summary>
        /// Checks if there is a next node
        /// </summary>
        /// <returns></returns>
        public bool hasNext()
        {

            return GetAndFilterChoices().Count>0;
        }

        /// <summary>
        /// Triggers the OnEnterAction of the Node
        /// </summary>
        private void TriggerEnterAction()
        {
            if(currentNode != null )
            {
                TriggerAction(currentNode.OnEnterAction);

            }
        }

        /// <summary>
        /// Triggers the OnExitAction of the Node
        /// </summary>
        private void TriggerExitAction()
        {
            if (currentNode != null)
            {
                TriggerAction(currentNode.OnExitAction);
            }
        }

        /// <summary>
        /// This will send to all dialogue triggers the action to trigger
        /// </summary>
        /// <param name="action"></param>
        private void TriggerAction(string action)
        {
            if (currentNode.OnExitAction == string.Empty)
                return;

            foreach(DialogueTrigger trigger in currentConversant.GetComponents<DialogueTrigger>())
            {
                trigger.Trigger(action);
            }
        }


        /// <summary>
        /// Triggers the conditions checking methodes for every choice nodes
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        private List<NodeLinkData> FilterOnCondition(List<NodeLinkData> links)
        {
            //We get the node data for each link
            List<DialogueNodeData> inputNodes = new List<DialogueNodeData>();
            foreach(NodeLinkData nodeLink in links)
            {
                inputNodes.Add(currentDialogue.GetNode(nodeLink.TargetNodeGuid));
            }
            List<DialogueNodeData> nodesToRemove = new List<DialogueNodeData>(); ;

            //We treat and filter the data

            foreach (DialogueNodeData node in inputNodes)
            {
                if (!node.CheckCondition(GetEvaluators()))
                {
                    nodesToRemove.Add(node);
                }
            }

            foreach(DialogueNodeData node in nodesToRemove)
            {
                inputNodes.Remove(node);
            }

            //We filter the links correspondingly
            List<NodeLinkData> newLinks = new List<NodeLinkData>();
            foreach (DialogueNodeData data in inputNodes)
            {
                NodeLinkData link = links.Find(x =>  x.TargetNodeGuid == data.GUID);
                if(link != null)
                    newLinks.Add(link);
            }
            return newLinks;
        }

        private IEnumerable<IPredicateEvaluator> GetEvaluators()
        {
            return GetComponents<IPredicateEvaluator>();
        }
    }
}
