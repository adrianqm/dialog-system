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
        public Action onEndConversation;
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
        [FormerlySerializedAs("runningNodeSo")] [HideInInspector] public NodeSO runningNode;
        
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
        
        public DSNode StartConversation(DialogSystemDatabase currentDatabase)
        {
            if (_finishedNode) ResetNodeStates();

            conversationState = State.Running;
            startNode.OnRunning();
            
            runningNode = startNode;
            _currentDatabase = currentDatabase;
            StartNodeSO nodeSo = runningNode as StartNodeSO;
            if (nodeSo)
            {
                return CheckNextChildMove(nodeSo.outputPorts[0].targetNodes);
            }

            return null;
        }

        private void EndConversation()
        {
            conversationState = State.Completed;
            runningNode.NodeState = NodeSO.State.Finished;
            _finishedNode = runningNode;
            runningNode = null;
            onEndConversation?.Invoke();
        }
        
        public DSNode GetNextNode(int option = -2)
        {
            if (!runningNode) return null;

            DSNode nextNode = null;
            DialogNodeSO dialogNodeSo = runningNode as DialogNodeSO;
            if (dialogNodeSo)
            {
                dialogNodeSo.OnCompleteNode();
                nextNode = CheckNextChildMove(dialogNodeSo.outputPorts[0].targetNodes);
            }
            
            ChoiceNodeSO choiceNodeSo = runningNode as ChoiceNodeSO;
            if (choiceNodeSo && option >= 0)
            {
                choiceNodeSo.OnCompleteNode();
                choiceNodeSo.choices[option].OnSelected();
                nextNode = CheckNextChildMove(choiceNodeSo.choices[option].port.targetNodes);
            }else if (choiceNodeSo && option == -1)
            {
                choiceNodeSo.OnCompleteNode();
                choiceNodeSo.OnDefaultSelected();
                nextNode = CheckNextChildMove(choiceNodeSo.defaultPort.targetNodes);
            }

            return nextNode;
        }
        
        public DSNode GetCurrentNode()
        {
#if LOCALIZATION_EXIST
                return GetTranslatedNode(runningNode);
#else
                return runningNode.GetData();
#endif
        }

        private DSNode CheckNextChildMove(List<NodeSO> children)
        {
            NodeSO childToMove = null;
            if (children.Count > 0 && children[0].CheckConditions())
            {
                runningNode.NodeState = NodeSO.State.Visited;
                childToMove = SetRunningNode(children[0]);
            }

            if (!childToMove)
            {
                // Conversation finished
                EndConversation();
                UpdateStatesEvent();
                return null;
            }
            UpdateStatesEvent();
#if LOCALIZATION_EXIST
            return GetTranslatedNode(childToMove);
#else
            return childToMove.GetData();
#endif
        }

        private void UpdateStatesEvent()
        {
#if UNITY_EDITOR
            onUpdateViewStates?.Invoke();
#endif
        }
        
