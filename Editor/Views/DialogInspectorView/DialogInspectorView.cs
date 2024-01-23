using System;
using System.Collections.Generic;
using AQM.Tools;
using Blackboard.Commands;
using Blackboard.Editor.Commands;
using Blackboard.Editor.Requirement;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;

#if LOCALIZATION_EXIST
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
#endif
using UnityEngine.UIElements;
using Action = System.Action;

public class DialogInspectorView : VisualElement
{
    private readonly string assetName = "DialogInspectorView";
    
    private DialogSystemDatabase _currentDatabase;
    private DialogNodeView _nodeView;
    private NodeSO _node;
    private DialogNodeSO _dialogNode;
    private ChoiceNodeSO _choiceNode;
    private List<Actor> _actors;
    private readonly TextField _messageTextField;
    private VisualElement _choicesAddVE;
    private VisualElement _choicesFoldoutVE;
    private Button _disabledChoiceButton;
    private Button _findActorButton;
    private VisualElement _actorContainer;
    private Actor _actor;
    private Dictionary<string, KeyValuePair<Label,TextField>> _translationMapText = new();
    private SpritePreviewElement _actorSprite;
    private Label _actorName;
    private SerializedObject _actorSerialized;
#if LOCALIZATION_EXIST
private StringTableCollection _collection;
#endif
    private Dictionary<string, VisualElement> _choicesMap = new();
    private Action _unregisterAll = null;
    
    public DialogInspectorView(NodeView nodeView)
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
        uxml.CloneTree(this);

