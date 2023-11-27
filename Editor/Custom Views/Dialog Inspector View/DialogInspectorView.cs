using System;
using System.Collections.Generic;
using AQM.Tools;
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
using Node = AQM.Tools.Node;

public class DialogInspectorView : VisualElement
{
    private DialogSystemDatabase _currentDatabase;
    private NodeView _nodeView;
    private Node _node;
    private DialogNode _dialogNode;
    private ChoiceNode _choiceNode;
    private List<Actor> _actors;
    private readonly TextField _messageTextField;
    private VisualElement _choicesAddVE;
    private VisualElement _choicesFoldoutVE;
    private Button _disabledChoiceButton;
    private Button _findActorButton;
    private VisualElement _actorContainer;
    private Actor _actor;
#if LOCALIZATION_EXIST
    private Dictionary<string, KeyValuePair<Label,TextField>> _translationMapText = new();
    private StringTableCollection _collection;
    private Locale _defaultLocale;
    private SpritePreviewElement _actorSprite;
    private Label _actorName;
    private SerializedObject _actorSerialized;
#endif
    private Dictionary<string, VisualElement> _choicesMap = new();
    private Action _unregisterAll = null;
    
    public DialogInspectorView(NodeView nodeView)
    {
        string uriFile = "Assets/dialog-system/Editor/Custom Views/Dialog Inspector View/DialogInspectorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);

        _currentDatabase = DSData.instance.database;
        _nodeView = nodeView;
        _node = nodeView.node;
        _actors = DSData.instance.database.actors;
        _messageTextField = this.Q<TextField>("message-text-field");
        SetUp();
    }

    void SetUp()
    {
        _choicesMap = new();
        if (_node is not StartNode && _node is not CompleteNode)
        {
            if(_node is DialogNode)
            {
                _dialogNode = _node as DialogNode;
                PopulateDialogInspector(_dialogNode);
            }else if (_node is ChoiceNode)
            {
                _choiceNode = _node as ChoiceNode;
                PopulateChoiceInspector(_choiceNode);
            }
        }
    }

    private void PopulateDialogInspector(DialogNode node)
    {
        if (node == null) return;
        SetUpDialogClasses();
        GetAndBindActor(node.actor, (actor) =>
        {
            node.actor = actor;
            EditorUtility.SetDirty(node);
        });
#if LOCALIZATION_EXIST
        BindLocalization(node);
#else
        BindMessage(node);
#endif
    }

    private void PopulateChoiceInspector(ChoiceNode node)
    {
        if (node == null) return;
        SetUpChoiceClasses();
        GetAndBindActor(node.actor,(actor) =>
        {
            node.actor = actor;
            EditorUtility.SetDirty(node);
        });
#if LOCALIZATION_EXIST
        BindLocalization(node);
#else
        BindMessage(node);
#endif
        BindChoices(node);
    }

    private void BindChoices(ChoiceNode node)
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
        
        // Register New Callback
        addButton.clickable = new Clickable(() =>
        {
            CreateNewChoice(node, addField.value);
            addField.value = "";
            addField.Focus();
            CheckOutputRemaining();
        });
    }

    private TextField AddChoiceTextField(ChoiceNode node)
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
    
    private Button AddChoiceButton(ChoiceNode node)
    {
        Button newChoiceButton = new Button();
        newChoiceButton.Add(new Image {
            image = EditorGUIUtility.IconContent("d_CreateAddNew").image
        });
        newChoiceButton.AddToClassList("choiceDataBtn");
        _choicesAddVE.Add(newChoiceButton);
        return newChoiceButton;
    }

    private void CreateNewChoice(ChoiceNode choiceNode, string defaultText)
    {
        Choice choice = ChoiceUtils.CreateChoice(_currentDatabase, choiceNode, defaultText);
        CreateNewOutputView(choiceNode, choice);
        _nodeView.OnChoiceAdded(choiceNode,choice);
    }

    private void RemoveOutputView(VisualElement ve, Choice choice)
    {
        _translationMapText.Remove(choice.guid);
        _choicesMap.Remove(choice.guid);
        _choicesFoldoutVE.Remove(ve);
        CheckOutputRemaining();
    }

    private void CreateNewOutputView(ChoiceNode choiceNode, Choice choice)
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
            ChoiceUtils.DeleteChoice(choiceNode,choice);
            RemoveOutputView(ve, choice);
        });
        ve.Add(deleteButton);

