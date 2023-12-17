
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AQM.Tools
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> onNodeSelected;
        public Action onRefreshInspector;
        public Node node;
        public Port input;
        public Port output;
        public readonly Dictionary<Port, Choice> choicePortTranslationMap;
        
        private SpritePreviewElement _actorSprite;
        private TextField _messageTextField;
        private VisualElement _outputChoiceContainer;
        private Label _actorLabel;
        private Actor _actor;
        private DialogSystemView _graphView;
        private DialogSystemDatabase _currentDatabase;
        private Action _onClearSelection;
        private Button _disabledChoiceButton;
        private VisualElement _actorContainer;
        private SerializedObject _actorSerialized;
        private Dictionary<string, KeyValuePair<VisualElement, Port>> _choicesMap = new();
            
        public NodeView(Node node, DialogSystemView graphView, Action onClearSelection): base("Assets/dialog-system/Editor/Custom Views/Node View/NodeView.uxml")
        {
            _graphView = graphView;
            _currentDatabase = graphView.GetDatabase();
            choicePortTranslationMap = new Dictionary<Port, Choice>();
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

                if (Application.isPlaying)
                {
                    if (node.NodeState == Node.State.Initial)
                    {
                        node.NodeState = Node.State.Running;
                    }
                }
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
    #if LOCALIZATION_EXIST
                    if (DSData.instance.tableCollection && DSData.instance.database.defaultLocale)
                    {
                        dialogNode.message = LocalizationUtils.GetDefaultLocaleLocalizedString(node.guid);
                    }
    #endif
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
                        onRefreshInspector.Invoke();
                        _onClearSelection.Invoke();
                    });
                    _messageTextField = this.Q<TextField>("message-textfield");
    #if LOCALIZATION_EXIST
                    if (DSData.instance.tableCollection && DSData.instance.database.defaultLocale)
                    {
                        choiceNode.message = LocalizationUtils.GetDefaultLocaleLocalizedString(node.guid);
                    }
    #endif
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
            newChoiceField.RegisterCallback<FocusEvent>((e) =>{_onClearSelection.Invoke();onNodeSelected(this);});
            newChoiceField.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode != KeyCode.Return) return;
                CreateNewOutput(choiceNode,newChoiceField.value);
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
            foreach (var choice in choiceNode.choices)
            {
                CreateNewOutputView(choiceNode, choice);
            }

            if (choiceNode.choices.Count == 0)
            {
                CreateNewOutput(choiceNode, "Choice 1");
                CreateNewOutput(choiceNode, "Choice 2");
            }
            else
            {
                CheckOutputRemaining();
            }
            
            // Register New Callback
            newChoiceButton.clickable = new Clickable(() =>
            {
                CreateNewOutput(choiceNode, newChoiceField.value);
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
            
            output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            output.portName = "";
            ve.Add(output);
            ve.AddToClassList("defaultChoiceDataContainer");
            topContainer.Add(ve);
        }

        public void OnChoiceAdded(ChoiceNode choiceNode, Choice choice)
        {
            CreateNewOutputView(choiceNode,choice);
            CheckOutputRemaining();
        }
        
        public void OnChoiceRemoved(Choice choice)
        {
            if (_choicesMap.TryGetValue(choice.guid, out var entry))
            {
                RemoveOutputView(entry.Key, entry.Value, choice);
            }
        }
        
        private void CreateNewOutput(ChoiceNode choiceNode, string defaultText = "Default")
        {
            Choice choice = ChoiceUtils.CreateChoice(_currentDatabase,choiceNode,defaultText);
            onRefreshInspector?.Invoke();
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
            
            Port newOutput = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            newOutput.portName = "";
            ve.Add(newOutput);
            
            ve.AddToClassList("choiceDataContainer");
            outputContainer.Add(ve);
            choicePortTranslationMap.Add(newOutput,choice);
            _choicesMap.Add(choice.guid, new KeyValuePair<VisualElement, Port>(ve,newOutput));

            deleteButton.clickable = new Clickable(() =>
            {
                ChoiceUtils.DeleteChoice(choiceNode,choice);
                RemoveOutputView(ve, newOutput, choice);
                onRefreshInspector();
            });
        }

        private void RemoveOutputView(VisualElement ve,Port outputPort, Choice choice)
        {
            List<Edge> edges = new List<Edge>(outputPort.connections);
            _graphView.DeleteElements(edges);
            outputContainer.Remove(ve);
            choicePortTranslationMap.Remove(outputPort);
            _choicesMap.Remove(choice.guid);
            CheckOutputRemaining();
        }

        public Choice FindPortChoice(Port port)
        {
            return choicePortTranslationMap.TryGetValue(port, out var value)? value: null;
        }

        public List<Edge> DeleteAndGetAllChoiceEdges()
        {
            List<Edge> edgesToRemove = new List<Edge>();
            if (node is not ChoiceNode) return edgesToRemove;
            ChoiceNode choiceNode = node as ChoiceNode;
            foreach(KeyValuePair<Port, Choice> entry in choicePortTranslationMap)
            {
                Port port = entry.Key;
                if (choiceNode != null)
                {
                    ChoiceUtils.DeleteChoice(choiceNode, entry.Value);
                }
                if (!port.connected) continue;
                List<Edge> edges = new List<Edge>(port.connections);
                edgesToRemove = edgesToRemove.Concat(edges).ToList();
            }

            if (output.connected) // For Default choice
            {
                List<Edge> edges = new List<Edge>(output.connections);
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

    #if LOCALIZATION_EXIST
        public void UpdateLocalizedMessage()
        {
            DialogNode dialogNode = node as DialogNode;
            if (dialogNode)
            {
                dialogNode.message = LocalizationUtils.GetDefaultLocaleLocalizedString(node.guid);
            }
            
            ChoiceNode choiceNode = node as ChoiceNode;
            if (choiceNode)
            {
                choiceNode.message = LocalizationUtils.GetDefaultLocaleLocalizedString(node.guid);
                foreach (var choice in choiceNode.choices)
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
}

