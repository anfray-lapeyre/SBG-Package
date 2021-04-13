using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Callbacks;
using SaltButter.Dialogue.Runtime;
using System.Linq;

namespace SaltButter.Dialogue.Editor
{
    public class Dialogue : EditorWindow
    {
        private DialogueView _graphView;
        private string _fileName = "New Narrative";
        private bool _autoSave = false;

        /// <summary>
        /// Sets The Filename that will be saved in the Resource Folder
        /// </summary>
        /// <param name="newName"></param>
        internal void SetFileName(string newName)
        {
            _fileName = newName;
        }


        /// <summary>
        /// If is autosave
        /// </summary>
        /// <returns></returns>
        internal bool GetAutoSave()
        {
            return _autoSave;
        }


        /// <summary>
        /// This method will open the graph without any resource opened
        /// It can be opened by going in "Tools" -> Dialogue Graph 
        /// </summary>
        [MenuItem("Tools/Dialogue Graph %#d")]
        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<Dialogue>();
            window.titleContent = new GUIContent(text: "Dialogue Graph");
            
        }

        /// <summary>
        /// Opens a dialogue graph base on the object's name
        /// </summary>
        /// <param name="fileName"></param>
        public static void OpenDialogueGraphWindow(string fileName)
        {
            var window = GetWindow<Dialogue>();
            window.titleContent = new GUIContent(text: "Dialogue Graph");
            window.SetFileName(fileName);
            window.OnEnable();
        }


        /// <summary>
        /// This will be invoked when we double click on a Dialogue Asset.
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Runtime.DialogueContainer dialogue = EditorUtility.InstanceIDToObject(instanceID) as SaltButter.Dialogue.Runtime.DialogueContainer;
            if (dialogue == null)
            {
                return false;
            }

            OpenDialogueGraphWindow(EditorUtility.InstanceIDToObject(instanceID).name);
            return true;
        }


        /// <summary>
        /// When the graph is enabled, we build the graph view, the blackboard element, the toolbar on top and the minimap
        /// </summary>
        private void OnEnable()
        {
            var toolbar = rootVisualElement.Q<Toolbar>();
            if(toolbar != null)
            {
                rootVisualElement.Remove(toolbar);
            }

            ConstructGraphView();
            GenerateToolBar();
            GenerateMiniMap();
            GenerateBlackBoard();

            if(Resources.Load<SaltButter.Dialogue.Runtime.DialogueContainer>($"Dialogues/{_fileName}") != null)
            {
                var saveUtility = GraphSaveUtility.GetInstance(_graphView, this);
                saveUtility.LoadGraph(_fileName);
            }
        }


        /// <summary>
        /// When the window is focused, we check if something was updated
        /// </summary>
        private void OnFocus()
        {
            if (_graphView == null)
                return;
            _graphView.CheckIfValueWasUpdated();
        }


        /// <summary>
        /// Generates a Blackboard for the Exposed Properties
        /// </summary>
        private void GenerateBlackBoard()
        {
            var blackboard = new Blackboard(_graphView) { subTitle = "Exposed Properties" };
            blackboard.Add(new BlackboardSection());
            blackboard.addItemRequested = _blackboard => { _graphView.AddPropertyToBlackBoard(new ExposedProperty()); };
            blackboard.editTextRequested = (blackboard1, element, newValue) =>
            {
                var oldPropertyName = ((BlackboardField)element).text;
                if (_graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "This property name already exists, please chose another one!", "OK");

                    return;
                }

                var propertyIndex = _graphView.ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
                _graphView.ExposedProperties[propertyIndex].PropertyName = newValue;

                ((BlackboardField)element).text = newValue;

                if (_autoSave)
                {
                    RequestDataOperation(true);
                }
            };
            
            _graphView._blackboard = blackboard;
            _graphView.Add(blackboard);
        }


        /// <summary>
        /// Gets the Rect of the BlackBoard
        /// </summary>
        /// <returns></returns>
        public Rect GetBlackBoardRect()
        {
            var bb = _graphView.contentContainer.Q<Blackboard>();
            return bb.GetPosition();

        }


        /// <summary>
        /// Sets the Rect of the BlackBoard, used when we load for example.
        /// </summary>
        /// <param name="rect"></param>
        public void SetBlackBoardRect(Rect rect)
        {
            var bb = _graphView.contentContainer.Q<Blackboard>();
            bb.SetPosition(rect);
        }

        /// <summary>
        /// Generates the minimap
        /// </summary>
        private void GenerateMiniMap()
        {
            var minimap = new MiniMap { anchored = false };
            var cords = minimap.GetPosition();
            cords.width = 200;
            cords.height = 140;
            minimap.SetPosition(cords);
            _graphView.Add(minimap);
        }

        /// <summary>
        /// Get the Minimap's Rect
        /// </summary>
        /// <returns></returns>
        public Rect GetMiniMapRect()
        {
            var mm = _graphView.contentContainer.Q<MiniMap>();
            return mm.GetPosition();

        }


        /// <summary>
        /// Sets the Minimap's Rect, used when we load for example.
        /// </summary>
        /// <param name="rect"></param>
        public void SetMiniMapRect(Rect rect)
        {
            var mm = _graphView.contentContainer.Q<MiniMap>();
            mm.SetPosition(rect);
        }

        /// <summary>
        /// Generate the top toolbar. It will contain the file's name, a toggle for autosave, a save and a load button
        /// </summary>
        private void GenerateToolBar()
        {
            //The toolbar will have a button to create new ndoes
            var toolbar = new Toolbar();
            toolbar.styleSheets.Add(styleSheet: Resources.Load<StyleSheet>(path: "Dialogue"));
            var fileNameTextField = new TextField(label: "File Name:");
            fileNameTextField.SetValueWithoutNotify(_fileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
            toolbar.Add(fileNameTextField);

            var saveButton = new Button(clickEvent: () => { _graphView.CheckIfValueWasUpdated(); RequestDataOperation(true); }) { text = "Save Data" };
                        
            var autoSave = new Toggle(label: "AutoSave");
            //autoSave.labelElement.sty
            _ = autoSave.RegisterValueChangedCallback(evt =>
            {
                _autoSave = evt.newValue;
                saveButton.SetEnabled(!_autoSave);
                RequestDataOperation(true);
            });
            toolbar.Add(autoSave);

            toolbar.Add(child: saveButton);
            toolbar.Add(child: new Button(clickEvent: () => RequestDataOperation(false)) {text ="Load Data" });



            //We add the toolbar to the window
            rootVisualElement.Add(toolbar);
        }

        /// <summary>
        /// Requests a save or a load
        /// </summary>
        /// <param name="save">if true, will save, if false, will load</param>
        public void RequestDataOperation(bool save)
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                EditorUtility.DisplayDialog(title:"Invalide file name !",message: "Please enter a valid file name.", ok:"OK");
                if (_autoSave)
                {
                    _autoSave = false;
                    Toolbar toolbar = rootVisualElement.Q<Toolbar>();
                    Button saveButton = toolbar.Q<Button>();
                    saveButton.SetEnabled(true);
                }
                return;
            }

            var saveUtility = GraphSaveUtility.GetInstance(_graphView, this);

            if (save)
            {
                saveUtility.SaveGraph(_fileName);
            }
            else
            {
                saveUtility.LoadGraph(_fileName);
            }
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
            Selection.activeObject = null;

        }

        /// <summary>
        /// Sets the name of the graph view and streches it to the whole of the editor window
        /// </summary>
        private void ConstructGraphView()
        {
            _graphView = new DialogueView(this)
            {
                name = "Dialogue Graph;"
                
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }
    }
}
