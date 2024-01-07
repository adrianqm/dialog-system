using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class PortSO : ScriptableObject
    {
        public string id;
        public string portName;
        public NodeSO originNode;
        public List<NodeSO> targetNodes = new List<NodeSO>();

        public void AddTargetNode(NodeSO targetNode)
        {
            Undo.RecordObject(this, "Node (Add child)");
            targetNodes.Add(targetNode);
            EditorUtility.SetDirty(this);
        }

        public void RemoveTargetNode(NodeSO targetNode)
        {
            Undo.RecordObject(this, "Node (Remove child)");
            targetNodes.Remove(targetNode);
            EditorUtility.SetDirty(this);
        }
        
        public void SaveAs(DialogSystemDatabase db)
        {
            AssetDatabase.AddObjectToAsset(this,db);
        }

        public void SortPort()
        {
            Undo.RecordObject(this, "Node (Child Sort)");
            targetNodes.Sort(SortByVerticalPosition);
            EditorUtility.SetDirty(this);
        }
        
        private int SortByVerticalPosition(NodeSO top, NodeSO bottom)
        {
            return top.position.y < bottom.position.y ? -1 : 1;
        }

        private void OnDestroy()
        {
            EditorUtility.SetDirty(this);
        }
    }
}