        _currentDatabase = DSData.instance.database;
        _nodeView = nodeView as DialogNodeView;
        _node = nodeView.node;
        _actors = DSData.instance.database.actors;
        _messageTextField = this.Q<TextField>("message-text-field");
        SetUp();
    }

    void SetUp()
    {
        _choicesMap = new();
        if (_node is not StartNodeSO && _node is not CompleteNodeSO)
        {
            if(_node is DialogNodeSO)
            {
                _dialogNode = _node as DialogNodeSO;
                PopulateDialogInspector(_dialogNode);
            }else if (_node is ChoiceNodeSO)
            {
                _choiceNode = _node as ChoiceNodeSO;
                PopulateChoiceInspector(_choiceNode);
            }
        }
    }

    private void PopulateDialogInspector(DialogNodeSO node)
    {
        if (node == null) return;
        SetUpDialogClasses();
        GetAndBindActor(node.actor, (actor) =>
        {
            node.actor = actor;
            EditorUtility.SetDirty(node);
            _nodeView.SetDefaultActorIfNotExist(actor);
        });
        BindDelayTime(node);
#if LOCALIZATION_EXIST
        if (!DSData.instance.tableCollection || !DSData.instance.database.defaultLocale)
        {
            BindMessage(node);
        }
        else
        {
            BindLocalization(node);
        }
#else
        BindMessage(node);
#endif
    }

    private void PopulateChoiceInspector(ChoiceNodeSO node)
    {
        if (node == null) return;
        SetUpChoiceClasses();
        GetAndBindActor(node.actor,(actor) =>
        {
            node.actor = actor;
            EditorUtility.SetDirty(node);
            _nodeView.SetDefaultActorIfNotExist(actor);
        });
        BindDelayTime(node);
#if LOCALIZATION_EXIST
        if (!DSData.instance.tableCollection || !DSData.instance.database.defaultLocale)
        {
            BindMessage(node);
        }
        else
        {
            BindLocalization(node);
        }
#else
        BindMessage(node);
#endif
        BindChoices(node);
    }

    private void BindChoices(ChoiceNodeSO node)
    {
        _choicesAddVE = this.Q("choices-add");
        _choicesAddVE.Clear();
        _choicesFoldoutVE = this.Q("choices-foldout");
        _choicesFoldoutVE.Clear();
        TextField addField = AddChoiceTextField(node);
        Button addButton = AddChoiceButton(node);
        
        // Draw previous Choices
        foreach (var choice in node.choices)
        {
            CreateNewOutputView(node, choice);
        }

        CheckOutputRemaining();

        CommandListView defaultChoiceActionList = this.Q<CommandListView>("default-action-list");
        defaultChoiceActionList.SaveAsSubAssetOf(_currentDatabase);
        defaultChoiceActionList.PopulateView(node.defaultActionList);
        defaultChoiceActionList.SetHeaderTitle("On Default Selected");
        
        // Register New Callback
        addButton.clickable = new Clickable(() =>
        {
            CreateNewChoice(node, addField.value);
            addField.value = "";
            addField.Focus();
            CheckOutputRemaining();
        });
    }

    private TextField AddChoiceTextField(ChoiceNodeSO node)
    {
        TextField newChoiceField = new TextField();
        newChoiceField.RegisterCallback<KeyDownEvent>(e =>
        {
            if (e.keyCode != KeyCode.Return) return;
            CreateNewChoice(node,newChoiceField.value);
            newChoiceField.value = "";
            newChoiceField.Focus();
            CheckOutputRemaining();
        });
        newChoiceField.AddToClassList("choiceDataAddText");
        _choicesAddVE.Add(newChoiceField);
        return newChoiceField;
    }
    
    private Button AddChoiceButton(ChoiceNodeSO node)
    {
        Button newChoiceButton = new Button();
        newChoiceButton.Add(new Image {
            image = EditorGUIUtility.IconContent("d_CreateAddNew").image
        });
        newChoiceButton.AddToClassList("choiceDataBtn");
        _choicesAddVE.Add(newChoiceButton);
        return newChoiceButton;
    }

    private void CreateNewChoice(ChoiceNodeSO choiceNodeSo, string defaultText)
    {
        Choice choice = choiceNodeSo.CreateChoice(_currentDatabase, defaultText);
        CreateNewOutputView(choiceNodeSo, choice);
        _nodeView.OnChoiceAdded(choiceNodeSo,choice);
    }

    private void RemoveOutputView(VisualElement ve, Choice choice)
    {
        _translationMapText.Remove(choice.guid);
        _choicesMap.Remove(choice.guid);
        _choicesFoldoutVE.Remove(ve);
        CheckOutputRemaining();
    }

    private void CreateNewOutputView(ChoiceNodeSO choiceNodeSo, Choice choice)
    {
        VisualElement ve = new VisualElement();
        ve.AddToClassList("choiceDataContainer");
        
        Button deleteButton = new Button();
        deleteButton.Add(new Image{
            image = EditorGUIUtility.IconContent("TreeEditor.Trash").image
        });
        deleteButton.AddToClassList("choiceDeleteBtn");
        deleteButton.clickable = new Clickable(() =>
        {
            ve.Unbind();
            _nodeView.OnChoiceRemoved(choice);
            choiceNodeSo.DeleteChoice(choice);
            RemoveOutputView(ve, choice);
        });
        ve.Add(deleteButton);
        VisualElement choiceLocalizationContainer = new VisualElement();
    
        VisualElement choiceLocalization = new VisualElement();
        choiceLocalizationContainer.Add(choiceLocalization);
    
        BindElementText(choiceLocalization, choiceNodeSo, choice);
        ve.Add(choiceLocalizationContainer);
        
#if LOCALIZATION_EXIST
        if (DSData.instance.tableCollection && DSData.instance.database.defaultLocale)
        {
            VisualElement idVe = new VisualElement();
            idVe.AddToClassList("message-guid-choice-container");
            Label labelId = new Label("ID");
            labelId.AddToClassList("message-guid-id");
            idVe.Add(labelId);
            
            VisualElement copyButtonVe = new VisualElement();
            copyButtonVe.AddToClassList("message-guid-copy-container");
            Button copyButton = new Button();
            copyButton.Add(new Image {
                image = EditorGUIUtility.IconContent("d_SearchQueryAsset Icon").image
            });
            copyButton.AddToClassList("copyBtn");
            copyButton.clickable = new Clickable(() =>
            {
                TextEditor te = new TextEditor();
                te.text = choice.guid;
                te.SelectAll();
                te.Copy();
            });
            copyButtonVe.Add(copyButton);
            idVe.Add(copyButtonVe);
            ve.Add(idVe);
        }
#endif
        _choicesMap.Add(choice.guid,ve);
        _choicesFoldoutVE.Add(ve);
    }

    private void CheckOutputRemaining()
    {
        if (_choicesFoldoutVE.childCount == 1)
        {
            VisualElement choiceElement = _choicesFoldoutVE.ElementAt(0);
            Button deleteButton = choiceElement.Q<Button>(className:"choiceDeleteBtn");
            deleteButton.SetEnabled(false);
            _disabledChoiceButton = deleteButton;
        }
        else if (_disabledChoiceButton != null)
        {
            _disabledChoiceButton.SetEnabled(true);
            _disabledChoiceButton = null;
        }
    }
    
