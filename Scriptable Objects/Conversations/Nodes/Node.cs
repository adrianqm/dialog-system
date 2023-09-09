using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Node : ScriptableObject
{
    [HideInInspector] public string guid;
    [HideInInspector] public Vector2 position;
    [HideInInspector] public List<Node> children = new();
    public GroupNode group;

    public enum State
    {
        Initial,
        Running,
        Unreachable,
        Finished
    }

    private State nodeState = State.Initial;

    public State NodeState
    {
        get => nodeState;
        set => nodeState = value;
    }

    public bool CheckConditions()
    {
        return true;
    }

    public Node Clone()
    {
        Node node = Instantiate(this);
        node.children = children.ConvertAll(c => c.Clone());
        return node;
    }

    public abstract void OnRunning();
}
