using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaltButter.Dialogue.Editor
{
    public class DialogueView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new Vector2(x: 150, y: 200);

        private NodeSearchWindow _searchWindow;

        private Dialogue _dialogueGraph;

        public Blackboard _blackboard;

        public List<Runtime.ExposedProperty> ExposedProperties = new List<Runtime.ExposedProperty>();


        /// <summary>
        /// When we open the dialogue graph window, we generate a content dragger, a selection dragger and a rectangle select
        /// We then add the Entry Point and the EditorWindow.
        /// </summary>
        /// <param name="editorWindow"></param>
        public DialogueView(EditorWindow editorWindow)
        {
            styleSheets.Add(styleSheet: Resources.Load<StyleSheet>(path: "Dialogue"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            //We add a content dragger, selection dragger and rectangle selector
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            _dialogueGraph =editorWindow as Dialogue;

            var grid = new GridBackground();
            Insert(index: 0, grid);
            grid.StretchToParentSize();

            //We create an initial node
            AddElement(GenerateEntryPointNode());
            AddSearchWindow(editorWindow);
        }

        
        /// <summary>
        /// This will instantiate a Search Window and add it to the graph
        /// It allows us to create DialogueNodes
        /// </summary>
        /// <param name="editorWindow"></param>
        private void AddSearchWindow(EditorWindow editorWindow)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Init(this,editorWindow);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        /// <summary>
        /// Returns all the ports that can be linked to the selected one
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach((port) =>
            {
                if(startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }

            });

            return compatiblePorts;
        }


        /// <summary>
        /// /This generates the ports (little dots to dock links between nodes) for the nodes
        /// </summary>
        /// <param name="node"></param>
        /// <param name="portDirection"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        private Port GeneratePort(DialogueNode node, Direction portDirection, Port.Capacity capacity=Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); //float : arbitrary type
        }

        /// <summary>
        /// If the value was updated, we set it on the view
        /// If autosave is enabled, we save
        /// </summary>
        public void CheckIfValueWasUpdated()
        {
            //Elements
            TextField dialogue = null;
            foreach(Node node in nodes.ToList())
            {
                dialogue = node.mainContainer.Q<TextField>(name: "dialogue");
                if (dialogue != null && ((node as DialogueNode).DialogueText != dialogue.value))
                {
                    dialogue.SetValueWithoutNotify((node as DialogueNode).DialogueText);
                }
            }
            if (_dialogueGraph.GetAutoSave())
            {
                _dialogueGraph.RequestDataOperation(true);
            }
        }


        /// <summary>
        /// This instantiates the first node with basic info
        /// </summary>
        /// <returns></returns>
        private DialogueNode GenerateEntryPointNode()
        {
            var node = new DialogueNode
            {
                title = "START",
                GUID = Guid.NewGuid().ToString(),
                DialogueText = "ENTRYPOINT",
                EntryPoint = true
            };

            //We generate the port for the basic node with a single port going outwards
            var generatedPort = GeneratePort(node, Direction.Output);
            generatedPort.portName = "START";
            node.outputContainer.Add(generatedPort);
            

            node.capabilities &= ~Capabilities.Movable;
            node.capabilities &= ~Capabilities.Deletable;

            //We refresh the visuals
            node.RefreshExpandedState();
            node.RefreshPorts();

            //We set a default position
            node.SetPosition(new Rect(position: Vector2.zero, defaultNodeSize));
            return node;
        }

        /// <summary>
        /// This will create a node and add it to the graph
        /// </summary>
        /// <param name="nodeName">Title of the node</param>
        /// <param name="mousePosition">Position where to spawn the node</param>
        /// <param name="OnEnter">Value of the OnEnter Trigger</param>
        /// <param name="OnExit">Value of the OnExit Trigger</param>
        /// <param name="condition">The complete predicate as a string that will be showed</param>
        public void CreateNode(string nodeName, Vector2 mousePosition,string OnEnter= "", string OnExit = "", string condition = "")
        {
            AddElement(CreateDialogueNode(nodeName, mousePosition, OnEnter, OnExit, condition));
            
        }

        /// <summary>
        ///Creates a new node. This function is called by the toolbar
        /// </summary>
        /// <param name="nodeName">Title of the node</param>
        /// <param name="mousePosition">Position where to spawn the node</param>
        /// <param name="OnEnter">Value of the OnEnter Trigger</param>
        /// <param name="OnExit">Value of the OnExit Trigger</param>
        /// <param name="condition">The complete predicate as a string that will be showed</param>
        public DialogueNode CreateDialogueNode(string nodeName,Vector2 position, string OnEnter="", string OnExit ="", string condition = "")
        {
            //We set the values
            var dialogueNode = new DialogueNode
            {
                title = nodeName,
                DialogueText = nodeName,
                GUID = Guid.NewGuid().ToString(),
                OnEnterAction = OnEnter,
                OnExitAction = OnExit,
                condition = condition,
            };
            //We create an input port for the node to be able to follow another one
            var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            dialogueNode.inputContainer.Add(inputPort);

            //This will link the Node.uss stylesheet for us to restyle the nodes
            dialogueNode.styleSheets.Add(styleSheet: Resources.Load<StyleSheet>(path: "Node"));

            //We add a button to create choices
            var button = new Button(clickEvent: () => { AddChoicePort(dialogueNode); });
            button.text = "New Choice";
            dialogueNode.titleContainer.Add(button);

            //Dialogue field
            var textField = new TextField(label: string.Empty)
            {
                name= "dialogue"
            };
            textField.RegisterValueChangedCallback(evt=>
            {
                dialogueNode.DialogueText = evt.newValue;
                dialogueNode.title = evt.newValue;

                if (_dialogueGraph.GetAutoSave())
                {
                    Debug.Log("I Save Here");
                    _dialogueGraph.RequestDataOperation(true);
                }
            });

            textField.SetValueWithoutNotify(dialogueNode.title);

            dialogueNode.mainContainer.Add(textField);



            //We now create an accordion that will contain the condition that will filter if the choice that leads to this node is availabe or not
            var collapsableCondition = new Foldout()
            {
                text = "Conditions",
                value = false
            };

            var predicateEntry = new TextField(label: "Predicate")
            {
                name = "predicate"
            };

            //var predicateEntry = new DropdownMenu();
            //predicateEntry.InsertAction()

            predicateEntry.RegisterValueChangedCallback(evt =>
            {
                dialogueNode.condition = evt.newValue;

                if (_dialogueGraph.GetAutoSave())
                {
                    _dialogueGraph.RequestDataOperation(true);
                }
            });

           
            predicateEntry.SetValueWithoutNotify(dialogueNode.condition);

            collapsableCondition.Add(predicateEntry);

  
            dialogueNode.mainContainer.Add(collapsableCondition);





            //Another accordion to store the events' value

            var collapsable = new Foldout()
            {
                text = "Events",
                value = false
            };

            var entryEvent = new TextField(label: "On Entry")
            {
                name = "entryEvent"
            };
            entryEvent.RegisterValueChangedCallback(evt =>
            {
                dialogueNode.OnEnterAction = evt.newValue;

                if (_dialogueGraph.GetAutoSave())
                {
                    _dialogueGraph.RequestDataOperation(true);
                }
            });

            entryEvent.SetValueWithoutNotify(dialogueNode.OnEnterAction);

            collapsable.Add(entryEvent);



            var exitEvent = new TextField(label: "On Exit")
            {
                name = "entryEvent"
            };
            exitEvent.RegisterValueChangedCallback(evt =>
            {
                dialogueNode.OnExitAction = evt.newValue;

                if (_dialogueGraph.GetAutoSave())
                {
                    _dialogueGraph.RequestDataOperation(true);
                }
            });

            exitEvent.SetValueWithoutNotify(dialogueNode.OnExitAction);
            collapsable.Add(exitEvent);
            //We add everything to the node viewer
            dialogueNode.mainContainer.Add(collapsable);
            //Then refresh the view
            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();
            //And position it correctly
            dialogueNode.SetPosition(new Rect(position:position, defaultNodeSize));
            return dialogueNode;
        }


        /// <summary>
        /// Adds a choice port with the correct Text and set its behaviour to handle modification
        /// </summary>
        /// <param name="dialogueNode"></param>
        /// <param name="overridenPortName"></param>
        public void AddChoicePort(DialogueNode dialogueNode, string overridenPortName ="")
        {
            //We check the number of ports
            var outputPortCount = dialogueNode.outputContainer.Query(name: "connector").ToList().Count;

            if (outputPortCount >= 3)
                return; var generatedPort = GeneratePort(dialogueNode, Direction.Output);


            var oldLabel = generatedPort.contentContainer.Q<Label>(name:"type");
            generatedPort.contentContainer.Remove(oldLabel);

            var choicePortName = string.IsNullOrEmpty(overridenPortName) ? $"Choice {outputPortCount}" : overridenPortName;

            var textField = new TextField
            {
                name= String.Empty,
                value = choicePortName

            };

            textField.RegisterValueChangedCallback(evt => { generatedPort.portName = evt.newValue;

                if (_dialogueGraph.GetAutoSave())
                {
                    _dialogueGraph.RequestDataOperation(true);
                }
            });
            generatedPort.contentContainer.Add(new Label(" "));
            generatedPort.contentContainer.Add(textField);
            var deleteButton = new Button(() => RemovePort(dialogueNode, generatedPort)){
                text="x"
            };
            generatedPort.Add(deleteButton);
            generatedPort.portName = choicePortName;


            dialogueNode.outputContainer.Add(generatedPort);


            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();

            if (_dialogueGraph.GetAutoSave())
            {
                _dialogueGraph.RequestDataOperation(true);
            }


        }

        /// <summary>
        /// Removes a choice from a Dialogue Node
        /// </summary>
        /// <param name="dialogueNode"></param>
        /// <param name="generatedPort"></param>
        private void RemovePort(DialogueNode dialogueNode, Port generatedPort)
        {
            var targetEdge = edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);
            bool mustDeleteEdge = targetEdge.Any();


            if (mustDeleteEdge)
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }

            dialogueNode.outputContainer.Remove(generatedPort);
            dialogueNode.RefreshPorts();
            dialogueNode.RefreshExpandedState();

            if (_dialogueGraph.GetAutoSave())
            {
                _dialogueGraph.RequestDataOperation(true);

            }
        }

        /// <summary>
        /// This will delete every Exposed Property
        /// </summary>
        public void ClearBlackBoardAndExposedProperties()
        {
            ExposedProperties.Clear();
            _blackboard.Clear();
        }


        /// <summary>
        /// The Blackboard contains the values of each Exposed Property
        /// This Method allows us to create a new exposed property and adds it to the BlackBoard
        /// </summary>
        /// <param name="exposedProperty"></param>
        /// <param name="refreshing"></param>
        public void AddPropertyToBlackBoard(Runtime.ExposedProperty exposedProperty, bool refreshing = false)
        {
            var localPropertyName = exposedProperty.PropertyName;
            var localPropertyValue = exposedProperty.PropertyValue;


            while (ExposedProperties.Any(x=>x.PropertyName == localPropertyName) && !refreshing)
            {
                localPropertyName = $"{localPropertyName}(1)";
            }

            if (!refreshing)
            {
                var property = new Runtime.ExposedProperty();
                property.PropertyName = localPropertyName;
                property.PropertyValue = localPropertyValue;

                ExposedProperties.Add(property);
            }
            int j = ExposedProperties.Count-1;

            var container = new VisualElement();
            var blackboardField = new BlackboardField { text = localPropertyName, typeText = "string" };
            blackboardField.Add(new Button(() => { RemovePropertyFromBlackboard(j); }) { text = "x" });

            container.Add(blackboardField);

            var propertyValueTextField = new TextField(label: "Value:")
            {
                value = localPropertyValue
            };

            propertyValueTextField.RegisterValueChangedCallback(evt => {
                int i = j;
                
                var changingPropertyIndex = ExposedProperties.FindIndex(x => x.PropertyName == localPropertyName);


                ExposedProperties[i].PropertyValue = evt.newValue;
            });

            var blackBoardValueRow = new BlackboardRow(blackboardField,propertyValueTextField);
            container.Add(blackBoardValueRow);

            _blackboard.Add(container);
            if (_dialogueGraph.GetAutoSave())
            {
                _dialogueGraph.RequestDataOperation(true);
            }
        }

        /// <summary>
        /// This has just debugging purposes
        /// </summary>
        public void ReadProperties()
        {
            foreach(Runtime.ExposedProperty ppt in ExposedProperties)
            {
                Debug.Log(ppt.PropertyName);
            }
        }

        /// <summary>
        /// Will remove an Exposed Property with the corresponding index
        /// </summary>
        /// <param name="i"></param>
        private void RemovePropertyFromBlackboard(int i)
        {
            //ReadProperties();

            var propertyToRemove = ExposedProperties[i];
            if (propertyToRemove == null)
            {
                return;
            }

            ExposedProperties.Remove(propertyToRemove);
            List<Runtime.ExposedProperty> _tmpProperties = new List<Runtime.ExposedProperty>(ExposedProperties);


            ClearBlackBoardAndExposedProperties();
            //Add properties from data
            foreach (var exposedProperty in _tmpProperties.ToList())
            {
                //Debug.Log(exposedProperty.PropertyName);
                AddPropertyToBlackBoard(exposedProperty, false);
            }
            if (_dialogueGraph.GetAutoSave())
            {
                _dialogueGraph.RequestDataOperation(true);
            }

        }

    }
}
