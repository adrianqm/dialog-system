
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Action<NodeView> onNodeSelected;
    public Node node;
    public Port input;
    public Port output;
    public readonly Dictionary<Port, Choice> portTranslationMap; 
    
    private SpritePreviewElement _actorSprite;
    private TextField _messageTextField;
    private VisualElement _outputChoiceContainer;
    private Label _actorLabel;
    private Actor _actor;
    private DialogSystemView _graphView;
    private DialogSystemDatabase _currentDatabase;
    private Action _onClearSelection;
    
        
    public NodeView(Node node, DialogSystemView graphView, Action onClearSelection): base("Assets/dialog-system/Custom Views/Node View/NodeView.uxml")
    {
        _graphView = graphView;
        _currentDatabase = graphView.GetDatabase();
        portTranslationMap = new Dictionary<Port, Choice>();
        _onClearSelection = onClearSelection;
        
        this.node = node;
        this.title = node.name;
        this.viewDataKey = node.guid;
        
        style.left = node.position.x;
        style.top = node.position.y;
        
        if (node is StartNode)
        {
            capabilities -= Capabilities.Deletable;
            capabilities -= Capabilities.Copiable;
            AddToClassList("static");
            Label staticField = this.Q<Label>("static-label");
            staticField.text = "[START]";
            inputContainer.AddToClassList("singleInputContainer");
            outputContainer.AddToClassList("singleOutputContainer");

            if (Application.isPlaying) node.NodeState = Node.State.Running;
            else node.NodeState = Node.State.Initial;
            
            CreateSingleOutputPorts();
        }
        else if (node is CompleteNode)
        {
            capabilities -= Capabilities.Deletable;
            capabilities -= Capabilities.Copiable;
            AddToClassList("static");
            Label staticField = this.Q<Label>("static-label");
            staticField.text = "[COMPLETE]";
            inputContainer.AddToClassList("singleInputContainer");
            outputContainer.AddToClassList("singleOutputContainer");
            CreateInputPorts();
        }
        else
        {
            CreateInputPorts();
            
            DialogNode dialogNode = node as DialogNode;
            if (dialogNode)
            {
                GetAndBindActor(dialogNode.actor,(actor) =>
                {
                    dialogNode.actor = actor;
                    EditorUtility.SetDirty(node);
                    onNodeSelected.Invoke(this);
                    _onClearSelection.Invoke();
                });
                _messageTextField = this.Q<TextField>("message-textfield");
                BindMessage();
                
                AddToClassList("dialog");
                VisualElement topContainerView = this.Q("top");
                topContainerView.style.flexDirection = FlexDirection.Row;
                inputContainer.AddToClassList("singleInputContainer");
                outputContainer.AddToClassList("singleOutputContainer");
                CreateSingleOutputPorts();
            }
            
            ChoiceNode choiceNode = node as ChoiceNode;
            if (choiceNode)
            {
                GetAndBindActor(choiceNode.actor, (actor) =>
                {
                    choiceNode.actor = actor;
                    EditorUtility.SetDirty(node);
                    onNodeSelected.Invoke(this);
                    _onClearSelection.Invoke();
                });
                _messageTextField = this.Q<TextField>("message-textfield");
                BindMessage();
                
                SetUpChoiceNode(choiceNode);
            }
        }
    }
    
    private void CreateInputPorts()
    {
        input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        input.portName = "";
        inputContainer.Add(input);
    }

    private void CreateSingleOutputPorts()
    {
        output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        output.portName = "";
        outputContainer.Add(output);
    }

    private void SetUpChoiceNode(ChoiceNode choiceNode)
    {
        AddToClassList("choice");
        inputContainer.AddToClassList("choiceInputContainer");
        outputContainer.AddToClassList("choiceOutputContainer");
        
        TextField newChoiceField = new TextField();
        newChoiceField.RegisterCallback<KeyDownEvent>(e =>
        {
            if (e.keyCode == KeyCode.Return)
            {
                CreateNewOutput(choiceNode,newChoiceField.value);
                newChoiceField.value = "";
                newChoiceField.Focus();
            }
        });
        newChoiceField.RegisterCallback<FocusOutEvent>((e) =>
        {
            newChoiceField.value = "";
        });
        newChoiceField.AddToClassList("choiceDataAddText");
        inputContainer.Add(newChoiceField);
        
        Button newChoiceButton = new Button();
        newChoiceButton.Add(new Image {
            image = EditorGUIUtility.IconContent("d_CreateAddNew").image
        });
        newChoiceButton.AddToClassList("choiceDataBtn");
        inputContainer.Add(newChoiceButton);
        
        // Draw previous Choices
        foreach (var choice in choiceNode.choices)
        {
            CreateNewOutputView(choiceNode, choice);
        }
        // Register New Callback
        newChoiceButton.clickable = new Clickable(() =>CreateNewOutput(choiceNode));
    }
    
    private void CreateNewOutput(ChoiceNode choiceNode, string defaultText = "")
    {
        Choice choice = choiceNode.CreateChoice(_currentDatabase, defaultText);
        CreateNewOutputView(choiceNode, choice);
    }

    private void CreateNewOutputView(ChoiceNode choiceNode, Choice choice)
    {
        VisualElement ve = new VisualElement();
        
        Button deleteButton = new Button();
        deleteButton.Add(new Image{
            image = EditorGUIUtility.IconContent("TreeEditor.Trash").image
        });
        deleteButton.AddToClassList("choiceDataBtn");
        ve.Add(deleteButton);
        
        TextField choiceTextField = new TextField()
        {
            value = choice.choiceMessage,
            multiline = true
        };
        choiceTextField.AddToClassList("choice-textfield");
        choiceTextField.bindingPath = "choiceMessage";
        choiceTextField.Bind(new SerializedObject(choice));
        choiceTextField.RegisterCallback<FocusEvent>((e) =>{onNodeSelected(this); _onClearSelection.Invoke();});
        ve.Add(choiceTextField);
        
        Port newOutput = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        newOutput.portName = "";
        ve.Add(newOutput);
        
        ve.AddToClassList("choiceDataContainer");
        outputContainer.Add(ve);
        portTranslationMap.Add(newOutput,choice);

        deleteButton.clickable = new Clickable(() =>
        {
            choiceTextField.Unbind();
            choiceNode.DeteleChoice(choice);
            List<Edge> edges = new List<Edge>(newOutput.connections);
            _graphView.DeleteElements(edges);
            outputContainer.Remove(ve);
            portTranslationMap.Remove(newOutput);
        });
    }

    public Choice FindPortChoice(Port port)
    {
        return portTranslationMap[port];
    }

    public List<Edge> DeleteAndGetAllChoiceEdges()
    {
        List<Edge> edgesToRemove = new List<Edge>();
        if (node is not ChoiceNode) return edgesToRemove;
        ChoiceNode choiceNode = node as ChoiceNode;
        foreach(KeyValuePair<Port, Choice> entry in portTranslationMap)
        {
            Port port = entry.Key;
            if (choiceNode != null) choiceNode.DeteleChoice(entry.Value);
            if (!port.connected) continue;
            List<Edge> edges = new List<Edge>(port.connections);
            edgesToRemove = edgesToRemove.Concat(edges).ToList();
        }
        return edgesToRemove;
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
        ParentNode parentNode = node as ParentNode;
        if (parentNode)
        {
            parentNode.children.Sort(SortByVerticalPosition);
        }
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
        RemoveFromClassList("visited");
        RemoveFromClassList("visitedUnreachable");
        
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
            case Node.State.Visited:
                AddToClassList("visited");
                break;
            case Node.State.VisitedUnreachable:
                AddToClassList("visitedUnreachable");
                break;
        }
    }

    public void UpdateActorToBind(Actor selectedActor)
    {
        BindActor(selectedActor);
    }

    private void GetAndBindActor(Actor actor, Action<Actor> onSelectActor)
    {
        SpritePreviewElement sprite = this.Q<SpritePreviewElement>("actor-sprite");
        _actorSprite = sprite;
        _actorSprite.bindingPath = "actorImage";
            
        Label label = this.Q<Label>("actor-name-text");
        _actorLabel = label;
        _actorLabel.bindingPath = "fullName";
        
        VisualElement actorSearchVe = this.Q("actor-search");
        Button actorSearchButton = new Button();
        actorSearchButton.Add(new Image {
            image = EditorGUIUtility.IconContent("d_pick_uielements").image
        });
        actorSearchButton.AddToClassList("actorSearchBtn");
        actorSearchButton.clickable = new Clickable(() =>
        {
            ActorsSearchProvider provider =
                ScriptableObject.CreateInstance<ActorsSearchProvider>();
            provider.SetUp(_currentDatabase.actors,
                (actorSelected) =>
                {
                    UpdateActorToBind(actorSelected);
                    onSelectActor.Invoke(actorSelected);
                });
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                provider);
        });
        actorSearchVe.Add(actorSearchButton);
            
        if (actor && actor.actorImage)
        {
            BindActor(actor);
        }
        else
        {
            _actorSprite.value = Resources.Load<Sprite>( "unknown-person" );
            _actorLabel.text = "-";
        }
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
        _messageTextField.RegisterCallback<FocusEvent>((e) =>{onNodeSelected(this); _onClearSelection.Invoke();});
    }
    
    void CheckForWarnings(SerializedObject serializedObject)
    {
        _actorSprite.style.backgroundColor = _actor.bgColor;
        _actorLabel.style.backgroundColor = _actor.bgColor;
    }
}
