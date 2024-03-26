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
            Branch,
            Time,
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
                new SearchTreeGroupEntry(new GUIContent("Nodes"),0),
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 1),
                new(new GUIContent("Dialog Node", _indentationIcon))
                {
                    userData = new EntryType(Types.SingleChoice),
                    level = 2
                },
                new(new GUIContent("Choice Node", _indentationIcon))
                {
                    userData = new EntryType(Types.MultipleChoice),
                    level = 2
                },
                new(new GUIContent("Branch Node", _indentationIcon))
                {
                    userData = new EntryType(Types.Branch),
                    level = 2
                },
                new(new GUIContent("Time Node", _indentationIcon))
                {
                    userData = new EntryType(Types.Time),
                    level = 2
                }
            };
            searchlist.Add(new SearchTreeGroupEntry(new GUIContent("Go to Bookmark"), 1));
            if (_tree.startBookmark)
            {
                SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(_tree.startBookmark.bookmarkTitle,_indentationIcon));
                entry.level = 2;
                entry.userData = new EntryType(Types.Bookmark,_tree.startBookmark);
                searchlist.Add(entry);
            }
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

                case Types.Branch:
                {
                    _graphView.CreateNode(NodeFactory.NodeType.Branch,pos);
                    return true;
                }
                
                case Types.Time:
                {
                    _graphView.CreateNode(NodeFactory.NodeType.Time,pos);
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

