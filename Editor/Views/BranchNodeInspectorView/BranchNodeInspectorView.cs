using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blackboard.Editor.Requirement;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AQM.Tools
{
    public class BranchNodeInspectorView : VisualElement
    {
        
        private readonly string assetName = "BranchNodeInspectorView";
        public BranchNodeInspectorView(BranchNodeSO node)
        {
            VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
            uxml.CloneTree(this);

            if (node != null)
            {
                RequirementsListView list = this.Q<RequirementsListView>();
                list.SaveAsSubAssetOf(DSData.instance.database);
                list.PopulateView(node.branch.requirements);
            }
        }
    }
}