#if LOCALIZATION_EXIST

        private DSNode GetTranslatedNode(NodeSO childToMove)
        {
            if (!_currentDatabase.localizationActivated) return childToMove.GetData();
            DialogNodeSO dialogNodeSo = childToMove as DialogNodeSO;
            if (dialogNodeSo)
            {
                DSDialog dsDialog = null;
                var translatedValue =
                    LocalizationSettings.StringDatabase.GetLocalizedString(_currentDatabase.tableCollectionName, dialogNodeSo.guid);
                if(translatedValue != null)
                {
                    dsDialog = new DSDialog(dialogNodeSo.actor, translatedValue);
                }
                return dsDialog;
            }
            ChoiceNodeSO choiceNodeSo = childToMove as ChoiceNodeSO;
            if (choiceNodeSo)
            {
                string message = "";
                List<string> choices = new List<string>();
                var translatedValue =
                    LocalizationSettings.StringDatabase.GetLocalizedString(_currentDatabase.tableCollectionName, choiceNodeSo.guid);
                if(translatedValue != null)
                {
                    message = translatedValue;
                }
                
                choiceNodeSo.choices.ForEach(c =>
                {
                    var choiceEntry = LocalizationSettings.StringDatabase.GetLocalizedString(_currentDatabase.tableCollectionName, c.guid);
                    if(choiceEntry != null)
                    {
                        choices.Add(choiceEntry);
                    }
                });
                return new DSChoice(choiceNodeSo.actor,message,choices);
            }
            return null;
        }
    #endif

        private NodeSO SetRunningNode(NodeSO nodeSo)
        {
            NodeSO nextChild = nodeSo;
            if (nodeSo is BranchNodeSO branchNodeSo)
            {
                nodeSo.NodeState = NodeSO.State.VisitedUnreachable;
                if(branchNodeSo.branch.CheckConditions())
                {
                    nextChild = branchNodeSo.TrueOutputPort.targetNodes[0];
                    nextChild = SetRunningNode(nextChild);
                }else
                {
                    nextChild = branchNodeSo.FalseOutputPort.targetNodes[0];
                    nextChild = SetRunningNode(nextChild);
                }
            }
            
            if (nodeSo is BookmarkNodeSO bookmarkNodeSo)
            {
                nodeSo.NodeState = NodeSO.State.VisitedUnreachable;
                if(bookmarkNodeSo.bookmark.goToNode != null)
                {
                    nextChild = bookmarkNodeSo.bookmark.goToNode;
                    nextChild = SetRunningNode(nextChild);
                }
            }
            
            if (nodeSo is StartNodeSO startNodeSo)
            {
                nodeSo.NodeState = NodeSO.State.VisitedUnreachable;
                if(startNodeSo.outputPorts[0].targetNodes.Count > 0)
                {
                    nextChild = startNodeSo.outputPorts[0].targetNodes[0];
                    nextChild = SetRunningNode(nextChild);
                }
            }
#if UNITY_EDITOR
            List<NodeSO> visitedNodes = CustomDFS.StartDFS(nextChild);
#endif
            foreach (var n in nodes)
            {
                if (nextChild.guid == n.guid)
                {
                    if (n is CompleteNodeSO || n is BookmarkNodeSO ||  n is BranchNodeSO || n is StartNodeSO)
                    {
                        runningNode = n;
                        EndConversation();
                    }else
                    {
                        n.OnRunning();
                        runningNode = n;
                    }
                    continue;
                }
#if UNITY_EDITOR
                if (visitedNodes.Find(vn => vn.guid == n.guid))
                {
                    n.NodeState = NodeSO.State.Initial;
                    continue;
                }
                if (n.NodeState == NodeSO.State.Visited) n.NodeState = NodeSO.State.VisitedUnreachable;
                else if(n.NodeState != NodeSO.State.VisitedUnreachable) n.NodeState = NodeSO.State.Unreachable;
#endif
            }

            return nextChild;
        }

        private void Traverse(NodeSO nodeSo, List<string> nonRepeatNodeList ,System.Action<NodeSO> visiter)
        {
            /*if (!nodeSo || nonRepeatNodeList.Contains(nodeSo.guid)) return;
            visiter.Invoke(nodeSo);
            ParentNode parentNode = nodeSo as ParentNode;
            if (parentNode)
            {
                var children = GetChildren(parentNode);
                children.ForEach((n)=> Traverse(n,nonRepeatNodeList,visiter));
            }
            
            ChoiceNode choiceNode = nodeSo as ChoiceNode;
            if (choiceNode)
            {
                choiceNode.choices.ForEach((choice) =>
                {
                    choice.children.ForEach((n)=> Traverse(n,nonRepeatNodeList,visiter));
                });
                
                choiceNode.defaultChildren.ForEach((n)=> Traverse(n,nonRepeatNodeList,visiter));
            }*/
        }
        
        public ConversationTree Clone()
        {
            ConversationTree tree = Instantiate(this);
            
            NodeCloningManager cloningManager = new NodeCloningManager();
            tree.startNode= cloningManager.CloneNode(tree.startNode);
            tree.groups = groups.ConvertAll(g => g.Clone());
            tree.nodes = new List<NodeSO>();
            List<string> nonRepeatNodeList = new();
            Traverse(tree.startNode,nonRepeatNodeList, (n) =>
            {
                //if (nonRepeatNodeList.Contains(n.guid)) return;
                tree.nodes.Add(n);
                nonRepeatNodeList.Add(n.guid);
            });

            if (tree.completeNode && !nonRepeatNodeList.Contains(tree.completeNode.guid))
            {
                tree.nodes.Add(tree.completeNode.Clone());
            }
            return tree;
        }
        private void ResetNodeStates()
        {
            conversationState = State.Running;
            // Reset states to initial
            foreach (var n in nodes)
            {
                n.NodeState = NodeSO.State.Initial;
            }
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

