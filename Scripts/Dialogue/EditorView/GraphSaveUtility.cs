using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SaltButter.Dialogue.Runtime;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using UnityEditor;
using System;
using UnityEngine.UIElements;

namespace SaltButter.Dialogue.Editor
{

    /// <summary>
    /// This utility class will gather all info from the editor view and will store it in a Serialized Asset that will be the object Dialogue that we see in the Project view
    /// </summary>
    public class GraphSaveUtility 
    {
        /// <summary>
        /// We need these references to gather all the data we need,
        /// Thoses references will be generated by themselves
        /// </summary>
        private DialogueView _targetGraphView;
        private Dialogue _targetGraph;
        private DialogueContainer _containerCache;

        /// <summary>
        /// We have a list of all edges and nodes there is.
        /// This way we know easily when something changes
        /// </summary>
        private List<Edge> Edges => _targetGraphView.edges.ToList();

        private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

        /// <summary>
        /// Create the GraphUtility Instance with te right view and data
        /// </summary>
        /// <param name="targetGraphView"></param>
        /// <param name="targetGraph"></param>
        /// <returns></returns>
        public static GraphSaveUtility GetInstance(DialogueView targetGraphView, Dialogue targetGraph)
        {
            return new GraphSaveUtility
            {
                _targetGraphView = targetGraphView,
                _targetGraph = targetGraph
            };
        }

        /// <summary>
        /// This will take all the nodes, node links (edges) and exposed properties to save them in a Serialized DialogueContainer format
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveGraph(string fileName)
        {
            var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();


            dialogueContainer.blackBoardRect = _targetGraph.GetBlackBoardRect();
            dialogueContainer.minimapRect = _targetGraph.GetMiniMapRect();

            if (!SaveNodes(dialogueContainer))
            {
                return;
            }

            SaveExposedProperties(dialogueContainer);

            //Auto creates the folders we need
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder(parentFolder: "Assets", newFolderName: "Resources");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Dialogues"))
            {
                AssetDatabase.CreateFolder(parentFolder: "Assets/Resources", newFolderName: "Dialogues");
            }

            
            if (String.IsNullOrEmpty(AssetDatabase.GetAssetPath(dialogueContainer)))
            {
                AssetDatabase.CreateAsset(dialogueContainer, path: $"Assets/Resources/Dialogues/{fileName}.asset");

            }

            EditorUtility.SetDirty(dialogueContainer);

            AssetDatabase.SaveAssets();
            Selection.activeObject = dialogueContainer;
            EditorUtility.FocusProjectWindow();
        }

        /// <summary>
        /// The same as the previous SaveGraph method but with a prefilled DialogueContainer
        /// </summary>
        /// <param name="dialogueContainer"></param>
        public static void SaveGraph(DialogueContainer dialogueContainer)
        {
            //we just set it dirty and save
            EditorUtility.SetDirty(dialogueContainer);

            AssetDatabase.SaveAssets();
            Selection.activeObject = dialogueContainer;

        }



        /// <summary>
        /// This will get all the data from a Dialogue Container and will translate it to a usable graph and view
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadGraph(string fileName)
        {
            _containerCache = Resources.Load<DialogueContainer>($"Dialogues/{fileName}");

            if (_containerCache == null)
            {
                EditorUtility.DisplayDialog(title:"File Not Found",message: "Target dialogue graph file does not exist",ok: "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
            CreateExposedProperties();

            _targetGraph.SetBlackBoardRect(_containerCache.blackBoardRect);
            _targetGraph.SetMiniMapRect(_containerCache.minimapRect);
        }

        /// <summary>
        /// This will get all the data from a Dialogue Container and will translate it to a usable graph and view
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadGraph(DialogueContainer dialogue)
        {
            if (dialogue == null)
                return;
            _containerCache = dialogue;

            ClearGraph();
            CreateNodes();
            ConnectNodes();
            CreateExposedProperties();

            _targetGraph.SetBlackBoardRect(_containerCache.blackBoardRect);
            _targetGraph.SetMiniMapRect(_containerCache.minimapRect);

        }


        /// <summary>
        /// This will clean the graph (before loading a new graph for example
        /// </summary>
        private void ClearGraph()
        {
            if (Nodes == null || Nodes.Count <= 0 || _containerCache.NodeLinks.Count<=0)
                return;

            //Set entry points guid back from the save. Discard existing guid.
            Nodes.Find(match: x => x.EntryPoint).GUID= _containerCache.NodeLinks[0].BaseNodeGuid;


            foreach (var node in Nodes)
            {
                //Remove edges that connected to this node
                if (node.EntryPoint) continue;
                Edges.Where(x => x.input.node == node).ToList()
                    .ForEach(edge => _targetGraphView.RemoveElement(edge));

                //Then remove the node
                _targetGraphView.RemoveElement(node);
            }


        }

        /// <summary>
        /// Creates all the nodes with the saved graph data
        /// </summary>
        private void CreateNodes()
        {
            foreach(var nodeData in _containerCache.dialogueNodeData)
            {
                var tempNode = _targetGraphView.CreateDialogueNode(nodeData.DialogueText, Vector2.zero, nodeData.OnEnterAction, nodeData.OnExitAction, (nodeData.condition != null) ? nodeData.condition.GetPredicate() : "");
                tempNode.GUID = nodeData.GUID;
                tempNode.OnEnterAction = nodeData.OnEnterAction;
                tempNode.OnExitAction = nodeData.OnExitAction;

                _targetGraphView.AddElement(tempNode);

                var nodePorts = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == nodeData.GUID).ToList();
                nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.PortName));
            }
        }

