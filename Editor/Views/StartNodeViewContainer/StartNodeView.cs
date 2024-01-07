using System;
using AQM.Tools;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class StartNodeView  : NodeView
{
    public StartNodeView(NodeSO nodeSo) : base(nodeSo)
    {
        capabilities -= Capabilities.Deletable;
        capabilities -= Capabilities.Copiable;
        inputContainer.AddToClassList("singleInputContainer");
        outputContainer.AddToClassList("singleOutputContainer");
        if (Application.isPlaying)
        {
            if (node.NodeState == NodeSO.State.Initial)
            {
                node.NodeState = NodeSO.State.Running;
            }
        }
        else node.NodeState = NodeSO.State.Initial;
        
        var nodeViewContainer = new StartNodeViewContainer();
        extensionContainer.Add(nodeViewContainer);
        CreateDefaultOutputPorts();
        RefreshExpandedState();
    }
}
