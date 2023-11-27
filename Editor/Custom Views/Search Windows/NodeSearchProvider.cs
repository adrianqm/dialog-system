using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace AQM.Tools
{
    public class NodeSearchProvider: ScriptableObject, ISearchWindowProvider
    {
        private DialogSystemView _graphView;
        private Texture2D _indentationIcon;

        private enum Types
        {
            SingleChoice,
            MultipleChoice,
            Group
        };
        
        public void SetUp(DialogSystemView graphView)
        {
            _graphView = graphView;
            
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, Color.clear);
            _indentationIcon.Apply();
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchlist = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Actors"),0),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1),
                new(new GUIContent("Single Choice", _indentationIcon))
                {
                    userData = Types.SingleChoice,
                    level = 2
                },
                new(new GUIContent("Multiple Choice", _indentationIcon))
                {
                    userData = Types.MultipleChoice,
                    level = 2
                },
                new(new GUIContent("Group Box", _indentationIcon))
                {
                    userData = Types.Group,
                    level = 1
                },
            };
            return searchlist;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var pos = _graphView.MouseToContent(context.screenMousePosition,true);
            switch (SearchTreeEntry.userData)
            {
                case Types.SingleChoice:
                {
                    _graphView.CreateNode(typeof(DialogNode),pos);
                    return true;
                }

                case Types.MultipleChoice:
                {
                    _graphView.CreateNode(typeof(ChoiceNode),pos);
                    return true;
                }

                case Types.Group:
                {
                    _graphView.CreateGroupBox(pos);
                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }
    }
}

