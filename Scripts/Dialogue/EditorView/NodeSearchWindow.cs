using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaltButter.Dialogue.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueView _graphView;
        private EditorWindow _window;
        private Texture2D _indentationIcon;
        public void Init(DialogueView graphView, EditorWindow window)
        {
            _graphView = graphView;
            _window = window;

            //Indentation hack as a transparent icon
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentationIcon.Apply();
        }

        /// <summary>
        /// The Window that gets call when we press Space or Right Click -> Create Node
        /// Will contain one thing -> Dialogue Node
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent(text: "Create Elements"), level: 0),
            new SearchTreeEntry(new GUIContent(text: "Dialogue Node", _indentationIcon))
            {
                userData = new DialogueNode(), level = 1
            }
        };

            return tree;
        }


        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, context.screenMousePosition - _window.position.position);
            var localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);

            switch (SearchTreeEntry.userData)
            {
                case DialogueNode dialogueNode :
                    _graphView.CreateNode("Dialogue Node", localMousePosition);
                    return true;
                default:
                    return false;

            }
        }
    }
}
