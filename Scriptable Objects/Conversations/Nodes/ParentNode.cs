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

    public override Node Clone()
    {
        ParentNode node = Instantiate(this);
        node.children = children.ConvertAll(c => c.Clone());
        return node;
    }
}