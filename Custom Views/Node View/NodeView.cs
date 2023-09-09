
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Action<NodeView> onNodeSelected;
    public Node node;
    public Port input;
    public Port output;
    
    private readonly SpritePreviewElement _actorSprite;
    private readonly TextField _messageTextField;
    private Label _actorLabel;
    private Actor _actor;
    private DialogSystemView _graphView;
    
        
    public NodeView(Node node, DialogSystemView graphView): base("Assets/dialog-system/Custom Views/Node View/NodeView.uxml")
    {
        _graphView = graphView;
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

        DialogNode dialogNode = node as DialogNode;
        if (dialogNode)
        {
            SpritePreviewElement sprite = this.Q<SpritePreviewElement>("actor-sprite");
            _actorSprite = sprite;
            _actorSprite.bindingPath = "actorImage";
            
            Label label = this.Q<Label>("actor-name-text");
            _actorLabel = label;
            _actorLabel.bindingPath = "fullName";
            
            if (dialogNode.actor && dialogNode.actor.actorImage)
            {
                BindActor(dialogNode.actor);
            }
            else
            {
                _actorSprite.value = Resources.Load<Sprite>( "unknown-person" );
                _actorLabel.text = "-";
            }
            
            _messageTextField = this.Q<TextField>("message-textfield");
            BindMessage();
        }
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

    public void UpdateActorToBind(Actor selectedActor)
    {
        BindActor(selectedActor);
    }

    private void BindActor(Actor selectedActor)
    {
        this.Unbind();
        this.TrackSerializedObjectValue(new SerializedObject(selectedActor), CheckForWarnings);
        _actor = selectedActor;
        SerializedObject actor = new SerializedObject(_actor);
        _actorSprite.Bind(actor);
        _actorLabel.Bind(actor);
        _actorSprite.style.backgroundColor = _actor.bgColor;
        _actorLabel.style.backgroundColor = _actor.bgColor;
        BindMessage();
    }

    void BindMessage()
    {
        if (_messageTextField == null) return;
        
        _messageTextField.bindingPath = "message";
        _messageTextField.Bind(new SerializedObject(node));
    }
    
    void CheckForWarnings(SerializedObject serializedObject)
    {
        _actorSprite.style.backgroundColor = _actor.bgColor;
        _actorLabel.style.backgroundColor = _actor.bgColor;
    }
}
