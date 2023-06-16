
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Action<NodeView> onNodeSelected;
    public Node node;
    public Port input;
    public Port output;
        
    public NodeView(Node node): base("Assets/dialog-system/NodeView.uxml")
    {
        this.node = node;
        this.title = node.name;
        this.viewDataKey = node.guid;
        if (node is RootNode)
        {
            capabilities -= Capabilities.Deletable;
            capabilities -= Capabilities.Copiable;
        }
        
        style.left = node.position.x;
        style.top = node.position.y;

        CreateInputPorts();
        CreateOutputPorts();
        SetupClasses();

        Label messageLabel = this.Q<Label>("message-label");
        messageLabel.bindingPath = "message";
        messageLabel.Bind(new SerializedObject(node));
    }

    private void SetupClasses()
    {
        if (node is DialogNode)
        {
            AddToClassList("dialog");
        }else if (node is RootNode)
        {
            AddToClassList("root");
        }
    }
    
    private void CreateInputPorts()
    {
        if (node is DialogNode)
        {
            input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        }

        if (input != null)
        {
            input.portName = "";
            inputContainer.Add(input);
        }

    }

    private void CreateOutputPorts()
    {
        if (node is DialogNode or RootNode)
        {
            output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        }
        
        if (output != null)
        {
            output.portName = "";
            outputContainer.Add(output);
        }
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        Undo.RecordObject(node, "Conversation Tree (Set Position)");
        node.position.x = newPos.xMin;
        node.position.y = newPos.yMin;
        EditorUtility.SetDirty(node);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if (onNodeSelected != null)
        {
            onNodeSelected.Invoke(this);
        }
    }

    public void SortChildren()
    {
        node.children.Sort(SortByVerticalPosition);
    }

    private int SortByVerticalPosition(Node top, Node bottom)
    {
        return top.position.y < bottom.position.y ? -1 : 1;
    }

    public void UpdateState()
    {
        RemoveFromClassList("initial");
        RemoveFromClassList("running");
        RemoveFromClassList("unreachable");
        RemoveFromClassList("finished");
        
        if (!Application.isPlaying) return;
        switch (node.NodeState)
        {
            case Node.State.Initial:
                AddToClassList("initial");
                break;
            case Node.State.Running:
                AddToClassList("running");
                break;
            case Node.State.Unreachable:
                AddToClassList("unreachable");
                break;
            case Node.State.Finished:
                AddToClassList("finished");
                break;
        }
    }
}
