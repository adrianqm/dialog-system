
using System;
using AQM.Tools;
using Blackboard.Utils.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
#if LOCALIZATION_EXIST
using UnityEditor.Localization;
using UnityEditor.Localization.UI;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
#endif
using UnityEngine.UIElements;

    public class DialogSystemEditor : EditorWindow
    {
        public static string RelativePath => AssetDataBaseExtensions.GetDirectoryOfScript<DialogSystemEditor>();
        private readonly string assetName = "DialogSystemEditor";
        
        private ToolbarMenu _fileMenu;
        private DialogSystemView _treeView;
        private InspectorView _inspectorView;
        private ActorMultiColumListView _actorMultiColumListView;
        private ConversationMultiColumListView _conversationMultiColumListView;
        private ConversationsView _conversationEditView;
        private ConversationListView _conversationListView;
        private ConversationSelectorView _conversationGroupSelector;
        private VisualElement _toolbarHeaderVE;
        private VisualElement _localicationInfoVE;
        private Label _localeLabel;
        private Label _collectionName;
        private Toggle _localActivated;
        private ToolbarHeaderView _toolbarHeader;
        private VisualElement _dialogSelectorVE;
        private DatabaseSelectorView _dialogSelector;
        private VisualElement _dialogEditorVE;
        private VisualElement _dialogLocalizationVE;
        private VisualElement _dialogCreatorVE;
        private DatabaseCreatorView _dialogCreatorView;
        private VisualElement _confirmationModalVE;
        private ConfirmationModalView _confirmationModal;
        private DatabaseEditorView _dialogEditor;
        private DatabaseLocalizationView _dialogLocalization;
        private const string hiddenContentClassName = "hiddenContent";
        
        private VisualElement _topConversationBar;
        private TabbedMenuController _tabbedMenuController;
        private Button _createNewActorButton;
        private Action _unregisterAll;
        private DialogSystemDatabase _currentDatabase;

        [SerializeField] private VisualTreeAsset visualTreeAsset = default;

        [SerializeField] private StyleSheet styleSheet = default;

        [MenuItem("Tools/Dialog Designer Editor")]
        public static void OpenWindow()
        {
            DialogSystemEditor wnd = GetWindow<DialogSystemEditor>();
            wnd.titleContent = new GUIContent("Dialog Designer");
            wnd.minSize = new Vector2(600,400);
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
            visualTreeAsset = UIToolkitLoader.LoadUXML(RelativePath, assetName);
            visualTreeAsset.CloneTree(root);

            // Import StyleSheets
            styleSheet = UIToolkitLoader.LoadStyleSheet(RelativePath, assetName);
            root.styleSheets.Add(styleSheet);
            
            // Instantiate Tab Controller
            _tabbedMenuController = new TabbedMenuController(root);
            _tabbedMenuController.RegisterTabCallbacks();
            
            // Tree view
            _treeView = root.Q<DialogSystemView>();
            _treeView.SetUpEditorWindow(this);
            _treeView.onNodeSelected = OnNodeSelectionChanged;
            _treeView.onRefreshInspector = OnRefreshInspector;
            _treeView.onNodesRemoved = OnNodesRemoved;
            
            // Inspector view
            _inspectorView = root.Q<InspectorView>();
            
            // Actor view
            _actorMultiColumListView = root.Q<ActorMultiColumListView>();
            VisualElement scrollContainer = _actorMultiColumListView.Q("unity-content-and-vertical-scroll-container");
            _actorMultiColumListView.SetUpScrollContainerManipulator(scrollContainer);
            ToolbarSearchField searchField = root.Q<ToolbarSearchField>("actorsSearchFilter");
            _actorMultiColumListView.SetUpSearchFieldFilterCallback(searchField);
            _actorMultiColumListView.onActorsRemoved = OnActorsRemoved;
            
            // Conversations List view
            _conversationMultiColumListView = root.Q<ConversationMultiColumListView>();
            VisualElement conversationScrollContainer = _conversationMultiColumListView.Q("unity-content-and-vertical-scroll-container");
            _conversationMultiColumListView.SetUpScrollContainerManipulator(conversationScrollContainer);
            ToolbarSearchField conversationsSearchField = root.Q<ToolbarSearchField>("conversationsSearchFilter");
            _conversationMultiColumListView.SetUpSearchFieldFilterCallback(conversationsSearchField);
            _conversationMultiColumListView.onEditConversation = OnEditConversation;
            
            // Conversations GraphView
            _conversationListView = root.Q<ConversationListView>();
            _conversationEditView = root.Q<ConversationsView>();
            _conversationGroupSelector = root.Q<ConversationSelectorView>();
            _conversationGroupSelector.onConversationGroupSelected = OnUpdateConversationTable;
            _conversationEditView.SetUpBackButton(OnBackToConversationsList);

            // DB selector
            _dialogSelectorVE = root.Q<VisualElement>("database-selector");
            _dialogSelector = _dialogSelectorVE.Q<DatabaseSelectorView>();
            _dialogSelector.OnDatabaseSelected = OnManualSet;
            _dialogSelector.OnCreateNewDatabaseClicked = OnCreateNewDatabaseClicked;
            
            // DB editor
            _dialogEditorVE = root.Q<VisualElement>("database-editor");
            _dialogEditor = _dialogEditorVE.Q<DatabaseEditorView>();
            _dialogEditor.OnCloseModal = OnCloseEditDatabase;
            
            // DB ChangeLocale
            _dialogLocalizationVE = root.Q<VisualElement>("database-localization");
            _dialogLocalization = _dialogLocalizationVE.Q<DatabaseLocalizationView>();
            _dialogLocalization.onCloseModal = OnCloseLocalizeDatabase;
            _dialogLocalization.onLocalizeCreated = OnLocalizeDatabaseCreated;
            
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
            _toolbarHeader.onDatabaseSelected = OnManualSet;
            _toolbarHeader.onCreateDatabase = OnCreateDatabase;
            _toolbarHeader.onEditDatabase = OnEditDatabase;
            _toolbarHeader.onLocalizeDatabase = OnLocalizeDatabase;
            _toolbarHeader.onRemoveDatabase = OnRemoveDatabase;
            
            //ChangeLocale Info
            _localicationInfoVE = root.Q<VisualElement>("localization-info");
            _localeLabel = root.Q<Label>("locale-desc");
            _collectionName = root.Q<Label>("string-table-desc");
            _localActivated = root.Q<Toggle>("local-activated");
    #if LOCALIZATION_EXIST
            Button openLocalizationButton = root.Q<Button>("open-string-collection");
            openLocalizationButton.Add(new Image {
                image = EditorGUIUtility.IconContent("d_ListView On").image
            });
            openLocalizationButton.clickable = new Clickable(() =>
            {
                if(!DSData.instance.tableCollection) return;
                LocalizationTablesWindow.ShowWindow(DSData.instance.tableCollection);
            });
    #endif
            SetDefaultIconForDatabase();
            SetDefaultDatabase();
        }

        private void SetDefaultIconForDatabase()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(DialogSystemDatabase)}");
            Texture2D icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/dialog-system/Assets/Icon.png", typeof(Texture2D));
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                DialogSystemDatabase db = AssetDatabase.LoadAssetAtPath<DialogSystemDatabase>(assetPath);
                EditorGUIUtility.SetIconForObject(db, icon); 
                AssetDatabase.ImportAsset(assetPath);
            }
            AssetDatabase.Refresh();
        }

        private void SetDefaultDatabase()
        {
            if(DSData.instance.database != null)
            {
                ClearView();
                OnBackToConversationsList();
                _currentDatabase = DSData.instance.database;
                _toolbarHeaderVE.RemoveFromClassList(hiddenContentClassName);
                SetUpComponentsDatabase(_currentDatabase);
            }
            else
            {
                OpenDatabaseSelector();
                ClearView();
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    //OnSelectionChange();
                    break;
                case PlayModeStateChange.EnteredPlayMode: break;
            }
        }

        private void OnSelectionChange()
        {
            if (Application.isPlaying && _currentDatabase != null)
            {
                if (Selection.activeGameObject)
                {
                    ConversationRunner runner = Selection.activeGameObject.GetComponent<ConversationRunner>();
                    if (runner != null)
                    {
                        OnEditConversation(runner.conversation);
                    }
                }
            }else if (!Application.isPlaying)
            {
                if(DSData.instance.database == null)
                {
                    OpenDatabaseSelector();
                    ClearView();
                }else
                {
                    DialogSystemDatabase database = Selection.activeObject as DialogSystemDatabase;
                    if(!database) return;
                    
                    _dialogSelectorVE.AddToClassList(hiddenContentClassName);
                    _toolbarHeaderVE.RemoveFromClassList(hiddenContentClassName);
                    OnBackToConversationsList();
                
                    // Set root element
                    _currentDatabase = database;
                    DSData.instance.database = database;
                    SerializedObject so = new SerializedObject(_currentDatabase);
                    rootVisualElement.Bind(so);
                    SetUpComponentsDatabase(_currentDatabase);
                }
            }
        }

        private void SetUpComponentsDatabase(DialogSystemDatabase database)
        {
            SetUpLocalizationInfo(database);
            _toolbarHeader.SetUpSelector(database);
            _dialogEditor.SetUpEditor(database);
            _treeView.SetUpTreeView(database);
            _actorMultiColumListView.SetupTable(database);
            _conversationGroupSelector.SetupTree(database);
        }

        private void OnManualSet(DialogSystemDatabase database)
        {
            _dialogSelectorVE.AddToClassList(hiddenContentClassName);
            _toolbarHeaderVE.RemoveFromClassList(hiddenContentClassName);
            OnBackToConversationsList();
            
            // Set root element
            _currentDatabase = database;
            DSData.instance.database = database;
            SerializedObject so = new SerializedObject(_currentDatabase);
            rootVisualElement.Bind(so);
            SetUpComponentsDatabase(_currentDatabase);
        }

        private void SetUpLocalizationInfo(DialogSystemDatabase database)
        {
#if LOCALIZATION_EXIST
                _dialogLocalization.SetUpDefaultSelectorValues();
                var collection = LocalizationEditorSettings.GetStringTableCollection(database.tableCollectionName);
                if (collection && database.defaultLocale)
                {
                    DSData.instance.tableCollection = collection;
                    StringTable table = LocalizationSettings.StringDatabase.GetTable(collection.name,database.defaultLocale);
                    DSData.instance.defaultStringTable = table;

                    _localicationInfoVE.RemoveFromClassList(hiddenContentClassName);
                    _localeLabel.text = database.defaultLocale.Identifier.CultureInfo.DisplayName;
                    SerializedObject so = new SerializedObject(collection);
                    SerializedProperty property = so.FindProperty("m_Name");
                    _collectionName.BindProperty(property);
                    _localActivated.bindingPath = "localizationActivated";
                    _collectionName.RegisterValueChangedCallback(HandleCollectionNameChange);
                    _localActivated.Bind(new SerializedObject(database));
                    _localActivated.RegisterValueChangedCallback(HandleLocalActivatedCallback);
                }
                else
                {
                    DSData.instance.tableCollection = null;
                    DSData.instance.defaultStringTable = null;
                    _localicationInfoVE.AddToClassList(hiddenContentClassName);
                    _localeLabel.text = "not defined";
                    _collectionName.Unbind();
                    _collectionName.RegisterValueChangedCallback(HandleCollectionNameChange);
                    _localActivated.Unbind();
                    _localActivated.UnregisterValueChangedCallback(HandleLocalActivatedCallback);
                }
#endif
        }
        
        private void HandleLocalActivatedCallback(ChangeEvent<bool> evt)
        {
            if (!evt.newValue)
            {
                _collectionName.AddToClassList("disabled-label-text");
                _localeLabel.AddToClassList("disabled-label-text");
            }
            else
            {
                _collectionName.RemoveFromClassList("disabled-label-text");
                _localeLabel.RemoveFromClassList("disabled-label-text");
            }
        }
        
        private void HandleCollectionNameChange(ChangeEvent<string> evt)
        {
#if LOCALIZATION_EXIST
            DSData.instance.database.tableCollectionName = evt.newValue;
#endif
        }

        private void SetTree(ConversationTree tree)
        {
            _conversationEditView.SetUpTree(tree);
        }

        void OnNodeSelectionChanged(NodeView node)
        {
            _inspectorView.ShowDialogInspector(node);
            _conversationEditView.ClearButtonHovers();
        }

        void OnRefreshInspector()
        {
            _inspectorView.RefreshCurrentInspector();
        }

        void OnNodesRemoved()
        {
            _conversationEditView.SetConversationNameSelected();
        }

        void OnActorsRemoved()
        {
            _conversationEditView.SetConversationNameAndRefresh();
        }

        void OpenDatabaseSelector()
        {
            _dialogSelector?.ClearDatabaseSelection();
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

        void OnUpdateConversationTable(ConversationGroup group)
        {
            _conversationMultiColumListView.SetupTable(_currentDatabase, group);
        }

        void OnEditConversation(ConversationTree conversation)
        {
            _conversationListView.style.display = DisplayStyle.None;
            _conversationEditView.style.display = DisplayStyle.Flex;
            SetTree(conversation);
        }
        
        void OnBackToConversationsList()
        {
            _conversationListView.style.display = DisplayStyle.Flex;
            _conversationEditView.style.display = DisplayStyle.None;
            //Reset tree
            _treeView.ResetTreeData();
        }

        void OnEditDatabase()
        {
            _dialogEditorVE.RemoveFromClassList(hiddenContentClassName);
        }
        
        void OnCloseEditDatabase()
        {
            _dialogEditorVE.AddToClassList(hiddenContentClassName);
        }
        
        void OnLocalizeDatabase()
        {
            _dialogLocalizationVE.RemoveFromClassList(hiddenContentClassName);
        }

        void OnCloseLocalizeDatabase()
        {
            _dialogLocalizationVE.AddToClassList(hiddenContentClassName);
        }

        void OnLocalizeDatabaseCreated()
        {
            _dialogLocalizationVE.AddToClassList(hiddenContentClassName);
            #if LOCALIZATION_EXIST
                _currentDatabase.localizationActivated = true;
            #endif
            SetUpLocalizationInfo(_currentDatabase);
        }

        void OnRemoveDatabase()
        {
            _confirmationModal.UpdateModalText("Are you sure you want to remove the database?");
            _confirmationModal.OnConfirmModal = OnRemoveDatabaseConfirm;
            _confirmationModalVE.RemoveFromClassList(hiddenContentClassName);
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
            _currentDatabase.DeleteDatabase();
            DSData.instance.database = null;
            _confirmationModalVE.AddToClassList(hiddenContentClassName);
            OpenDatabaseSelector();
            ClearView();
        }

        private void ClearView()
        {
            _currentDatabase = null;
            _toolbarHeaderVE?.AddToClassList(hiddenContentClassName);
            _actorMultiColumListView.ClearList();
            _conversationEditView.ClearConversationView();
            ClearAllValueCallbacks();
        }
        
        private void ClearAllValueCallbacks()
        {
            _unregisterAll?.Invoke();
            _unregisterAll = null;
        }
    }

