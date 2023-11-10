using System;
using System.Collections.Generic;
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
#if LOCALIZATION_EXIST
    private Dictionary<KeyValuePair<string,Locale>, KeyValuePair<Label, TextField>> _translationMapText = new();
    private StringTableCollection _collection;
    private Locale _defaultLocale;
#endif
    private Action _unregisterAll = null;
    
    public DialogInspectorView(NodeView nodeView,List<Actor> actors)
    {
        string uriFile = "Assets/dialog-system/Editor/Custom Views/Dialog Inspector View/DialogInspectorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);

        _currentDatabase = DSData.instance.database;
        _nodeView = nodeView;
        _node = nodeView.node;
        _actors = actors;
        _messageTextField = this.Q<TextField>("message-text-field");
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

    public void SoftUpdate(NodeView nodeView,List<Actor> actors)
    {
        _nodeView = nodeView;
        _node = nodeView.node;
        _actors = actors;
        
        if(_node is DialogNode dialogNode)
        {
            BindActor(dialogNode.actor,(actor) =>
            {
                dialogNode.actor = actor;
                EditorUtility.SetDirty(dialogNode);
            });
        }else if (_node is ChoiceNode choiceNode)
        {
            BindActor(choiceNode.actor,(actor) =>
            {
                choiceNode.actor = actor;
                EditorUtility.SetDirty(choiceNode);
            });
        }
        
    }

    private void PopulateDialogInspector(DialogNode node)
    {
        if (node == null) return;
        SetUpDialogClasses();
        BindActor(node.actor, (actor) =>
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
        BindActor(node.actor,(actor) =>
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
        _choicesFoldoutVE = this.Q("choices-foldout");
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
        Choice choice = choiceNode.CreateChoice(_currentDatabase, defaultText);
        CreateNewOutputView(choiceNode, choice);
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
        ve.Add(deleteButton);

#if LOCALIZATION_EXIST
        VisualElement choiceLocalizationContainer = new VisualElement();
        
        Label choiceId = new Label(choice.guid);
        choiceId.AddToClassList("preview-choice-text");
        choiceLocalizationContainer.Add(choiceId);
        
        VisualElement choiceLocalization = new VisualElement();
        choiceLocalizationContainer.Add(choiceLocalization);
        
        BindLocalizationElement(choiceLocalization, choiceNode, choice);
        ve.Add(choiceLocalizationContainer);
#else
        TextField translatedText = new TextField {multiline = true};
        translatedText.style.marginRight = 0;
        translatedText.AddToClassList("field");
        translatedText.bindingPath = "choiceMessage";
        translatedText.Bind(new SerializedObject(choice));
        ve.Add(translatedText);
#endif
        
        _choicesFoldoutVE.Add(ve);
    }

    private void CheckOutputRemaining()
    {
        if (_choicesFoldoutVE.childCount == 1)
        {
            VisualElement choiceElement = _choicesFoldoutVE.ElementAt(0);
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
    
#if LOCALIZATION_EXIST
    private void BindLocalization(Node node)
    {
        if (!DSData.instance.database.tableCollection || !DSData.instance.database.defaultLocale) return;
        
        _collection = DSData.instance.database.tableCollection;
        _defaultLocale = DSData.instance.database.defaultLocale;
        _translationMapText.Clear();
        LocalizationEditorSettings.EditorEvents.TableEntryModified += TableEntryModified;
        LocalizationEditorSettings.EditorEvents.LocaleAdded += LocaleAddedRemoved;
        LocalizationEditorSettings.EditorEvents.LocaleRemoved += LocaleAddedRemoved;
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        
        VisualElement localizationMessageVe = this.Q("localization-message");
        BindLocalizationElement(localizationMessageVe,node);
        
        Label messageGuidLabel = this.Q<Label>("message-guid-label");
        messageGuidLabel.RemoveFromClassList("hidden-class");
        messageGuidLabel.text = node.guid;
    }

    private void BindLocalizationElement(VisualElement localizationContainer, Node node, Choice choice = null)
    {
        localizationContainer.Clear();
        string tableGuid = choice != null ? choice.guid : node.guid;
        
        int localeIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(_defaultLocale);
        List<Locale> locales = LocalizationSettings.AvailableLocales.Locales.MoveItemAtIndexToFront(localeIndex);
        foreach (var locale in locales)
        {
            StringTable table = LocalizationSettings.StringDatabase.GetTable(_collection.name,locale);
            string translation = LocalizationUtils.GetLocalizedString(table, tableGuid);

            bool isFoldOpened = _defaultLocale == locale && choice == null;
            var localeFoldout = new Foldout {text = locale.Identifier.CultureInfo.DisplayName, value = isFoldOpened};
            localeFoldout.style.marginBottom = 5;
            
            TextField translatedText = new TextField {multiline = true};
            translatedText.style.marginRight = choice == null?0:7;
            translatedText.AddToClassList("field");
            
            if (_defaultLocale == locale)
            {
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
            }
            
            localeFoldout.Add(translatedText);
            localizationContainer.Add(localeFoldout);
            
            VisualElement toggleInputVe = localeFoldout.Q(className: "unity-foldout__input");
            VisualElement foldoutContentVe = localeFoldout.Q(className: "unity-foldout__content");
            foldoutContentVe.style.marginLeft = 0;
            Label toggleText = toggleInputVe.Q<Label>(className: "unity-foldout__text");
            toggleText.AddToClassList("toggle-foldout-text");
                
            Label translationString = new Label(translation);
            translationString.AddToClassList("preview-text");
            toggleInputVe.Add(translationString);
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
                translationString.text = finalTranslation;
            };
            translatedText.RegisterCallback(focusOutEvent);
            _translationMapText.Add(new KeyValuePair<string, Locale>(tableGuid,locale),new KeyValuePair<Label, TextField>(translationString,translatedText));
            _unregisterAll += () => translatedText.UnregisterCallback(focusOutEvent);
        }
    }

    private void TableEntryModified(SharedTableData.SharedTableEntry tableEntry)
    {
        if (!_node.guid.Equals(tableEntry.Key) && 
            (_choiceNode && _choiceNode.choices.FindIndex(c => c.guid.Equals(tableEntry.Key)) == -1))  return;
        foreach(KeyValuePair<KeyValuePair<string, Locale>,KeyValuePair<Label,TextField>> entry in _translationMapText)
        {
            if(!entry.Key.Key.Equals(tableEntry.Key)) continue;
            StringTable table = LocalizationSettings.StringDatabase.GetTable(_collection.name,entry.Key.Value);
            string translation = LocalizationUtils.GetLocalizedString(table, entry.Key.Key);
            
            Label currentLabel = entry.Value.Key;
            if (currentLabel != null)
            {
                currentLabel.text = translation;
            }
            TextField currentTexField = entry.Value.Value;
            currentTexField.value = translation;
        }
    }
    
    private void LocaleAddedRemoved(Locale locale)
    {
        ClearAllValueChangedCallbacks();
        BindLocalization(_node);
    }
#endif

    private void BindMessage(Node node)
    {
        _messageTextField.RemoveFromClassList("hidden-class");
        _messageTextField.bindingPath = "message";
        _messageTextField.Bind(new SerializedObject(node));
    }

    private void BindActor(Actor actor, Action<Actor> onSelectActor)
    {
        Button findActorButton = this.Q<Button>("find-actor-button");
        SpritePreviewElement actorSprite = this.Q<SpritePreviewElement>("actor-sprite");
        Label actorName = this.Q<Label>("actor-name");
        actorSprite.bindingPath = "actorImage";
        actorName.bindingPath = "fullName";
            
        if (actor)
        {
            actorSprite.Bind(new SerializedObject(actor));
            actorName.Bind(new SerializedObject(actor));
        }
        else
        {
            actorSprite.value = Resources.Load<Sprite>( "unknown-person" );
            actorName.text = "Unknown Dialog Actor";
        }
        findActorButton.clickable = new Clickable(() =>
        {
            ActorsSearchProvider provider =
                ScriptableObject.CreateInstance<ActorsSearchProvider>();
            provider.SetUp(_actors,
                (actorSelected) =>
                {
                    actorSprite.Bind(new SerializedObject(actorSelected));
                    actorName.Bind(new SerializedObject(actorSelected));
                    _nodeView.UpdateActorToBind(actorSelected);
                    onSelectActor.Invoke(actorSelected);
                });
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                provider);
        });
    }

    public void ClearViewCallbacks()
    {
#if LOCALIZATION_EXIST
        LocalizationEditorSettings.EditorEvents.TableEntryModified -= TableEntryModified;
        LocalizationEditorSettings.EditorEvents.LocaleAdded -= LocaleAddedRemoved;
        LocalizationEditorSettings.EditorEvents.LocaleRemoved -= LocaleAddedRemoved;
        _translationMapText.Clear();
#endif
        ClearAllValueChangedCallbacks();
    }
    
    private void SetUpDialogClasses()
    {
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
