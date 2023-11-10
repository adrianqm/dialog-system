using System;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools.Serializable;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
#if LOCALIZATION_EXIST
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
#endif

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

    public void SetName(string newTitle)
    {
        title = newTitle;
        onNameChanged?.Invoke();
        EditorUtility.SetDirty(this);
    }
    
    public Node StartConversation()
    {
        if (_finishedNode) ResetNodeStates();
        
        conversationState = State.Running;
        runningNode = startNode;
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
    
    public void EndConversation()
    {
        conversationState = State.Completed;
        runningNode.NodeState = Node.State.Finished;
        _finishedNode = runningNode;
        runningNode = null;
        onEndConversation.Invoke();
    }
    
    public Node GetNextNode(int option = -1)
    {
        if (!runningNode) return null;

        Node nextNode = null;
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
    
    public Node GetCurrentNode()
    {
        Node currentNode = runningNode;
        #if LOCALIZATION_EXIST
            currentNode = GetTranslatedNode(currentNode);
        #endif
        return currentNode;
    }

    private Node CheckNextChildMove(List<Node> children)
    {
        Node childToMove = null;
        foreach (var child in children.Where(child => child.CheckConditions()))
        {
            runningNode.NodeState = Node.State.Visited;
            childToMove = child;
            SetRunningNode(child);
            break;
        }

        if (childToMove == null)
        {
            // Conversation finished
            EndConversation();
        }
        
        #if LOCALIZATION_EXIST
            childToMove = GetTranslatedNode(childToMove);
        #endif
        
        onUpdateViewStates?.Invoke();
        return childToMove;
    }
    
#if LOCALIZATION_EXIST

    private Node GetTranslatedNode(Node childToMove)
    {
        if (DSData.instance.database.tableCollection && DSData.instance.database.localizationActivated)
        {
            StringTable table = LocalizationSettings.StringDatabase.GetTable(
                DSData.instance.database.tableCollection.name,
                LocalizationSettings.SelectedLocale);
            DialogNode dialogNode = childToMove as DialogNode;
            if (dialogNode)
            {
                var entry = table.GetEntry(dialogNode.guid);
                if(entry != null)
                {
                    dialogNode.message = entry.Value;
                }
                return dialogNode;
            }
            ChoiceNode choiceNode = childToMove as ChoiceNode;
            if (choiceNode)
            {
                var entry = table.GetEntry(choiceNode.guid);
                if(entry != null)
                {
                    choiceNode.message = entry.Value;
                }
            
                choiceNode.choices.ForEach(c =>
                {
                    var choiceEntry = table.GetEntry(c.guid);
                    if(choiceEntry != null)
                    {
                        c.choiceMessage = choiceEntry.Value;
                    }
                });
                return choiceNode;
            }
        }
        return childToMove;
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

#if  UNITY_EDITOR

    public ConversationTree Create(DialogSystemDatabase db, string title, string description)
    {
        this.title = title;
        this.description = description;
        guid = GUID.Generate().ToString();
        name = title;
        hideFlags = HideFlags.HideInHierarchy;
        
        AssetDatabase.AddObjectToAsset(this,db);
        return this;
    }
    
    public GroupNode CreateGroup(DialogSystemDatabase db, string groupTitle, Vector2 position)
    {
        GroupNode groupNode = ScriptableObject.CreateInstance(typeof(GroupNode)) as GroupNode;
        groupNode.name = groupTitle;
        groupNode.title = groupTitle;
        groupNode.guid = GUID.Generate().ToString();
        groupNode.position = position;
        groupNode.hideFlags = HideFlags.HideInHierarchy;
        
        Undo.RecordObject(this, "Conversation Tree (CreateGroup)");
        groups ??= new List<GroupNode>();
        groups.Add(groupNode);
        
        if (!Application.isPlaying)
        {
            AssetDatabase.AddObjectToAsset(groupNode,db);
        }
        Undo.RegisterCreatedObjectUndo(groupNode, "Conversation Tree (CreateGroup)");
        AssetDatabase.SaveAssets();
        return groupNode;
    }
    
    public void DeteleGroupNode(GroupNode groupNode)
    {
        Undo.RecordObject(this, "Conversation Tree (DeleteGroupNode)");
        groups.Remove(groupNode);
        
        //AssetDatabase.RemoveObjectFromAsset(node);
        Undo.DestroyObjectImmediate(groupNode);
        AssetDatabase.SaveAssets();
    }
    
    public bool AddGroupToNode(Node node,GroupNode group)
    {
        bool hasBeenAdded = false;
        if(node  && group)
        {
            Undo.RecordObject(node, "Conversation Tree (AddGroupToNode)");
            node.group = group;
            EditorUtility.SetDirty(node);
            hasBeenAdded = true;
        }

        return hasBeenAdded;
    }
    
    public void RemoveGroupFromNode(Node node)
    {
        if(node)
        {
            Undo.RecordObject(node, "Conversation Tree (RemoveGroupFromNode)");
            node.group = null;
            EditorUtility.SetDirty(node);
        }
    }
    
    public void SetGroupTitle(GroupNode group, string newTitle)
    {
        if (!group) return;
        Undo.RecordObject(group, "Conversation Tree (SetGroupTitle)");
        group.title = newTitle;
        EditorUtility.SetDirty(group);
    }
    
    public Node CreateNode(DialogSystemDatabase db, System.Type type, Vector2 position)
    {
        Node node = ScriptableObject.CreateInstance(type) as Node;
        node.name = type.Name;
        node.guid = GUID.Generate().ToString();
        node.position = position;

        AddNodeToList(db,node);
#if LOCALIZATION_EXIST
        if(node is not StartNode && node is not CompleteNode) LocalizationUtils.AddDefaultKeyToCollection(node.guid,"");
#endif
        
        return node;
    }
    
    public DialogNode CreateDialogNodeCopy(DialogSystemDatabase db,  System.Type type, Vector2 position, SerializableDialogNode nodeToCopy)
    {
        DialogNode node = ScriptableObject.CreateInstance(type) as DialogNode;
        node.name = type.Name;
        node.guid = GUID.Generate().ToString();
        node.position = position;
        node.actor = db.actors.Find(actor => actor.guid == nodeToCopy.actorGuid);
        node.message = nodeToCopy.message;

        AddNodeToList(db,node);
#if LOCALIZATION_EXIST
        LocalizationUtils.AddCopyKeyToCollection(node.guid,nodeToCopy.guid);
#endif
        
        return node;
    }
    
    public ChoiceNode CreateChoiceNodeToCopy(DialogSystemDatabase db, Vector2 position, SerializableChoiceNode nodeToCopy, Dictionary<string, Choice> choiceMap)
    {
        Type type = typeof(ChoiceNode);
        ChoiceNode node = ScriptableObject.CreateInstance(type) as ChoiceNode;
        node.name = type.Name;
        node.guid = GUID.Generate().ToString();
        node.position = position;
        node.actor = db.actors.Find(actor => actor.guid == nodeToCopy.actorGuid);
        node.message = nodeToCopy.message;
        node.choices = new List<Choice>();
        foreach (var serializableChoice in nodeToCopy.choices)
        {
            Choice choice = node.CreateChoice(db, serializableChoice.choiceMessage);
            choiceMap.Add(serializableChoice.guid, choice);
        }

        AddNodeToList(db,node);
#if LOCALIZATION_EXIST
        LocalizationUtils.AddCopyKeyToCollection(node.guid,nodeToCopy.guid);
#endif
        
        return node;
    }

    private void AddNodeToList(DialogSystemDatabase db, Node node)
    {
        node.hideFlags = HideFlags.HideInHierarchy;
        Undo.RecordObject(this, "Conversation Tree (CreateNode)");
        nodes ??= new List<Node>();
        nodes.Add(node);
        
        if (!Application.isPlaying)
        {
            AssetDatabase.AddObjectToAsset(node,db);
        }
        Undo.RegisterCreatedObjectUndo(node, "Conversation Tree (CreateNode)");
        AssetDatabase.SaveAssets();
    }

    public void DeleteNode(Node node)
    {
        Undo.RecordObject(this, "Conversation Tree (DeleteNode)");
        nodes.Remove(node);
        
        //AssetDatabase.RemoveObjectFromAsset(node);
        Undo.DestroyObjectImmediate(node);
        AssetDatabase.SaveAssets();
    }

    public bool AddChild(Node parent, Node child, Port parentPort = null)
    {
        bool hasAdded = false;
        ParentNode parentNode = parent as ParentNode;
        if(parentNode && !parentNode.children.Contains(child) )
        {
            Undo.RecordObject(parentNode, "Conversation Tree (AddChild)");
            parentNode.children.Add(child);
            EditorUtility.SetDirty(parentNode);
            hasAdded = true;
        }
        
        ChoiceNode choiceNode = parent as ChoiceNode;
        if(choiceNode && parentPort != null)
        {
            NodeView  choiceView = parentPort.node as NodeView;
            Choice choice = choiceView?.FindPortChoice(parentPort);
            if (choice)
            {
                Undo.RecordObject(choice, "Conversation Tree (AddChoice)");
                choice.children.Add(child);
                EditorUtility.SetDirty(choice);
                hasAdded = true;
            }
        }

        return hasAdded;
    }
    
    public void RemoveChild(Node parent, Node child, Port parentPort = null)
    {
        ParentNode parentNode = parent as ParentNode;
        if(parentNode)
        {
            Undo.RecordObject(parentNode, "Conversation Tree (RemoveChild)");
            parentNode.children.Remove(child);
            EditorUtility.SetDirty(parentNode);
        }
        
        ChoiceNode choiceNode = parent as ChoiceNode;
        if(choiceNode && parentPort != null)
        {
            NodeView  choiceView = parentPort.node as NodeView;
            Choice choice = choiceView?.FindPortChoice(parentPort);
            if (choice)
            {
                Undo.RecordObject(choice, "Conversation Tree (RemoveChild)");
                choice.children.Remove(child);
                EditorUtility.SetDirty(choice);
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
#endif
}
