using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParentNode : Node
{
    [HideInInspector] public List<Node> children = new();

    public override void OnRunning()
    {
        NodeState = State.Running;
    }
}