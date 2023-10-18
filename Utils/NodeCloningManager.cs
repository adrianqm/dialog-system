using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeCloningManager
{
    private Dictionary<Node, Node> clonedNodes = new Dictionary<Node, Node>();

    public Node CloneNode(Node originalNode)
    {
        if (originalNode == null)
        {
            return null;
        }

        if (clonedNodes.TryGetValue(originalNode, out var node))
        {
            return node;
        }

        Node clonedNode = originalNode.Clone();
        clonedNodes.Add(originalNode, clonedNode);
        
        ParentNode parentNode = originalNode as ParentNode;
        if (parentNode && parentNode.children != null)
        {
            ParentNode parentClonedNode = clonedNode as ParentNode;
            parentClonedNode.children = new List<Node>();
            foreach (Node child in parentNode.children)
            {
                Node clonedChild = CloneNode(child);
                parentClonedNode.children.Add(clonedChild);
            }
        }
        
        ChoiceNode choiceNode = originalNode as ChoiceNode;
        if (choiceNode && choiceNode.choices != null)
        {
            ChoiceNode clonedChoiceNode = clonedNode as ChoiceNode;
            clonedChoiceNode.choices = new List<Choice>();
            foreach (Choice choice in choiceNode.choices)
            {
                Choice clonedChoice = choice.Clone();
                clonedChoice.children = new List<Node>();
                
                foreach (Node child in choice.children)
                {
                    Node choiceClonedChild = CloneNode(child);
                    clonedChoice.children.Add(choiceClonedChild);
                }
                clonedChoiceNode.choices.Add(clonedChoice);
            }
        }
        return clonedNode;
    }
}