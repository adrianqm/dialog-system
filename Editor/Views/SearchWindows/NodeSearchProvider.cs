using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace AQM.Tools
{
    public class NodeSearchProvider: ScriptableObject, ISearchWindowProvider
    {
        private DialogSystemView _graphView;
        private ConversationTree _tree;
        private Texture2D _indentationIcon;

        private enum Types
        {
            SingleChoice,
            MultipleChoice,
            Bookmark,
            Group
        }
        
        private struct  EntryType
        {
            public Types type;
            public ScriptableObject param;

            public EntryType(Types t, ScriptableObject p = null)
            {
                type = t;
                param = p;
            }
        }
        
        public void SetUp(DialogSystemView graphView)
        {
            _graphView = graphView;
            _tree = graphView.GetCurrentTree();
            
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
                    userData = new EntryType(Types.SingleChoice),
                    level = 2
                },
                new(new GUIContent("Multiple Choice", _indentationIcon))
                {
                    userData = new EntryType(Types.MultipleChoice),
                    level = 2
                }
            };
            searchlist.Add(new SearchTreeGroupEntry(new GUIContent("Go to Bookmark"), 1));
            foreach (var treeBookmark in _tree.bookmarks)
            {
                SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(treeBookmark.bookmarkTitle,_indentationIcon));
                entry.level = 2;
                entry.userData = new EntryType(Types.Bookmark,treeBookmark);
                searchlist.Add(entry);
            }
            searchlist.Add(new(new GUIContent("Group Box", _indentationIcon))
            {
                userData = new EntryType(Types.Group),
                level = 1
            });
            return searchlist;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var pos = _graphView.MouseToContent(context.screenMousePosition,true);
            EntryType entry = SearchTreeEntry.userData is EntryType ? (EntryType) SearchTreeEntry.userData : default;
            switch (entry.type)
            {
                case Types.SingleChoice:
                {
                    _graphView.CreateNode(NodeFactory.NodeType.Dialog,pos);
                    return true;
                }

                case Types.MultipleChoice:
                {
                    _graphView.CreateNode(NodeFactory.NodeType.Choice,pos);
                    return true;
                }
                
                case Types.Bookmark:
                {
                    _graphView.CreateBookmark(NodeFactory.NodeType.Bookmark,pos, entry.param as BookmarkSO);
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

