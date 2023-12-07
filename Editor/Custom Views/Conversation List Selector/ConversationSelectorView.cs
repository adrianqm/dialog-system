using System;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ConversationSelectorView : VisualElement
{
    public new class UxmlFactory:  UxmlFactory<ConversationSelectorView, ConversationSelectorView.UxmlTraits> {}

    public Action<ConversationGroup> onConversationGroupSelected;
    
    private TreeView _treeView;
    private DialogSystemDatabase _database;
    private int _selectedIndex = -1;
    private ConversationGroup _selectedGroup;
    private int _currentCreationId;
    private bool _columnsHandled;
    private Dictionary<string, int> _translationKeyMap = new();
    
    public ConversationSelectorView()
    {
        string uriFile = "Assets/dialog-system/Editor/Custom Views/Conversation List Selector/ConversationSelectorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);
        
        SetUpToolbar();
        SetUpContextMenu();
    }

    private void SetUpToolbar()
    {
        ToolbarMenu menuBar = this.Q<ToolbarMenu>();
        VisualElement textElement = menuBar.ElementAt(0);
        textElement.AddToClassList("plusElement");
        textElement.Add(new Image {
            image = EditorGUIUtility.IconContent("Toolbar Plus").image
        });
        menuBar.menu.AppendAction("Add New Group", a => AddChildToTree());
    }

    private void SetUpContextMenu()
    {
        VisualElement scroll = this.Q("unity-content-and-vertical-scroll-container");
        scroll.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Add New Group", a => AddChildToTree());
            
            DropdownMenuAction.Status removeStatus = DropdownMenuAction.Status.Disabled;
            string message = "Remove Selected Group";
            if( _treeView.selectedItem != null) removeStatus = DropdownMenuAction.Status.Normal;
            evt.menu.AppendAction(message, a => RemoveSelectedGroup(),removeStatus);
        }));
    }

    private void AddChildToTree()
    {
        if (_database)
        {
            ConversationGroup group = null;
            if (_selectedGroup)
            {
                group = DatabaseUtils.CreateConversationGroup(_database, "Default Group "+ _currentCreationId, _selectedGroup);
            }
            else group = DatabaseUtils.CreateConversationGroup(_database, "Default Group"+ _currentCreationId);
            
            if (group)
            {
                var zeroList = new List<TreeViewItemData<ConversationGroup>>();
                _translationKeyMap.Add(group.guid, _currentCreationId);
                int elementId = _currentCreationId;
                var newTreeList = new TreeViewItemData<ConversationGroup>(_currentCreationId, group, zeroList);
                _currentCreationId += 1;
                if(_selectedGroup != null) _treeView.AddItem(newTreeList,_translationKeyMap[_selectedGroup.guid]);
                else _treeView.AddItem(newTreeList);
                
                _treeView.ExpandItem(_treeView.GetIdForIndex(_selectedIndex));
                VisualElement ve = _treeView.GetRootElementForId(elementId);
                var label = ve.Q<Label>(className: "group-label");
                var textField = ve.Q<TextField>(className: "group-textfield");
                label.AddToClassList("hidden");
                textField.RemoveFromClassList("hidden");
                schedule.Execute(()=>
                {
                    textField.BringToFront();
                    textField.Q(TextInputBaseField<string>.textInputUssName).Focus();
                });
            }
        }
    }

    private void RemoveSelectedGroup()
    {
        bool deleteClicked = EditorUtility.DisplayDialog(
            "Delete Group selected",
            "Are you sure you want to delete the group?",
            "Delete",
            "Cancel");

        if (deleteClicked)
        {
            int parentId = _treeView.GetParentIdForIndex(_selectedIndex);
            ConversationGroup parentGroup = _treeView.GetItemDataForId<ConversationGroup>(parentId);
            ConversationGroup group = _treeView.selectedItem as ConversationGroup;
            DatabaseUtils.DeleteConversationGroup(_database, group, parentGroup);
            if (!_treeView.TryRemoveItem(_translationKeyMap[group.guid])) return;
            _treeView.RefreshItems();
            _selectedIndex = -1;
        }
    }

    public void SetupTree(DialogSystemDatabase database)
    {
        _database = database;
        
        List<TreeViewItemData<ConversationGroup>> treeRoots = new List<TreeViewItemData<ConversationGroup>>();
        _currentCreationId = 0;
        _translationKeyMap.Clear();
        foreach (var group in _database.conversationGroups)
        {
            List<TreeViewItemData<ConversationGroup>> groupsInGroup = AddConversationGroupsToTree(group);
            _translationKeyMap.Add(group.guid, _currentCreationId);
            treeRoots.Add(new TreeViewItemData<ConversationGroup>(_currentCreationId, group, groupsInGroup));
            _currentCreationId += 1;
        }

        if (_treeView != null)
        {
            _treeView.selectedIndicesChanged -= SelectionChanged;
            _treeView.Clear();
        }
        _treeView = this.Q<TreeView>();
        _treeView.selectedIndicesChanged += SelectionChanged;
        _treeView.SetRootItems(treeRoots);
        _treeView.Rebuild();

        if (!_columnsHandled)
        {
            _columnsHandled = true;
            _treeView.makeItem = () =>
            {
                var ve = new VisualElement();
                ve.AddToClassList("group-ve");
                ve.focusable = true;
            
                var label = new Label();
                label.AddToClassList("group-label");
                ve.Add(label);
            
                var textField = new TextField();
                textField.AddToClassList("group-textfield");
                textField.AddToClassList("hidden");
                textField.focusable = true;
                ve.Add(textField);
            
                return ve;
            };
            _treeView.bindItem = (ve, index) =>
            {
                var label = ve.Q<Label>(className: "group-label");
                label.bindingPath = "title";
                label.Bind(new SerializedObject(_treeView.GetItemDataForIndex<ConversationGroup>(index)));
            
                var textField = ve.Q<TextField>(className: "group-textfield");
                textField.bindingPath = "title";
                textField.Bind(new SerializedObject(_treeView.GetItemDataForIndex<ConversationGroup>(index)));
                ve.Add(textField);
            
                label.RegisterCallback<MouseDownEvent>((evt) =>
                {
                    if (evt.clickCount == 2)
                    {
                        label.AddToClassList("hidden");
                        textField.RemoveFromClassList("hidden");
                        textField.Focus();
                    }
                });
            
                textField.RegisterCallback<FocusOutEvent>((_) =>
                {
                    textField.AddToClassList("hidden");
                    label.RemoveFromClassList("hidden");
                });
            };
        }
    }

    private List<TreeViewItemData<ConversationGroup>> AddConversationGroupsToTree (ConversationGroup group)
    {
        List<TreeViewItemData<ConversationGroup>> groupsInGroup = new List<TreeViewItemData<ConversationGroup>>();
        foreach (var g in group.groups)
        {
            List<TreeViewItemData<ConversationGroup>> childGroups = AddConversationGroupsToTree(g);
            
            _translationKeyMap.Add(g.guid, _currentCreationId);
            groupsInGroup.Add(new TreeViewItemData<ConversationGroup>(_currentCreationId, g,childGroups));
            _currentCreationId += 1;
        }
        return groupsInGroup;
    }

    private void SelectionChanged(IEnumerable<int> indexes)
    {
        _selectedIndex = indexes.ToList()[0];
        _selectedGroup = _treeView.GetItemDataForIndex<ConversationGroup>(_selectedIndex);
        onConversationGroupSelected?.Invoke(_selectedGroup);
    }
}
