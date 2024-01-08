using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogNodeView : NodeView
{
    private SpritePreviewElement _actorSprite;
    private TextField _messageTextField;
    private VisualElement _outputChoiceContainer;
    private Label _actorLabel;
    private Actor _actor;
    private Action _onClearSelection;
    private Button _disabledChoiceButton;
    private VisualElement _actorContainer;
    private SerializedObject _actorSerialized;
    private Dictionary<string, KeyValuePair<VisualElement, Port>> _choicesMap = new();
    
    private DialogSystemView _graphView;
    private DialogSystemDatabase _currentDatabase;

    private Port _defaultPort;
    
    // Start is called before the first frame update
    public  DialogNodeView(NodeSO nodeSo, DialogSystemView graphView, Action onClearSelection) : base(nodeSo)
    {
        _graphView = graphView;
        _currentDatabase = graphView.GetDatabase();
        _onClearSelection = onClearSelection;
        
        
        var nodeViewContainer = new DialogNodeViewContainer();
        extensionContainer.Add(nodeViewContainer);
        CreateDefaultInputPorts();
        DialogNodeSO dialogNodeSo = nodeSo as DialogNodeSO;
        if (dialogNodeSo)
        {
            CreateDefaultOutputPorts();
            SetUpTopData(dialogNodeSo);
            BindMessage();
            AddToClassList("dialog");
            VisualElement topContainerView = this.Q("top");
            topContainerView.style.flexDirection = FlexDirection.Row;
            inputContainer.AddToClassList("singleInputContainer");
            outputContainer.AddToClassList("singleOutputContainer");
        }
                
        ChoiceNodeSO choiceNodeSo = nodeSo as ChoiceNodeSO;
        if (choiceNodeSo)
        {
            SetUpTopData(choiceNodeSo);
            BindMessage();
            SetUpChoiceNode(choiceNodeSo);
        }
        RefreshExpandedState();
    }

    private void SetUpTopData(ConversationNodeSO conversationNode)
    {
        GetAndBindActor(conversationNode.actor,(actor) =>
        {
            conversationNode.actor = actor;
            EditorUtility.SetDirty(conversationNode);
            onNodeSelected.Invoke(this);
            _onClearSelection.Invoke();
        });
        _messageTextField = this.Q<TextField>("message-textfield");
#if LOCALIZATION_EXIST
        if (DSData.instance.tableCollection && DSData.instance.database.defaultLocale)
        {
            conversationNode.message = LocalizationUtils.GetDefaultLocaleLocalizedString(conversationNode.guid);
        }
#endif
    }
    
    private void SetUpChoiceNode(ChoiceNodeSO choiceNodeSo)
    {
        AddToClassList("choice");
        inputContainer.AddToClassList("choiceInputContainer");
        outputContainer.AddToClassList("choiceOutputContainer");
        
        TextField newChoiceField = new TextField();
        newChoiceField.RegisterCallback<FocusEvent>((e) =>{_onClearSelection.Invoke();onNodeSelected(this);});
        newChoiceField.RegisterCallback<KeyDownEvent>(e =>
        {
            if (e.keyCode != KeyCode.Return) return;
            CreateNewOutput(choiceNodeSo,newChoiceField.value);
            newChoiceField.value = "";
            newChoiceField.Focus();
            CheckOutputRemaining();
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
        foreach (var choice in choiceNodeSo.choices)
        {
            CreateNewOutputView(choiceNodeSo, choice);
        }

        if (choiceNodeSo.choices.Count == 0)
        {
            CreateNewOutput(choiceNodeSo, "Choice 1");
            CreateNewOutput(choiceNodeSo, "Choice 2");
        }
        else
        {
            CheckOutputRemaining();
        }
        
        // Register New Callback
        newChoiceButton.clickable = new Clickable(() =>
        {
            CreateNewOutput(choiceNodeSo, newChoiceField.value);
            newChoiceField.value = "";
            newChoiceField.Focus();
            CheckOutputRemaining();
        });

        CreateDefaultChoiceView();
    }
        
    private void CreateDefaultChoiceView()
    {
        VisualElement ve = new VisualElement();
        
        TextField defaultTextfield = new TextField()
        {
            value = "Default choice",
            focusable = false
        };
        defaultTextfield.AddToClassList("default-choice-textfield");
        ve.Add(defaultTextfield);

        if (node is ChoiceNodeSO choiceNodeSo)
        {
            _defaultPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            _defaultPort.portName = "";
            _defaultPort.viewDataKey = choiceNodeSo.defaultPort.id;
            ve.Add(_defaultPort);
            ve.AddToClassList("defaultChoiceDataContainer");
            topContainer.Add(ve);
        }
    }

    public void OnChoiceAdded(ChoiceNodeSO choiceNodeSo, Choice choice)
    {
        CreateNewOutputView(choiceNodeSo,choice);
        CheckOutputRemaining();
    }
    
    public void OnChoiceRemoved(Choice choice)
    {
        if (_choicesMap.TryGetValue(choice.guid, out var entry))
        {
            RemoveOutputView(entry.Key, entry.Value, choice);
        }
    }
    
    private void CreateNewOutput(ChoiceNodeSO choiceNodeSo, string defaultText = "Default")
    {
        Choice choice = choiceNodeSo.CreateChoice(_currentDatabase,defaultText);
#if LOCALIZATION_EXIST
        LocalizationUtils.SetDefaultLocaleEntry(node.guid,defaultText);
#endif
        onRefreshInspector?.Invoke();
        CreateNewOutputView(choiceNodeSo, choice);
    }

    private void CreateNewOutputView(ChoiceNodeSO choiceNodeSo, Choice choice)
    {
        VisualElement ve = new VisualElement();
        
        // Add Delete Button
        Button deleteButton = new Button();
        deleteButton.Add(new Image{
            image = EditorGUIUtility.IconContent("TreeEditor.Trash").image
        });
        deleteButton.AddToClassList("choiceDataBtn");
        ve.Add(deleteButton);
        
        // Add Choice Text Field
        TextField choiceTextField = new TextField()
        {
            value = choice.choiceMessage,
            multiline = true
        };
        choiceTextField.AddToClassList("choice-textfield");
        choiceTextField.bindingPath = "choiceMessage";
        choiceTextField.Bind(new SerializedObject(choice));
        choiceTextField.RegisterCallback<FocusEvent>((e) =>{_onClearSelection.Invoke();onNodeSelected(this);});
#if LOCALIZATION_EXIST
        if (DSData.instance.tableCollection && DSData.instance.database.defaultLocale)
        {
            choiceTextField.RegisterCallback<FocusOutEvent>((e) =>
            {
                string valTrimmed = choiceTextField.value.Trim(' ');
                string previousString = LocalizationUtils.GetDefaultLocaleLocalizedString(choice.guid);
                if (valTrimmed != "" && valTrimmed != previousString)
                {
                    LocalizationUtils.SetDefaultLocaleEntry(choice.guid, valTrimmed);
                }
                string finalTranslation = LocalizationUtils.GetDefaultLocaleLocalizedString(choice.guid);
                choiceTextField.value = finalTranslation;
            });
        }
#endif
        ve.Add(choiceTextField);
        
        // Add Choice Port
        var newOutput = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        newOutput.portName = "";
        newOutput.viewDataKey = choice.port.id;
        outputPortsList.Add(newOutput);
        outputContainer.Add(newOutput);
        ve.Add(newOutput);
        
        ve.AddToClassList("choiceDataContainer");
        outputContainer.Add(ve);
        _choicesMap.Add(choice.guid, new KeyValuePair<VisualElement, Port>(ve,newOutput));

        deleteButton.clickable = new Clickable(() =>
        {
            RemoveOutputView(ve, newOutput, choice);
            choiceNodeSo.DeleteChoice(choice);
#if LOCALIZATION_EXIST
            LocalizationUtils.RemoveKeyFromCollection(node.guid);
#endif
            onRefreshInspector();
        });
    }

    private void RemoveOutputView(VisualElement ve,Port outputPort, Choice choice)
    {
        List<Edge> edges = new List<Edge>(outputPort.connections);
        _graphView.DeleteElements(edges);
        outputPortsList.Remove(outputPort);
        outputContainer.Remove(ve);
        //choicePortTranslationMap.Remove(outputPort);
        _choicesMap.Remove(choice.guid);
        CheckOutputRemaining();
    }
    

    public List<Edge> GetAllChoiceEdges()
    {
        List<Edge> edgesToRemove = new List<Edge>();
        if (node is not ChoiceNodeSO) return edgesToRemove;
        foreach(Port port in outputPortsList)
        {
            if (!port.connected) continue;
            List<Edge> edges = new List<Edge>(port.connections);
            edgesToRemove = edgesToRemove.Concat(edges).ToList();
        } 

        if (_defaultPort.connected) // For Default choice
        {
            List<Edge> edges = new List<Edge>(_defaultPort.connections);
            edgesToRemove = edgesToRemove.Concat(edges).ToList();
        }
        return edgesToRemove;
    }
     public void UpdateActorToBind(Actor selectedActor)
    {
        BindActor(selectedActor);
    }

#if LOCALIZATION_EXIST
    public void UpdateLocalizedMessage()
    {
        DialogNodeSO dialogNodeSo = node as DialogNodeSO;
        if (dialogNodeSo)
        {
            dialogNodeSo.message = LocalizationUtils.GetDefaultLocaleLocalizedString(node.guid);
        }
        
        ChoiceNodeSO choiceNodeSo = node as ChoiceNodeSO;
        if (choiceNodeSo)
        {
            choiceNodeSo.message = LocalizationUtils.GetDefaultLocaleLocalizedString(node.guid);
            foreach (var choice in choiceNodeSo.choices)
            {
                choice.choiceMessage = LocalizationUtils.GetDefaultLocaleLocalizedString(choice.guid);
            }
        }
    }
#endif

    private void GetAndBindActor(Actor actor, Action<Actor> onSelectActor)
    {
        _actorContainer = this.Q("node-container");
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
            
        if (actor)
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
        _actorContainer.Unbind();
        _actor = selectedActor;
        _actorSerialized = new SerializedObject(_actor);
        _actorContainer.TrackSerializedObjectValue(_actorSerialized, CheckForWarnings);
        SerializedObject actor = new SerializedObject(_actor);
        _actorSprite.Bind(actor);
        _actorLabel.Bind(actor);
        _actorSprite.style.backgroundColor = _actor.bgColor;
        _actorLabel.style.backgroundColor = _actor.bgColor;
        _messageTextField?.Bind(new SerializedObject(node));
    }

    private void BindMessage()
    {
        if (_messageTextField == null) return;
        
        _messageTextField.bindingPath = "message";
        _messageTextField.Bind(new SerializedObject(node));
        _messageTextField.RegisterCallback<FocusEvent>((e) =>{onNodeSelected(this); _onClearSelection.Invoke();});
#if LOCALIZATION_EXIST
        if (DSData.instance.tableCollection && DSData.instance.database.defaultLocale)
        {
            _messageTextField.RegisterCallback<FocusOutEvent>((e) =>
            {
                string valTrimmed = _messageTextField.value.Trim(' ');
                string previousString = LocalizationUtils.GetDefaultLocaleLocalizedString(node.guid);
                if (valTrimmed != "" && valTrimmed != previousString)
                {
                    LocalizationUtils.SetDefaultLocaleEntry(node.guid,valTrimmed);
                }

                string finalTranslation = LocalizationUtils.GetDefaultLocaleLocalizedString(node.guid);
                _messageTextField.value = finalTranslation;
            });
        }
#endif
    }
    
    private void CheckForWarnings(SerializedObject serializedObject)
    {
        _actorSprite.style.backgroundColor = _actor.bgColor;
        _actorLabel.style.backgroundColor = _actor.bgColor;
    }

    private void CheckOutputRemaining()
    {
        if (outputContainer.childCount == 1)
        {
            VisualElement choiceElement = outputContainer.ElementAt(0);
            Button deleteButton = choiceElement.Q<Button>(className:"choiceDataBtn");
            deleteButton.SetEnabled(false);
            _disabledChoiceButton = deleteButton;
        }
        else if (_disabledChoiceButton != null)
        {
            _disabledChoiceButton.SetEnabled(true);
            _disabledChoiceButton = null;
        }
    }
}
