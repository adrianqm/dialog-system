using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BranchNodeView  : NodeView
{
    public BranchNodeView(NodeSO nodeSo) : base(nodeSo)
    {
        topContainer.style.flexDirection = FlexDirection.Row;
        inputContainer.AddToClassList("multiInputContainer");
        outputContainer.AddToClassList("multiOutputContainer");
        
        var nodeViewContainer = new BranchNodeViewContainer();
        extensionContainer.Add(nodeViewContainer);
        CreateDefaultInputPorts();
        CreateDefaultOutputPorts();
        
        SetUpRequirements(nodeSo);
        RefreshExpandedState();
    }

    private void SetUpRequirements(NodeSO nodeSo)
    {
        BranchNodeSO branchSo = nodeSo as BranchNodeSO;
        if (branchSo)
        {
            RequirementsListView requirementsListView = this.Q<RequirementsListView>();
            requirementsListView.SaveAsSubAssetOf(DSData.instance.database);
            requirementsListView.Populate(branchSo.branch.requirements);
        }
    }
}
