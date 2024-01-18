
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace AQM.Tools
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> onNodeSelected;
        public Action onRefreshInspector;
        public NodeSO node;
        public List<Port> inputPortsList;
        public List<Port> outputPortsList;
            
        public NodeView(NodeSO nodeSo): base(DialogSystemEditor.RelativePath+"/Views/NodeView/NodeView.uxml")
        {
            this.node = nodeSo;
            title = nodeSo.name;
            viewDataKey = nodeSo.guid;
            
            style.left = nodeSo.position.x;
            style.top = nodeSo.position.y;

            inputPortsList = new List<Port>();
            outputPortsList = new List<Port>();
        }
        
        protected void CreateDefaultInputPorts()
        {
            inputPortsList = new List<Port>();
            
            foreach (PortSO inputPortData in node.inputPorts)
            {
                var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
                inputPort.portName = inputPortData.portName;
                inputPort.viewDataKey = inputPortData.id;
        
                inputPortsList.Add(inputPort);
                inputContainer.Add(inputPort);    
            }
        }
        
        protected void CreateDefaultOutputPorts()
        {
            outputPortsList = new List<Port>();
            
            foreach (PortSO outputPortData in node.outputPorts)
            {
                var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                outputPort.portName = outputPortData.portName;
                outputPort.viewDataKey = outputPortData.id;
        
                outputPortsList.Add(outputPort);
                outputContainer.Add(outputPort);    
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            node.SetPosition(newPos);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (onNodeSelected != null)
            {
                onNodeSelected.Invoke(this);
            }
        }

        public void UpdateState()
        {
            RemoveFromClassList("initial");
            RemoveFromClassList("running");
            RemoveFromClassList("unreachable");
            RemoveFromClassList("finished");
            RemoveFromClassList("visited");
            RemoveFromClassList("visitedUnreachable");
            
            if (!Application.isPlaying) return;
            switch (node.NodeState)
            {
                case NodeSO.State.Initial:
                    AddToClassList("initial");
                    break;
                case NodeSO.State.Running:
                    AddToClassList("running");
                    break;
                case NodeSO.State.Unreachable:
                    AddToClassList("unreachable");
                    break;
                case NodeSO.State.Finished:
                    AddToClassList("finished");
                    break;
                case NodeSO.State.Visited:
                    AddToClassList("visited");
                    break;
                case NodeSO.State.VisitedUnreachable:
                    AddToClassList("visitedUnreachable");
                    break;
            }
        }
    }
}

