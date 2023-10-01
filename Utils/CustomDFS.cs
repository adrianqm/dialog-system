using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomDFS
{
    private static List<Node> visitedNodes;
    
    public static List<Node> StartDFS (Node currentNode)
    {
        visitedNodes = new List<Node>();
        DFSUtil(currentNode, visitedNodes);
        return visitedNodes;
    }

    private static void DFSUtil(Node currentNode, List<Node> visitedNodes)
    {
        visitedNodes.Add(currentNode);
        ParentNode parentNode = currentNode as ParentNode;
        if (parentNode)
        {
            foreach (var n in parentNode.children)
            {
                if (!visitedNodes.Contains(n))
                {
                    DFSUtil(n, visitedNodes);
                }
            }
        }
    }
}
