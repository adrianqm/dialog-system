using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Node : ScriptableObject
{
    [HideInInspector] public string guid;
    [HideInInspector] public Vector2 position;
    public GroupNode group;

    public enum State
    {
        Initial,
        Running,
        Unreachable,
        Finished,
        Visited,
        VisitedUnreachable
    }

    private State _nodeState = State.Initial;

    public State NodeState
    {
        get => _nodeState;
        set => _nodeState = value;
    }

    public bool CheckConditions()
    {
        return true;
    }

    public abstract Node Clone();

    public abstract void OnRunning();
}
