using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
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
        
        public Node startNode;
        public Node completeNode;
        public List<Node> nodes = new ();
        public List<GroupNode> groups = new ();
        [HideInInspector] public Node runningNode;
        
        [TextArea] public string title;
        [TextArea] public string description;
        [HideInInspector] [TextArea] public string guid;
        public State conversationState = State.Editor;
        
        private Node _finishedNode;
        private DialogSystemDatabase _currentDatabase;

        public void SetName(string newTitle)
        {
            title = newTitle;
            onNameChanged?.Invoke();
        }
        
        public DSNode StartConversation(DialogSystemDatabase currentDatabase)
        {
            if (_finishedNode) ResetNodeStates();
            
            conversationState = State.Running;
            runningNode = startNode;
            _currentDatabase = currentDatabase;
            StartNode node = runningNode as StartNode;
            if (node)
            {
                return CheckNextChildMove(node.children);
            }

            return null;
        }

        private void ResetNodeStates()
        {
            conversationState = State.Running;
            // Reset states to initial
            foreach (var n in nodes)
            {
                n.NodeState = Node.State.Initial;
            }
        }

        private void EndConversation()
        {
            conversationState = State.Completed;
            runningNode.NodeState = Node.State.Finished;
            _finishedNode = runningNode;
            runningNode = null;
            onEndConversation.Invoke();
        }
        
        public DSNode GetNextNode(int option = -1)
        {
            if (!runningNode) return null;

            DSNode nextNode = null;
            DialogNode dialogNode = runningNode as DialogNode;
            if (dialogNode)
            {
                nextNode = CheckNextChildMove(dialogNode.children);
            }
            
            ChoiceNode choiceNode = runningNode as ChoiceNode;
            if (choiceNode && option != -1)
            {
                nextNode = CheckNextChildMove(choiceNode.choices[option].children);
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

        private DSNode CheckNextChildMove(List<Node> children)
        {
            Node childToMove = null;
            foreach (var child in children.Where(child => child.CheckConditions()))
            {
                runningNode.NodeState = Node.State.Visited;
                childToMove = child;
                SetRunningNode(child);
                break;
            }

            if (!childToMove)
            {
                // Conversation finished
                EndConversation();
                onUpdateViewStates?.Invoke();
                return null;
            }
            onUpdateViewStates?.Invoke();
#if LOCALIZATION_EXIST
            return GetTranslatedNode(childToMove);
#else
            return childToMove.GetData();
#endif
        }
        
    #if LOCALIZATION_EXIST

        private DSNode GetTranslatedNode(Node childToMove)
        {
            if (!_currentDatabase.localizationActivated) return childToMove.GetData();
            DialogNode dialogNode = childToMove as DialogNode;
            if (dialogNode)
            {
                DSDialog dsDialog = null;
                var translatedValue =
                    LocalizationSettings.StringDatabase.GetLocalizedString(_currentDatabase.tableCollectionName, dialogNode.guid);
                if(translatedValue != null)
                {
                    dsDialog = new DSDialog(dialogNode.actor, translatedValue);
                }
                return dsDialog;
            }
            ChoiceNode choiceNode = childToMove as ChoiceNode;
            if (choiceNode)
            {
                string message = "";
                List<string> choices = new List<string>();
                var translatedValue =
                    LocalizationSettings.StringDatabase.GetLocalizedString(_currentDatabase.tableCollectionName, choiceNode.guid);
                if(translatedValue != null)
                {
                    message = translatedValue;
                }
                
                choiceNode.choices.ForEach(c =>
                {
                    var choiceEntry = LocalizationSettings.StringDatabase.GetLocalizedString(_currentDatabase.tableCollectionName, c.guid);
                    if(choiceEntry != null)
                    {
                        choices.Add(choiceEntry);
                    }
                });
                return new DSChoice(choiceNode.actor,message,choices);
            }
            return null;
        }
    #endif

        private void SetRunningNode(Node node)
        {
            // Update unreachable status
            List<Node> visitedNodes = CustomDFS.StartDFS(node);
            foreach (var n in nodes)
            {
                if (node.guid == n.guid)
                {
                    if (n is CompleteNode)
                    {
                        runningNode = n;
                        EndConversation();
                    }
                    else
                    {
                        n.NodeState = Node.State.Running;
                        runningNode = n;
                    }
                    continue;
                }
                
                if (!visitedNodes.Find(vn => vn.guid == n.guid))
                {
                    if (n.NodeState == Node.State.Visited) n.NodeState = Node.State.VisitedUnreachable;
                    else if(n.NodeState != Node.State.VisitedUnreachable) n.NodeState = Node.State.Unreachable;
                }
            }
        }

        public List<Node> GetChildren(ParentNode parent)
        {
            return parent.children;
        }

        private void Traverse(Node node, System.Action<Node> visiter)
        {
            if (!node) return;
            visiter.Invoke(node);
            ParentNode parentNode = node as ParentNode;
            if (parentNode)
            {
                var children = GetChildren(parentNode);
                children.ForEach((n)=> Traverse(n,visiter));
            }
            
            ChoiceNode choiceNode = node as ChoiceNode;
            if (choiceNode)
            {
                choiceNode.choices.ForEach((choice) =>
                {
                    choice.children.ForEach((n)=> Traverse(n,visiter));
                });
            }
        }
        
        public ConversationTree Clone()
        {
            ConversationTree tree = Instantiate(this);
            
            NodeCloningManager cloningManager = new NodeCloningManager();
            tree.startNode= cloningManager.CloneNode(tree.startNode);
            tree.conversationState = State.Idle;
            tree.groups = groups.ConvertAll(g => g.Clone());
            tree.nodes = new List<Node>();
            List<string> nonRepeatNodeList = new();
            Traverse(tree.startNode, (n) =>
            {
                if (nonRepeatNodeList.Contains(n.guid)) return;
                tree.nodes.Add(n);
                nonRepeatNodeList.Add(n.guid);
            });

            if (!nonRepeatNodeList.Contains(tree.completeNode.guid))
            {
                tree.nodes.Add(tree.completeNode.Clone());
            }
            return tree;
        }
    }
}
