using System;
using System.Collections.Generic;
using AQM.Tools;
using Codice.Client.BaseCommands.Import;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class DialogSystemEditor : EditorWindow
{
    private DialogSystemView _treeView;
    private InspectorView _inspectorView;
    private ActorsListView _actorsListView;
    private Label _conversationNameLabel;
    private TabbedMenuController _tabbedMenuController;
    private Button _createNewActorButton;
    
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
        _createNewActorButton.clicked -= OnCreateNewActor;
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
        if (database) tree = database.conversations[0];
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
        
        // Register Conversation Name Label Query
        _conversationNameLabel = rootVisualElement.Q<Label>(className: "conversation-name-label");
        if (_conversationNameLabel == null)
        {
            _conversationNameLabel = rootVisualElement.Q<Label>(className: "conversation-name-label--selected");
        } 
        _conversationNameLabel.style.display = DisplayStyle.Flex;
        _conversationNameLabel.bindingPath = "title";
        _conversationNameLabel.Bind(new SerializedObject(tree));
        _conversationNameLabel.RegisterCallback<ClickEvent>((evt) =>
        {
            _conversationNameLabel.AddToClassList("conversation-name-label--selected");
            _conversationNameLabel.RemoveFromClassList("conversation-name-label");
            _treeView.ClearSelection();
            _inspectorView.UpdateWithConversationName(tree);
        });
        
        // Set Actors data
        _actorsListView.SetupTable(tree.database.actorsTree);
        
        _treeView?.PopulateView(tree);
    }
    
    

    void OnNodeSelectionChanged(NodeView node)
    {
        _inspectorView.UpdateSelection(node);
    }

    private void OnInspectorUpdate()
    {
        _treeView?.UpdateNodeStates();
    }
}
