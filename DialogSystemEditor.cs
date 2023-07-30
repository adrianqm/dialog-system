
using System;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogSystemEditor : EditorWindow
{
    private ToolbarMenu _fileMenu;
    private DialogSystemView _treeView;
    private InspectorView _inspectorView;
    private ActorMultiColumListView _actorMultiColumListView;
    private VisualElement _toolbarHeaderVE;
    private ToolbarHeaderView _toolbarHeader;
    private VisualElement _dialogSelectorVE;
    private DatabaseSelectorView _dialogSelector;
    private VisualElement _dialogEditorVE;
    private VisualElement _dialogCreatorVE;
    private DatabaseCreatorView _dialogCreatorView;
    private VisualElement _confirmationModalVE;
    private ConfirmationModalView _confirmationModal;
    private DatabaseEditorView _dialogEditor;
    private const string hiddenContentClassName = "hiddenContent";
    private Label _conversationNameLabel;
    private TabbedMenuController _tabbedMenuController;
    private Button _createNewActorButton;
    private Action _unregisterAll;
    private DialogSystemDatabase _currentDatabase;
    private ConversationTree _currentTree;

    [SerializeField] private VisualTreeAsset visualTreeAsset = default;

    [SerializeField] private StyleSheet styleSheet = default;

    [MenuItem("AQM Tools/Dialog System Editor ...")]
    public static void OpenWindow()
    {
        DialogSystemEditor wnd = GetWindow<DialogSystemEditor>();
        wnd.titleContent = new GUIContent("Dialog System Editor");
        wnd.minSize = new Vector2(500,400);
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        if (Selection.activeObject is DialogSystemDatabase)
        {
            OpenWindow();
            return true;
        }

        return false;
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        visualTreeAsset.CloneTree(root);

        // Import StyleSheets 
        root.styleSheets.Add(styleSheet);
        
        // File Menu
        _fileMenu = root.Q<ToolbarMenu>("fileMenu");
        _fileMenu.menu.AppendAction("Import New Database...",
            a => { OnImportDatabase(); }, 
            a => DropdownMenuAction.Status.Normal);
        _fileMenu.menu.AppendAction("Edit Current Database...",
            a => { OnEditDatabase(); }, 
            a => DropdownMenuAction.Status.Normal);
        
        // Instantiate Tab Controller
        _tabbedMenuController = new TabbedMenuController(root);
        _tabbedMenuController.RegisterTabCallbacks();
        
        // Tree view
        _treeView = root.Q<DialogSystemView>();
        _treeView.OnNodeSelected = OnNodeSelectionChanged;
        _treeView.onNodesRemoved = OnNodesRemoved;
        // To remove Hover style
        _treeView.RegisterCallback<ClickEvent>((evt) =>
        {
            if (_conversationNameLabel == null) return;
            _conversationNameLabel.RemoveFromClassList("conversation-name-label--selected");
            _conversationNameLabel.AddToClassList("conversation-name-label");
        });
        
        // Inspector view
        _inspectorView = root.Q<InspectorView>();
        
        // Actor view
        _actorMultiColumListView = root.Q<ActorMultiColumListView>();
        _createNewActorButton = root.Q<Button>(className: "createNewActorButton");
        _createNewActorButton.clicked += OnCreateNewActor;
        VisualElement scrollContainer = _actorMultiColumListView.Q("unity-content-and-vertical-scroll-container");
        _actorMultiColumListView.SetUpScrollContainerManipulator(scrollContainer);
        ToolbarSearchField searchField = root.Q<ToolbarSearchField>(className: "actorsSearchFilter");
        _actorMultiColumListView.SetUpSearchFieldFilterCallback(searchField);
        _actorMultiColumListView.onActorsRemoved = OnActorsRemoved;

        // DB selector
        _dialogSelectorVE = root.Q<VisualElement>("database-selector");
        _dialogSelector = _dialogSelectorVE.Q<DatabaseSelectorView>();
        _dialogSelector.OnDatabaseSelected = OnManualSet;
        _dialogSelector.OnCreateNewDatabaseClicked = OnCreateNewDatabaseClicked;
        
        // DB editor
        _dialogEditorVE = root.Q<VisualElement>("database-editor");
        _dialogEditor = _dialogEditorVE.Q<DatabaseEditorView>();
        _dialogEditor.OnCloseModal = OnCloseEditDatabase;
        
        // DB creator
        _dialogCreatorVE = root.Q<VisualElement>("database-creator");
        _dialogCreatorView = _dialogCreatorVE.Q<DatabaseCreatorView>();
        _dialogCreatorView.OnCloseModal = OnCloseCreatorDatabase;
        _dialogCreatorView.OnCreatedDatabase = OnCreatedDatabase;
        
        // Confirmation Modal
        _confirmationModalVE = root.Q<VisualElement>("confirmation-modal");
        _confirmationModal = _confirmationModalVE.Q<ConfirmationModalView>();
        _confirmationModal.OnCloseModal = OnConfirmationModalClose;
        
        //Toolbar DB Selector
        _toolbarHeaderVE = root.Q<VisualElement>("toolbar-header");
        _toolbarHeader = _toolbarHeaderVE.Q<ToolbarHeaderView>();
        _toolbarHeader.OnDatabaseSelected = OnManualSet;
        _toolbarHeader.OnCreateDatabase = OnCreateDatabase;
        _toolbarHeader.OnEditDatabase = OnEditDatabase;
        _toolbarHeader.OnRemoveDatabase = OnRemoveDatabase;

        // Register Conversation Name Label Query
        _conversationNameLabel = root.Q<Label>(className: "conversation-name-label");

        OnSelectionChange();
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        if (_createNewActorButton != null) _createNewActorButton.clicked -= OnCreateNewActor;
    }

    private void OnCreateNewActor()
    {
        _actorMultiColumListView.CreateNewActor();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange obj)
    {
        switch (obj)
        {
            case PlayModeStateChange.EnteredEditMode:
                OnSelectionChange();
                break;
            case PlayModeStateChange.EnteredPlayMode:
                OnSelectionChange();
                break;
        }
    }

    private void OnSelectionChange()
    {
        DialogSystemDatabase database = Selection.activeObject as DialogSystemDatabase;
        ConversationTree tree = null;
        if (database)
        {
            _dialogSelectorVE.AddToClassList(hiddenContentClassName);
            _toolbarHeaderVE.RemoveFromClassList(hiddenContentClassName);

            database.RegisterUndoOperation(_actorMultiColumListView,_treeView);
            tree = database.conversations[0];
            _currentDatabase = database;
            _toolbarHeader.SetUpSelector(_currentDatabase);
            _dialogEditor.SetUpEditor(_currentDatabase);
            
            // Set root element
            SerializedObject so = new SerializedObject(_currentDatabase);
            rootVisualElement.Bind(so);
        }
        else if(_currentDatabase != null)
        {
            tree = _currentDatabase.conversations[0];
            _toolbarHeaderVE.RemoveFromClassList(hiddenContentClassName);
            _toolbarHeader.SetUpSelector(_currentDatabase);
            _dialogEditor.SetUpEditor(_currentDatabase);
        }
        else
        {
            OpenDatabaseSelector();
            ClearView();
        }
        
        /*
        if (!tree)
        {
            if (Selection.activeGameObject)
            {
                ConversationRunner runner = Selection.activeGameObject.GetComponent<ConversationRunner>();
                if (runner)
                {
                    tree = runner.conversationTree;
                }
            }
        }*/

        if (Application.isPlaying)
        {
            if (tree) SetTree(tree);
        }
        else if (tree && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
        {
            SetTree(tree);
        }

    }

    private void OnManualSet(DialogSystemDatabase database)
    {
        ConversationTree tree;
        _dialogSelectorVE.AddToClassList(hiddenContentClassName);
        _toolbarHeaderVE.RemoveFromClassList(hiddenContentClassName);
        
        database.RegisterUndoOperation(_actorMultiColumListView,_treeView);
        tree = database.conversations[0];
        _currentDatabase = database;
        _toolbarHeader.SetUpSelector(_currentDatabase);
        _dialogEditor.SetUpEditor(_currentDatabase);
        
        // Set root element
        SerializedObject so = new SerializedObject(_currentDatabase);
        rootVisualElement.Bind(so);
        SetTree(tree);
    }

    private void SetTree(ConversationTree tree)
    {
        _currentTree = tree;
        
        // Show conversation inspector
        _inspectorView.ShowConversationInspector(tree);
        
        if (_conversationNameLabel == null)
        {
            _conversationNameLabel = rootVisualElement.Q<Label>(className: "conversation-name-label--selected");
        }

        if (_conversationNameLabel != null)
        {
            ClearAllValueCallbacks();
            _conversationNameLabel.style.display = DisplayStyle.Flex;
            _conversationNameLabel.bindingPath = "title";
            _conversationNameLabel.Bind(new SerializedObject(tree));
            EventCallback<ClickEvent> clickEvent = (e) =>
            {
                _conversationNameLabel.AddToClassList("conversation-name-label--selected");
                _conversationNameLabel.RemoveFromClassList("conversation-name-label");
                SetConversationNameSelected();
            };

            _conversationNameLabel.RegisterCallback(clickEvent);
            _unregisterAll += () => _conversationNameLabel.UnregisterCallback(clickEvent);
        }

        // Set Actors data
        _actorMultiColumListView.SetupTable(_currentDatabase.actorsTree);

        _treeView?.PopulateView(tree);
    }

    void OnNodeSelectionChanged(NodeView node)
    {
        _inspectorView.ShowDialogInspector(node, _currentDatabase.actorsTree);
    }

    void OnNodesRemoved()
    {
        SetConversationNameSelected();
    }

    void OnActorsRemoved()
    {
        SetConversationNameSelected();
        _treeView?.PopulateView(_currentTree);
    }

    void OnImportDatabase()
    {
        OpenDatabaseSelector();
    }

    void OpenDatabaseSelector()
    {
        _dialogSelector.ClearDatabaseSelection();
        _dialogSelectorVE.RemoveFromClassList(hiddenContentClassName);
    }

    void OnCreateNewDatabaseClicked()
    {
        _dialogSelectorVE.AddToClassList(hiddenContentClassName);
        OnCreateDatabase();
    }

    void OnCreateDatabase()
    {
        _dialogCreatorView.ResetInputs();
        _dialogCreatorVE.RemoveFromClassList(hiddenContentClassName);
    }

    void OnEditDatabase()
    {
        _dialogEditorVE.RemoveFromClassList(hiddenContentClassName);
    }

    void OnRemoveDatabase()
    {
        _confirmationModal.UpdateModalText("Are you sure you want to remove the database?");
        _confirmationModal.OnConfirmModal = OnRemoveDatabaseConfirm;
        _confirmationModalVE.RemoveFromClassList(hiddenContentClassName);
    }
    
    void OnCloseEditDatabase()
    {
        _dialogEditorVE.AddToClassList(hiddenContentClassName);
    }

    void OnCloseCreatorDatabase()
    {
        _dialogCreatorVE.AddToClassList(hiddenContentClassName);
        if (_currentDatabase == null)
        {
            OpenDatabaseSelector();
        }
    }

    void OnCreatedDatabase(DialogSystemDatabase newDb)
    {
        _dialogCreatorVE.AddToClassList(hiddenContentClassName);
        OnManualSet(newDb);
    }

    void OnConfirmationModalClose()
    {
        _confirmationModalVE.AddToClassList(hiddenContentClassName);
    }
    
    void OnRemoveDatabaseConfirm()
    {
        _currentDatabase.Delete();
        _confirmationModalVE.AddToClassList(hiddenContentClassName);
        OpenDatabaseSelector();
        ClearView();
    }

    private void SetConversationNameSelected()
    {
        _treeView.ClearSelection();
        _inspectorView.ShowConversationInspector(_currentTree);
    }

    private void OnInspectorUpdate()
    {
        _treeView?.UpdateNodeStates();
    }

    private void ClearView()
    {
        _currentDatabase = null;
        _toolbarHeaderVE.AddToClassList(hiddenContentClassName);
        _actorMultiColumListView.ClearList();
        _inspectorView.ClearInspector();
        _treeView.ClearGraph();
        
        ClearAllValueCallbacks();
        _conversationNameLabel.RemoveFromClassList("conversation-name-label--selected");
        _conversationNameLabel.AddToClassList("conversation-name-label");
        _conversationNameLabel.style.display = DisplayStyle.None;
    }
    
    private void ClearAllValueCallbacks()
    {
        _unregisterAll?.Invoke();
        _unregisterAll = null;
    }
}
