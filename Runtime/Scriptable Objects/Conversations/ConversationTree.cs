using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
#if LOCALIZATION_EXIST
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
#endif

namespace AQM.Tools
{
    public class ConversationTree : ScriptableObject
    {
        public Action onUpdateViewStates;
        public Action onNameChanged;

        public enum State
        {
            Editor,
            Idle,
            Running, 
            Completed
        }
        
        public NodeSO startNode;
        public BookmarkSO startBookmark;
        public Actor defaultActor;
        public NodeSO completeNode;
        public List<NodeSO> nodes = new ();
        public List<GroupNode> groups = new ();
        public List<BookmarkSO> bookmarks = new();
        
        [TextArea] public string title;
        [TextArea] public string description;
        [HideInInspector] [TextArea] public string guid;
        public State conversationState = State.Editor;
        
        private NodeSO _finishedNode;
        private DialogSystemDatabase _currentDatabase;
        private NodeSO _nodeToDestroy;

        public void SetName(string newTitle)
        {
            title = newTitle;
            onNameChanged?.Invoke();
        }

        public void UpdateStatesEvent()
        {
            onUpdateViewStates?.Invoke();
        }
        
#if UNITY_EDITOR
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    conversationState = State.Editor;
                    onUpdateViewStates?.Invoke();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    conversationState = State.Idle;
                    onUpdateViewStates?.Invoke();
                    break;
            }
        }
        
        public NodeSO CreateNode(DialogSystemDatabase db, NodeFactory.NodeType type, Vector2 position, bool registerCreated = true)
        {
            NodeSO node = NodeFactory.CreateNode(type, position);
            
            // Set default actor if null
            if (defaultActor != null && node is ConversationNodeSO conversationNode)
            {
                conversationNode.actor = defaultActor;
                EditorUtility.SetDirty(conversationNode);
            }
            
            if (registerCreated)
            {
                Undo.RecordObject(this, "Conversation Tree (CreateNode)");
            }

            nodes ??= new List<NodeSO>();
            nodes.Add(node);
            
            node.SaveAs(db);

            if (registerCreated)
            {
                Undo.RegisterCreatedObjectUndo(node, "Conversation Tree (CreateNode)");
            }
            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(NodeSO node)
        {
            Undo.RecordObject(this, "Conversation Tree (DeleteNode)");
            nodes.Remove(node);
            _nodeToDestroy = node;
            EditorApplication.delayCall += () =>
            {
                Undo.DestroyObjectImmediate(node);
                AssetDatabase.SaveAssets();
            };
        }
        
        public BookmarkSO CreateBookmark(DialogSystemDatabase db, string bookmarkTitle = "default", 
            NodeSO goToNode = null, Color color = new (),bool registerCreated = true,bool addToList = true)
        {
            BookmarkSO bookmark = BookmarkFactory.CreateBookmark(bookmarkTitle, goToNode,color);

            if (registerCreated)
            {
                Undo.RecordObject(this, "Conversation Tree (CreateBookmark)");
            }

            if (addToList)
            {
                bookmarks ??= new List<BookmarkSO>();
                bookmarks.Add(bookmark);
            }
            
            bookmark.SaveAs(db);

            if (registerCreated)
            {
                Undo.RegisterCreatedObjectUndo(bookmark, "Conversation Tree (CreateBookmark)");
            }
            AssetDatabase.SaveAssets();
            return bookmark;
        }
        
        public void DeleteBookmark(BookmarkSO bookmark)
        {
            Undo.RecordObject(this, "Conversation Tree (DeleteBookmark)");
            bookmarks.Remove(bookmark);
            Undo.DestroyObjectImmediate(bookmark);
            AssetDatabase.SaveAssets();
        }
        
        private void OnDestroy()
        {
            foreach (var node in nodes.ToList())
            {
                Undo.DestroyObjectImmediate(node);
            }
            
            Undo.DestroyObjectImmediate(startBookmark);
            foreach (var bookmark in bookmarks.ToList())
            {
                Undo.DestroyObjectImmediate(bookmark);
            }
        }
#endif
    }
}

