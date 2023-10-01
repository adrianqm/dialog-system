using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools.Serializable;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Conversation Tree", menuName = "AQM/Tools/Dialog System/Conversation Tree", order = 2)]
public class ConversationTree : ScriptableObject
{
    public Action OnEndConversation;
    
    
    [HideInInspector] public Node rootNode;
    public List<Node> nodes = new ();
    public List<GroupNode> groups = new ();
    [HideInInspector] public Node runningNode;
    
    [TextArea] public string title;
    [TextArea] public string description;
    [HideInInspector] [TextArea] public string guid;
    
    private Node finishedNode;

    public void StartConversation()
    {
        if (finishedNode) ResetNodeStates();
        
        runningNode = rootNode;
        RootNode node = runningNode as RootNode;
        if (node) CheckNextChildMove(node.children);
    }

    private void ResetNodeStates()
    {
        // Reset states to initial
        foreach (var n in nodes)
        {
            n.NodeState = Node.State.Initial;
        }
    }
    
    public void EndConversation()
    {
        runningNode.NodeState = Node.State.Finished;
        finishedNode = runningNode;
        runningNode = null;
        Debug.Log("Conversation finished");
        OnEndConversation.Invoke();
    }
    
    public void NextMessage()
    {
        if (!runningNode) return;
        
        DialogNode node = runningNode as DialogNode;
        if (node)
        {
            CheckNextChildMove(node.children);
        }
    }

    private Node CheckNextChildMove(List<Node> children)
    {
        Node childToMove = null;
        foreach (var child in children.Where(child => child.CheckConditions()))
        {
            runningNode.NodeState = Node.State.Initial;
            childToMove = child;
            SetRunningNode(child);
            break;
        }

        if (childToMove == null)
        {
            // Conversation finished
            EndConversation();
        }
        return childToMove;
    }

    private void SetRunningNode(Node node)
    {
        node.OnRunning();
        runningNode = node;
        
        // Update unreachable status
        List<Node> visitedNodes = CustomDFS.StartDFS(node);
        foreach (var n in nodes)
        {
            if (!visitedNodes.Contains(n))
            {
                n.NodeState = Node.State.Unreachable;
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

    public void DeteleNode(Node node)
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
        if (parentNode != null)
        {
            var children = GetChildren(parentNode);
            children.ForEach((n)=> Traverse(n,visiter));
        }
    }
    
    public ConversationTree Clone()
    {
        ConversationTree tree = Instantiate(this);
        tree.rootNode = tree.rootNode.Clone();
        tree.nodes = new List<Node>();
        List<string> nonRepeatNodeList = new();
        Traverse(tree.rootNode, (n) =>
        {
            if (nonRepeatNodeList.Contains(n.guid)) return;
            tree.nodes.Add(n);
            nonRepeatNodeList.Add(n.guid);
        });
        return tree;
    }
#endif
}
