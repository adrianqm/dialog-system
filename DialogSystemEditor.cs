
using System;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogSystemEditor : EditorWindow
{
    private DialogSystemView _treeView;
    private InspectorView _inspectorView;
    private ActorsListView _actorsListView;
    private Label _conversationNameLabel;
    private TabbedMenuController _tabbedMenuController;
    private Button _createNewActorButton;
    private Action _unregisterAll;
    private DialogSystemDatabase _currentDatabase;
    
    [SerializeField]
    private VisualTreeAsset visualTreeAsset = default;
    
    [SerializeField]
    private StyleSheet styleSheet = default;

    [MenuItem("AQM Tools/Dialog System Editor ...")]
    public static void OpenWindow()
    {
        DialogSystemEditor wnd = GetWindow<DialogSystemEditor>();
        wnd.titleContent = new GUIContent("Dialog System Editor");
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        if (Selection.activeObject is ConversationTree)
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
        
        // Instanciate Tab Controller
        _tabbedMenuController = new TabbedMenuController(root);
        _tabbedMenuController.RegisterTabCallbacks();
        
        _treeView = root.Q<DialogSystemView>();
        _inspectorView = root.Q<InspectorView>();
        _actorsListView = root.Q<ActorsListView>();
        _createNewActorButton = root.Q<Button>(className: "createNewActorButton");
        _createNewActorButton.clicked += OnCreateNewActor;
        VisualElement scrollContainer = _actorsListView.Q("unity-content-and-vertical-scroll-container");
        _actorsListView.SetUpScrollContainerManipulator(scrollContainer);
        ToolbarSearchField searchField = root.Q<ToolbarSearchField>(className: "actorsSearchFilter");
        _actorsListView.SetUpSearchFieldFilterCallback(searchField);
        
        _treeView.OnNodeSelected = OnNodeSelectionChanged;
        
        // To remove Hover style
        _treeView.RegisterCallback<ClickEvent>((evt) =>
        {
            if (_conversationNameLabel == null) return;
            _conversationNameLabel.RemoveFromClassList("conversation-name-label--selected");
            _conversationNameLabel.AddToClassList("conversation-name-label");
        });
        
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
        if(_createNewActorButton != null) _createNewActorButton.clicked -= OnCreateNewActor;
    }

    private void OnCreateNewActor()
    {
        _actorsListView.CreateNewActor();
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
        ConversationTree tree;
        if (database)
        {
            tree = database.conversations[0];
            _currentDatabase = database;
        }
        else tree = Selection.activeObject as ConversationTree;
        
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
        }

        if (Application.isPlaying)
        {
            if (tree) SetTree(tree);
        }
        else if (tree && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
        {
           SetTree(tree);
        }
        
    }

    private void SetTree(ConversationTree tree)
    {
        // Set root element
        SerializedObject so = new SerializedObject(tree.database);
        rootVisualElement.Bind(so);
        
        // Show conversation inspector
        _inspectorView.ShowConversationInspector(tree);
        
        // Register Conversation Name Label Query
        _conversationNameLabel = rootVisualElement.Q<Label>(className: "conversation-name-label");
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
                _treeView.ClearSelection();
                _inspectorView.ShowConversationInspector(tree);
            };
            
            _conversationNameLabel.RegisterCallback(clickEvent);
            _unregisterAll += () => _conversationNameLabel.UnregisterCallback(clickEvent);
        }
        
        // Set Actors data
        _actorsListView.SetupTable(tree.database.actorsTree);
        
        _treeView?.PopulateView(tree);
    }

    void OnNodeSelectionChanged(NodeView node)
    {
        _inspectorView.ShowDialogInspector(node,_currentDatabase.actorsTree);
    }

    private void OnInspectorUpdate()
    {
        _treeView?.UpdateNodeStates();
    }
    
    private void ClearAllValueCallbacks()
    {
        _unregisterAll?.Invoke();
        _unregisterAll = null;
    }
}