#if LOCALIZATION_EXIST
    private void BindLocalization(NodeSO nodeSo)
    {
        _collection = DSData.instance.tableCollection;
        _translationMapText.Clear();
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        
        VisualElement localizationMessageVe = this.Q("message-container-text");
        BindElementText(localizationMessageVe,nodeSo);
        
        VisualElement copyVe = this.Q("message-guid-copy-container");
        copyVe.parent.RemoveFromClassList("hidden-class");
        copyVe.Clear();
        Button copyButton = new Button();
        copyButton.Add(new Image {
            image = EditorGUIUtility.IconContent("d_SearchQueryAsset Icon").image
        });
        copyButton.AddToClassList("copyBtn");
        copyButton.clickable = new Clickable(() =>
        {
            TextEditor te = new TextEditor();
            te.text = nodeSo.guid;
            te.SelectAll();
            te.Copy();
        });
        copyVe.Add(copyButton);
    }
    
#endif
    
    private void BindElementText(VisualElement localizationContainer, NodeSO nodeSo, Choice choice = null)
    {
        localizationContainer.Unbind();
        localizationContainer.Clear();
        
#if LOCALIZATION_EXIST
        string tableGuid = choice != null ? choice.guid : nodeSo.guid;
        StringTable table = null;
        string translation = "";
        if (DSData.instance.tableCollection && DSData.instance.database.defaultLocale)
        {
            table = DSData.instance.defaultStringTable;
            translation = LocalizationUtils.GetLocalizedString(table, tableGuid);
        }
#endif
        
        TextField translatedText = new TextField {multiline = true};
        Label toggleText = null;

        if (choice)
        {
            
            // Add Choice Requirements
            RequirementsListView requirementsListView = new RequirementsListView();
            requirementsListView.collapdseByDefault = true;
            requirementsListView.AddToClassList("condition-list");
            requirementsListView.SaveAsSubAssetOf(_currentDatabase);
            requirementsListView.PopulateView(choice.requirements);
            
            // Add Choice Actions
            CommandListView actionsListView = new CommandListView();
            actionsListView.collapdseByDefault = true;
            actionsListView.AddToClassList("condition-list");
            actionsListView.SaveAsSubAssetOf(_currentDatabase);
            actionsListView.PopulateView(choice.actionList);
            actionsListView.SetHeaderTitle("On Choice Selected");
            
            translatedText.style.marginRight = 3;
            translatedText.AddToClassList("choice");
            var foldOut = new Foldout{text= "-", value = false};
            foldOut.Add(translatedText);
            foldOut.Add(requirementsListView);
            foldOut.Add(actionsListView);
            localizationContainer.Add(foldOut);
            
            VisualElement toggleInputVe = foldOut.Q(className: "unity-foldout__input");
            VisualElement foldoutContentVe = foldOut.Q(className: "unity-foldout__content");
            foldoutContentVe.style.marginLeft = 0;
            toggleText = toggleInputVe.Q<Label>(className: "unity-foldout__text");
            toggleText.AddToClassList("toggle-foldout-text");
            toggleText.bindingPath = "choiceMessage";
            toggleText.Bind(new SerializedObject(choice));
        }
        else
        {
            ConversationNodeSO conversationNode = nodeSo as ConversationNodeSO;
            if(conversationNode != null)
            {
                // Add Choice Requirements
                RequirementsListView requirementsListView = new RequirementsListView();
                requirementsListView.collapdseByDefault = true;
                requirementsListView.AddToClassList("condition-list");
                requirementsListView.SaveAsSubAssetOf(_currentDatabase);
                requirementsListView.PopulateView(conversationNode.requirements);
                
                // Add Choice Actions
                CommandListView actionsListView = new CommandListView();
                actionsListView.collapdseByDefault = true;
                actionsListView.AddToClassList("condition-list");
                actionsListView.SaveAsSubAssetOf(_currentDatabase);
                actionsListView.PopulateView(conversationNode.actionList);
                actionsListView.SetHeaderTitle("On Complete");
            
                translatedText.style.marginRight = 0;
                translatedText.AddToClassList("field");
                localizationContainer.Add(translatedText);
                localizationContainer.Add(requirementsListView);
                localizationContainer.Add(actionsListView);
            }
        }
        
        if (choice == null)
        {
            translatedText.bindingPath = "message";
            translatedText.Bind(new SerializedObject(nodeSo));
        }
        else
        {
            translatedText.bindingPath = "choiceMessage";
            translatedText.Bind(new SerializedObject(choice));
        }
#if LOCALIZATION_EXIST
        if (table == null) return;
        translatedText.value = translation;
        EventCallback<FocusOutEvent> focusOutEvent = (e) =>
        {
            string valTrimmed = translatedText.value.Trim(' ');
            string previousString = LocalizationUtils.GetLocalizedString(table, tableGuid);
            if (valTrimmed != "" && valTrimmed != previousString)
            {
                table.AddEntry(tableGuid,valTrimmed);
                LocalizationUtils.RefreshStringTableCollection(_collection, table);
            }

            string finalTranslation = LocalizationUtils.GetLocalizedString(table, tableGuid);
            translatedText.value = finalTranslation;
        };
        translatedText.RegisterCallback(focusOutEvent);
        _translationMapText.Add(tableGuid,new KeyValuePair<Label, TextField>(toggleText,translatedText));
        _unregisterAll += () => translatedText.UnregisterCallback(focusOutEvent);
#endif
    }
    
    private void BindMessage(NodeSO nodeSo)
    {
        VisualElement localizationMessageVe = this.Q("message-container-text");
        BindElementText(localizationMessageVe,nodeSo);
    }
    
    private void BindDelayTime(ConversationNodeSO node){
        FloatField field = this.Q<FloatField>("time-float");
        field.bindingPath = "delayTime";
        field.Bind(new SerializedObject(node));
    }

    private void GetAndBindActor(Actor actor, Action<Actor> onSelectActor)
    {
        _actorSprite = this.Q<SpritePreviewElement>("actor-sprite");
        _actorName = this.Q<Label>("actor-name");
        _actorSprite.bindingPath = "actorImage";
        _actorName.bindingPath = "fullName";
            
        if (actor)
        {
            BindActor(actor);
        }
        else
        {
            _actorSprite.value = UIToolkitLoader.LoadSprite(DialogSystemEditor.RelativePath, "unknown-person.jpg");
            _actorName.text = "Unknown Dialog Actor";
        }
        _findActorButton.clickable = new Clickable(() =>
        {
            ActorsSearchProvider provider =
                ScriptableObject.CreateInstance<ActorsSearchProvider>();
            provider.SetUp(_actors,
                (actorSelected) =>
                {
                    BindActor(actorSelected);
                    _nodeView.UpdateActorToBind(actorSelected);
                    onSelectActor.Invoke(actorSelected);
                });
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                provider);
        });
    }
    
    private void HandleActorChanges(SerializedObject serializedObject)
    {
        _actorContainer.style.backgroundColor = _actor.bgColor;
    }

    public void ClearViewCallbacks()
    {
#if LOCALIZATION_EXIST
        _translationMapText.Clear();
#endif
        ClearAllValueChangedCallbacks();
    }

    private void BindActor(Actor selectedActor)
    {
        _actorContainer.Unbind();
        _actor = selectedActor;
        _actorSerialized = new SerializedObject(selectedActor);
        _actorContainer.TrackSerializedObjectValue(_actorSerialized, HandleActorChanges);
        _actorSprite.Bind(_actorSerialized);
        _actorName.Bind(_actorSerialized);
        _actorContainer.style.backgroundColor = _actor.bgColor;
    }
    
    private void SetUpDialogClasses()
    {
        _findActorButton = this.Q<Button>("find-actor-button");
        _findActorButton.Add(new Image {
            image = EditorGUIUtility.IconContent("d_pick_uielements").image
        });
        _actorContainer = this.Q("actor-container");
        this.Q("notVisibleContainer").AddToClassList("hidden-class");
        this.Q("actorContainer").RemoveFromClassList("hidden-class");
        this.Q("messageContainer").RemoveFromClassList("hidden-class");
        this.Q("delayContainer").RemoveFromClassList("hidden-class");
    }
    
    private void SetUpChoiceClasses()
    {
        SetUpDialogClasses();
        this.Q("choicesContainer").RemoveFromClassList("hidden-class");
    }
    
    private void ClearAllValueChangedCallbacks()
    {
        _unregisterAll?.Invoke();
        _unregisterAll = null;
    }
}