#if LOCALIZATION_EXIST
        VisualElement choiceLocalizationContainer = new VisualElement();
        
        VisualElement choiceLocalization = new VisualElement();
        choiceLocalizationContainer.Add(choiceLocalization);
        
        BindLocalizationElement(choiceLocalization, choiceNode, choice);
        ve.Add(choiceLocalizationContainer);
        
        VisualElement idVe = new VisualElement();
        idVe.AddToClassList("message-guid-choice-container");
        Label labelId = new Label("ID");
        labelId.AddToClassList("message-guid-id");
        idVe.Add(labelId);
            
        VisualElement copyButtonVe = new VisualElement();
        copyButtonVe.AddToClassList("message-guid-copy-container");
        Button copyButton = new Button();
        copyButton.Add(new Image {
            image = EditorGUIUtility.IconContent("d_winbtn_win_restore_a").image
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
#else
        TextField translatedText = new TextField {multiline = true};
        translatedText.style.marginRight = 0;
        translatedText.AddToClassList("field");
        translatedText.bindingPath = "choiceMessage";
        translatedText.Bind(new SerializedObject(choice));
        ve.Add(translatedText);
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
    private void BindLocalization(Node node)
    {
        if (!DSData.instance.tableCollection || !DSData.instance.database.defaultLocale) return;
        
        _collection = DSData.instance.tableCollection;
        _defaultLocale = DSData.instance.database.defaultLocale;
        _translationMapText.Clear();
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        
        VisualElement localizationMessageVe = this.Q("localization-message");
        BindLocalizationElement(localizationMessageVe,node);
        
        VisualElement copyVe = this.Q("message-guid-copy-container");
        copyVe.Clear();
        Button copyButton = new Button();
        copyButton.Add(new Image {
            image = EditorGUIUtility.IconContent("d_winbtn_win_restore_a").image
        });
        copyButton.AddToClassList("copyBtn");
        copyButton.clickable = new Clickable(() =>
        {
            TextEditor te = new TextEditor();
            te.text = node.guid;
            te.SelectAll();
            te.Copy();
        });
        copyVe.Add(copyButton);
    }

    private void BindLocalizationElement(VisualElement localizationContainer, Node node, Choice choice = null)
    {
        localizationContainer.Unbind();
        localizationContainer.Clear();
        string tableGuid = choice != null ? choice.guid : node.guid;
        StringTable table = DSData.instance.defaultStringTable;
        string translation = LocalizationUtils.GetLocalizedString(table, tableGuid);
        
        TextField translatedText = new TextField {multiline = true};
        Label toggleText = null;

        if (choice)
        {
            translatedText.style.marginRight = 3;
            translatedText.AddToClassList("choice");
            var foldOut = new Foldout{text= "-", value = false};
            foldOut.Add(translatedText);
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
            translatedText.style.marginRight = 0;
            translatedText.AddToClassList("field");
            localizationContainer.Add(translatedText);
        }
        
        if (choice == null)
        {
            translatedText.bindingPath = "message";
            translatedText.Bind(new SerializedObject(node));
        }
        else
        {
            translatedText.bindingPath = "choiceMessage";
            translatedText.Bind(new SerializedObject(choice));
        }
        
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
    }
#endif

    private void BindMessage(Node node)
    {
        _messageTextField.RemoveFromClassList("hidden-class");
        _messageTextField.bindingPath = "message";
        _messageTextField.Bind(new SerializedObject(node));
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
            _actorSprite.value = Resources.Load<Sprite>( "unknown-person" );
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