        /// <summary>
        /// Creates all the exposed properties with the saved graph data
        /// </summary>
        private void CreateExposedProperties()
        {
            //Clear existing properties and add correct ones
            _targetGraphView.ClearBlackBoardAndExposedProperties();

            foreach (var exposedProperty in _containerCache.ExposedProperties)
            {
                _targetGraphView.AddPropertyToBlackBoard(exposedProperty);
            }

        }

        /// <summary>
        /// Creates all connections between nodes with the saved graph data
        /// </summary>
        private void ConnectNodes()
        {
            for(var i=0; i<Nodes.Count; i++)
            {
                var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].GUID).ToList();
                for(var j=0; j < connections.Count; j++)
                {
                    var targetNodeGuid = connections[j].TargetNodeGuid;
                    var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
                    LinkNodes(output: Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(newPos: new Rect(_containerCache.dialogueNodeData.First(x => x.GUID == targetNodeGuid).Position,_targetGraphView.defaultNodeSize));
                }
            }
        }

        /// <summary>
        /// Links a port with an other
        /// </summary>

        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };

            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            _targetGraphView.Add(tempEdge);


        }

        /// <summary>
        /// Checks Nodes validity and translates them in a saveable data (DialogueNodeData and NodeLinkData for the edges)
        /// </summary>
        private bool SaveNodes(DialogueContainer dialogueContainer)
        {
            var entryPoint = Edges.Find(x => x.output.node.title == "START");

            if (entryPoint == null)

            {

                EditorUtility.DisplayDialog("Error", "Start Node must be connected before saving.", "OK");
                if (_targetGraph.GetAutoSave())
                {
                    _targetGraph.Close();
                }

                return false;

            }

            if (!Edges.Any())//Is there any connection made ? If not, return, that way we do not save anything
            {
                return false;
            }


            //We get only connections in the input ports, since the output ports will be stored in another nodes' input port
            var connectedPorts = Edges.Where(x => x.input.node != null).OrderByDescending(x => ((DialogueNode)(x.output.node)).EntryPoint).ToArray();;
            for(int i=0; i < connectedPorts.Length; i++)
            {
                var outputNode = connectedPorts[i].output.node as DialogueNode;
                var inputNode = connectedPorts[i].input.node as DialogueNode;

                dialogueContainer.NodeLinks.Add(item: new NodeLinkData
                {
                    BaseNodeGuid = outputNode.GUID,
                    PortName = connectedPorts[i].output.portName,
                    TargetNodeGuid = inputNode.GUID
                });
            }

            foreach (var dialogueNode in Nodes.Where(node => !node.EntryPoint))
            {
                //If the condition is not null nor empty, and it is not a handled predicate, we say stop
                if (!Core.PredicateHelper.IsWholePredicateValid(dialogueNode.condition))
                {
                    string errorMessage= "Error, current condition for node '"+dialogueNode.name+"' isnt valid. Valid conditions are : ";
                    foreach(string type in Enum.GetNames(typeof(Core.PredicateHelper.predicateType)))
                    {
                        errorMessage += type + ", ";
                    }

                    errorMessage += "please chose one of them before saving, or set it up to nothing. You can also use boolean operators &&, ||, ! and brackets ().";

                    EditorUtility.DisplayDialog(title: "Condition not valid", message: errorMessage, ok: "OK");
                    return false;
                }

                //We make sure that the node's data was created and updated
                DialogueNodeData data = new DialogueNodeData()
                {
                    DialogueText = dialogueNode.DialogueText,
                    GUID = dialogueNode.GUID,
                    Position = new Vector2(dialogueNode.GetPosition().x, dialogueNode.GetPosition().y),
                    OnEnterAction = dialogueNode.OnEnterAction,
                    OnExitAction = dialogueNode.OnExitAction,
                    condition = new Core.Condition(dialogueNode.condition,null)
                };
                //Then add it to the save container
                dialogueContainer.dialogueNodeData.Add(item: data);
            }
            return true;
        }

        /// <summary>
        /// Adds the exposed properties to the list contained in the Dialogue Container file
        /// </summary>
        /// <param name="dialogueContainer"></param>
        private void SaveExposedProperties(DialogueContainer dialogueContainer)
        {
            dialogueContainer.ExposedProperties.AddRange(_targetGraphView.ExposedProperties);
        }

    }
}
