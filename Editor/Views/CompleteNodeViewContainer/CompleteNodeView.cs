using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CompleteNodeView  : NodeView
{
    public CompleteNodeView(NodeSO nodeSo) : base(nodeSo)
    {
        capabilities -= Capabilities.Deletable;
        capabilities -= Capabilities.Copiable;
        inputContainer.AddToClassList("singleInputContainer100");
        outputContainer.AddToClassList("singleOutputContainer");
        
        var nodeViewContainer = new CompleteNodeViewContainer();
        extensionContainer.Add(nodeViewContainer);
        CreateDefaultInputPorts();
        RefreshExpandedState();
    }
}
