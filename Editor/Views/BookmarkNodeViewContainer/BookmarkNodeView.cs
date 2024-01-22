using System;
using AQM.Tools;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BookmarkNodeView  : NodeView
{
    public BookmarkNodeView(DialogSystemView graphview, BookmarkNodeSO nodeSo) : base(nodeSo)
    {
        inputContainer.AddToClassList("singleInputContainer100");
        outputContainer.AddToClassList("singleOutputContainer");
        
        var nodeViewContainer = new BookmarkNodeViewContainer(graphview.GetCurrentTree(), nodeSo);
        extensionContainer.Add(nodeViewContainer);
        CreateDefaultInputPorts();
        RefreshExpandedState();
    }
}
