using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Conversation Tree", menuName = "AQM/Tools/Dialog System/Conversation Tree", order = 2)]
public class ConversationTree : ScriptableObject
{
    public Action OnEndConversation;
    
    [HideInInspector] public DialogSystemDatabase database;
    [HideInInspector] public Node rootNode;
    [HideInInspector] public List<Node> nodes = new();
    [HideInInspector] public Node runningNode;
    
    [TextArea] public string title;
    [TextArea] public string description;
    
    private Node finishedNode;

    ConversationTree(DialogSystemDatabase database)
    {
        this.database = database;
    }

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
    public Node CreateNode(System.Type type, Vector2 position)
    {
        Node node = ScriptableObject.CreateInstance(type) as Node;
        node.name = type.Name;
        node.guid = GUID.Generate().ToString();
        node.position = position;

        Undo.RecordObject(this, "Conversation Tree (CreateNode)");
        nodes.Add(node);

        if (!Application.isPlaying)
        {
            AssetDatabase.AddObjectToAsset(node,this);
        }
        Undo.RegisterCreatedObjectUndo(node, "Conversation Tree (CreateNode)");
        AssetDatabase.SaveAssets();
        return node;
    }

    public void DeteleNode(Node node)
    {
        Undo.RecordObject(this, "Conversation Tree (DeleteNode)");
        nodes.Remove(node);
        
        //AssetDatabase.RemoveObjectFromAsset(node);
        Undo.DestroyObjectImmediate(node);
        AssetDatabase.SaveAssets();
    }

    public bool AddChild(Node parent, Node child)
    {
        bool hasAdded = false;
        DialogNode node = parent as DialogNode;
        if(node && !node.children.Contains(child))
        {
            Undo.RecordObject(node, "Conversation Tree (AddChild)");
            node.children.Add(child);
            EditorUtility.SetDirty(node);
            hasAdded = true;
        }
        
        RootNode rootNode = parent as RootNode;
        if (rootNode && !rootNode.children.Contains(child))
        {
            Undo.RecordObject(rootNode, "Conversation Tree (AddChild)");
            rootNode.children.Add(child);
            EditorUtility.SetDirty(rootNode);
            hasAdded = true;
        }

        return hasAdded;
    }
    
    public void RemoveChild(Node parent, Node child)
    {
        DialogNode node = parent as DialogNode;
        if(node)
        {
            Undo.RecordObject(node, "Conversation Tree (RemoveChild)");
            node.children.Remove(child);
            EditorUtility.SetDirty(node);
        }
        
        RootNode rootNode = parent as RootNode;
        if (rootNode)
        {
            Undo.RecordObject(rootNode, "Conversation Tree (RemoveChild)");
            rootNode.children.Remove(child);
            EditorUtility.SetDirty(rootNode);
        }
    }
    public List<Node> GetChildren(Node parent)
    {
        List<Node> children = new();
        
        DialogNode node = parent as DialogNode;
        if(node)
        {
            return node.children;
        }
        
        RootNode rootNode = parent as RootNode;
        if (rootNode)
        {
            return rootNode.children;
        }

        return children;
    }

    public void Traverse(Node node, System.Action<Node> visiter)
    {
        if (!node) return;
        visiter.Invoke(node);
        var children = GetChildren(node);
        children.ForEach((n)=> Traverse(n,visiter));
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
