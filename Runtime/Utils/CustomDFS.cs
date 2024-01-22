using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public static class CustomDFS
    {
        private static List<NodeSO> visitedNodes;
    
        public static List<NodeSO> StartDFS (NodeSO currentNode)
        {
            visitedNodes = new List<NodeSO>();
            DFSUtil(currentNode, visitedNodes);
            return visitedNodes;
        }

        private static void DFSUtil(NodeSO currentNode, List<NodeSO> visitedNodes)
        {
            visitedNodes.Add(currentNode);
            StartNodeSO parentNode = currentNode as StartNodeSO;
            if (parentNode)
            {
                foreach (var n in parentNode.outputPorts[0].targetNodes)
                {
                    if (!visitedNodes.Contains(n))
                    {
                        DFSUtil(n, visitedNodes);
                    }
                }
            }
            
            DialogNodeSO dialogNodeSo = currentNode as DialogNodeSO;
            if (dialogNodeSo)
            {
                foreach (var n in dialogNodeSo.outputPorts[0].targetNodes)
                {
                    if (!visitedNodes.Contains(n))
                    {
                        DFSUtil(n, visitedNodes);
                    }
                }
            }
        
            ChoiceNodeSO choiceNodeSo = currentNode as ChoiceNodeSO;
            if (choiceNodeSo)
            {
                foreach (var c in choiceNodeSo.choices)
                {
                    foreach (var n in c.port.targetNodes)
                    {
                        if (!visitedNodes.Contains(n))
                        {
                            DFSUtil(n, visitedNodes);
                        }
                    }
                }
                
                foreach (var n in choiceNodeSo.defaultPort.targetNodes)
                {
                    if (!visitedNodes.Contains(n))
                    {
                        DFSUtil(n, visitedNodes);
                    }
                }
            }
            
            BranchNodeSO branchNodeSo = currentNode as BranchNodeSO;
            if (branchNodeSo)
            {
                foreach (var n in branchNodeSo.TrueOutputPort.targetNodes)
                {
                    if (!visitedNodes.Contains(n))
                    {
                        DFSUtil(n, visitedNodes);
                    }
                }
                foreach (var n in branchNodeSo.FalseOutputPort.targetNodes)
                {
                    if (!visitedNodes.Contains(n))
                    {
                        DFSUtil(n, visitedNodes);
                    }
                }
                
            }
        }
    }
}

