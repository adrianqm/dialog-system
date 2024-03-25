using System;
using AQM.Tools;
using UnityEngine;

public static class NodeFactory
{
#if UNITY_EDITOR
    public enum NodeType
    {
        Start,
        Complete,
        Dialog,
        Choice,
        Branch,
        Bookmark,
        Time
    }
    public static NodeSO CreateNode(NodeType type, Vector2 position)
    {
        switch (type)
        {
            case NodeType.Start:
                StartNodeSO startNode = ScriptableObject.CreateInstance<StartNodeSO>();
                startNode.Init(position);
                return startNode;
                
            case NodeType.Complete:
                CompleteNodeSO completeNode = ScriptableObject.CreateInstance<CompleteNodeSO>();
                completeNode.Init(position);
                return completeNode;
            
            case NodeType.Dialog:
                DialogNodeSO dialogNode = ScriptableObject.CreateInstance<DialogNodeSO>();
                dialogNode.Init(position);
                return dialogNode;
            
            case NodeType.Choice:
                ChoiceNodeSO choiceNode = ScriptableObject.CreateInstance<ChoiceNodeSO>();
                choiceNode.Init(position);
                return choiceNode;
            
            case NodeType.Branch:
                BranchNodeSO branchNode = ScriptableObject.CreateInstance<BranchNodeSO>();
                branchNode.Init(position);
                return branchNode;
            
            case NodeType.Bookmark:
                BookmarkNodeSO bookmarkNode = ScriptableObject.CreateInstance<BookmarkNodeSO>();
                bookmarkNode.Init(position);
                return  bookmarkNode;
            
            case NodeType.Time:
                TimerNodeSO timerNodeSo = ScriptableObject.CreateInstance<TimerNodeSO>();
                timerNodeSo.Init(position);
                return  timerNodeSo;
            
            default:
                return null;
        }
    }
#endif
}