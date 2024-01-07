using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class NodeCloningManager
    {
        private Dictionary<NodeSO, NodeSO> clonedNodes = new Dictionary<NodeSO, NodeSO>();
    
        public NodeSO CloneNode(NodeSO originalNode)
        {
            if (originalNode == null)
            {
                return null;
            }
    
            if (clonedNodes.TryGetValue(originalNode, out var node))
            {
                return node;
            }
    
            NodeSO clonedNode = originalNode.Clone();
            clonedNodes.Add(originalNode, clonedNode);
            
            StartNodeSO startNode = originalNode as StartNodeSO;
            if (startNode && startNode.outputPorts[0].targetNodes != null)
            {
                StartNodeSO parentClonedNode = clonedNode as StartNodeSO;
                parentClonedNode.outputPorts[0].targetNodes = new List<NodeSO>();
                foreach (NodeSO child in startNode.outputPorts[0].targetNodes)
                {
                    NodeSO clonedChild = CloneNode(child);
                    parentClonedNode.outputPorts[0].targetNodes.Add(clonedChild);
                }
            }
            
            DialogNodeSO dialogNodeSo = originalNode as DialogNodeSO;
            if (startNode && startNode.outputPorts[0].targetNodes != null)
            {
                DialogNodeSO parentClonedNode = clonedNode as DialogNodeSO;
                parentClonedNode.outputPorts[0].targetNodes = new List<NodeSO>();
                foreach (NodeSO child in dialogNodeSo.outputPorts[0].targetNodes )
                {
                    NodeSO clonedChild = CloneNode(child);
                    parentClonedNode.outputPorts[0].targetNodes.Add(clonedChild);
                }
            }
            
            ChoiceNodeSO choiceNodeSo = originalNode as ChoiceNodeSO;
            if (choiceNodeSo && choiceNodeSo.choices != null)
            {
                ChoiceNodeSO clonedChoiceNode = clonedNode as ChoiceNodeSO;
                clonedChoiceNode.choices = new List<Choice>();
                foreach (Choice choice in choiceNodeSo.choices)
                {
                    Choice clonedChoice = choice.Clone();
                    clonedChoice.port.targetNodes = new List<NodeSO>();
                    
                    foreach (NodeSO child in choice.port.targetNodes)
                    {
                        NodeSO choiceClonedChild = CloneNode(child);
                        clonedChoice.port.targetNodes.Add(choiceClonedChild);
                    }
                    clonedChoiceNode.choices.Add(clonedChoice);
                }
                clonedChoiceNode.defaultPort.targetNodes = new List<NodeSO>();
                foreach (NodeSO child in choiceNodeSo.defaultPort.targetNodes)
                {
                    NodeSO choiceClonedChild = CloneNode(child);
                    clonedChoiceNode.defaultPort.targetNodes.Add(choiceClonedChild);
                }
            }
            return clonedNode;
        }
    }
}
