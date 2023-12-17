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
    private Dictionary<string, int> _translationKeyMap = new();
    private ToolbarSearchField _searchField;
    private List<TreeViewItemData<ConversationGroup>> _filteredList = new();
    private List<TreeViewItemData<ConversationGroup>> _rootList = new();
    
    public ConversationSelectorView()
    {
        string uriFile = "Assets/dialog-system/Editor/Custom Views/Conversation List Selector/ConversationSelectorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);
        
        SetUpToolbar();
        SetUpContextMenu();
        _treeView = this.Q<TreeView>();
    }

    private void SetUpToolbar()
    {
        ToolbarMenu menuBar = this.Q<ToolbarMenu>("groupsToolbar");
        VisualElement textElement = menuBar.ElementAt(0);
        textElement.AddToClassList("plusElement");
        textElement.Add(new Image {
            image = EditorGUIUtility.IconContent("Toolbar Plus").image
        });
        menuBar.menu.AppendAction("Create Group", a => AddGroup());
        menuBar.menu.AppendAction("Create Group Child", a => AddGroupChild());
        
        _searchField = this.Q<ToolbarSearchField>("groupsSearchFilter");
        _searchField.RegisterValueChangedCallback((evt) =>
        {
            FilterTree(evt.newValue.ToLower());
        });
    }

    private void FilterTree(string value)
    {
        if (value == "")
        {
            _treeView.SetRootItems(_rootList);
            _treeView.RefreshItems();
            _treeView.reorderable = true;
        } else
        {
            _filteredList = new List<TreeViewItemData<ConversationGroup>>();
            _filteredList = FilterGroups(_rootList, value.ToLower());
            _treeView.SetRootItems(_filteredList);
            _treeView.RefreshItems();
            _treeView.reorderable = false;
        }
    }
    
    private List<TreeViewItemData<ConversationGroup>> FilterGroups(List<TreeViewItemData<ConversationGroup>> treeViewData, string searchTerm)
    {
        var results = new List<TreeViewItemData<ConversationGroup>>();

        foreach (var node in treeViewData)
        {
            if (node.data.title.ToLower().Contains(searchTerm))
            {
                results.Add(new TreeViewItemData<ConversationGroup>(node.id, node.data));
            }
            
            results.AddRange(FilterGroups(node.children.ToList(), searchTerm));
        }
        return results;
    }

    private void SetUpContextMenu()
    {
        VisualElement scroll = this.Q("unity-content-and-vertical-scroll-container");
        scroll.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Create Group", a => AddGroup());
            evt.menu.AppendAction("Create Group Child", a => AddGroupChild());
            
            DropdownMenuAction.Status removeStatus = DropdownMenuAction.Status.Disabled;
            string message = "Remove Selected Group";
            if(_treeView.selectedItem != null) removeStatus = DropdownMenuAction.Status.Normal;
            evt.menu.AppendAction(message, a => RemoveSelectedGroup(),removeStatus);
        }));
    }
    
    private void AddGroup()
    {
        if (!_database) return;

        ConversationGroup group = null;
        group = DatabaseUtils.CreateConversationGroup(_database, "Default Group "+ _currentCreationId);
        AddNewGroupToTree(group,false);
    }

    private void AddGroupChild()
    {
        if (!_database) return;
        
        ConversationGroup group = null;
        if (_selectedGroup)
        {
            group = DatabaseUtils.CreateConversationGroup(_database, "Default Group "+ _currentCreationId, _selectedGroup);
        }
        else group = DatabaseUtils.CreateConversationGroup(_database, "Default Group "+ _currentCreationId);

        AddNewGroupToTree(group, true);
    }

    private void AddNewGroupToTree(ConversationGroup group, bool isChild)
    {
        if (group)
        {
            _searchField?.SetValueWithoutNotify("");
            _treeView.SetRootItems(_rootList);
            _treeView.Rebuild();
                
            var zeroList = new List<TreeViewItemData<ConversationGroup>>();
            _translationKeyMap.Add(group.guid, _currentCreationId);
            int elementId = _currentCreationId;
            var newTreeList = new TreeViewItemData<ConversationGroup>(_currentCreationId, group, zeroList);
            _currentCreationId += 1;
            if (isChild && _selectedGroup)
            {
                _treeView.AddItem(newTreeList,_translationKeyMap[_selectedGroup.guid]);
                _treeView.ExpandItem(_treeView.GetIdForIndex(_selectedIndex));
            }
            else
            {
                _treeView.AddItem(newTreeList);
                _rootList.Add(newTreeList);
            }
            
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

    private void RemoveSelectedGroup()
    {
        bool deleteClicked = EditorUtility.DisplayDialog(
            "Delete Group selected",
            "Are you sure you want to delete the group?",
            "Delete",
            "Cancel");

        if (deleteClicked)
        {
            _searchField?.SetValueWithoutNotify("");
            _treeView.SetRootItems(_rootList);
            _treeView.Rebuild();
            
            ConversationGroup group = _treeView.selectedItem as ConversationGroup;
            int groupIndex = _treeView.GetIdForIndex(_translationKeyMap[group.guid]);
            int parentId = _treeView.GetParentIdForIndex(groupIndex);
            ConversationGroup parentGroup = _treeView.GetItemDataForId<ConversationGroup>(parentId);
            if(!_treeView.TryRemoveItem(_translationKeyMap[group.guid])) return;
            DatabaseUtils.DeleteConversationGroup(_database, group, parentGroup, _translationKeyMap);
            if(_rootList.Count > 0) _treeView.SetSelection(0);
            SetupTree(_database);
        }
    }

    public void SetupTree(DialogSystemDatabase database)
    {
        _database = database;
        _selectedGroup = null;
        _selectedIndex = -1;
        
        if (_treeView != null)
        {
            _treeView.selectedIndicesChanged -= SelectionChanged;
            _treeView.ClearSelection();
        }
        _treeView.selectedIndicesChanged += SelectionChanged;
        
        SetTree();

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

    private void SetTree()
    {
        List<TreeViewItemData<ConversationGroup>> treeRoots = new List<TreeViewItemData<ConversationGroup>>();
        _currentCreationId = 0;
        _translationKeyMap.Clear();
        _rootList.Clear();
        foreach (var group in _database.conversationGroups)
        {
            List<TreeViewItemData<ConversationGroup>> groupsInGroup = AddConversationGroupsToTree(group);
            _translationKeyMap.Add(group.guid, _currentCreationId);
            treeRoots.Add(new TreeViewItemData<ConversationGroup>(_currentCreationId, group, groupsInGroup));
            _currentCreationId += 1;
        }
        
        _rootList = treeRoots;
        _searchField?.SetValueWithoutNotify("");
        _treeView.SetRootItems(treeRoots);
        _treeView.Rebuild();
        
        //Set default
        if(treeRoots.Count > 0)_treeView.SetSelection(0);
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
        if(indexes == null) return;
        _selectedIndex = indexes.ToList()[0];
        _selectedGroup = _treeView.GetItemDataForIndex<ConversationGroup>(_selectedIndex);
        onConversationGroupSelected?.Invoke(_selectedGroup);
    }
}
