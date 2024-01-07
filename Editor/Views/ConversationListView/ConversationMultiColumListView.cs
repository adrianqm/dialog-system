
using System;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ConversationMultiColumListView : MultiColumnListView
{
    public new class UxmlFactory : UxmlFactory<ConversationMultiColumListView, UxmlTraits>{}
    
    public Action<ConversationTree> onEditConversation;
    private List<ConversationTree> _filteredList;
    private List<ConversationTree> _currentConversationList;
    private ConversationGroup _conversationGroup;
    private DialogSystemDatabase _database;
    private Action _unregisterAll;
    private ToolbarSearchField _searchField;
    

    public ConversationMultiColumListView()
    {
        columnSortingChanged += ColumnSortChange;
    }

    public void SetupTable(DialogSystemDatabase database, ConversationGroup conversationGroup)
    {
        if (conversationGroup.conversations == null) return;

        _database = database;
        _conversationGroup = conversationGroup;
        _currentConversationList = conversationGroup.conversations;
        _searchField?.SetValueWithoutNotify("");
        
        // Create new table
        if(itemsSource == null) AddColumns();
        itemsSource = _currentConversationList;
        RefreshTable();

        fixedItemHeight = 55;
        reorderable = true;
        sortingEnabled = true;
        reorderMode = ListViewReorderMode.Animated;
        selectionType = SelectionType.Multiple;
        //showAddRemoveFooter = true;
        
        //this.Q<Button>("unity-list-view__add-button").clickable = new Clickable(CreateNewConversation);
        //this.Q<Button>("unity-list-view__remove-button").clickable = new Clickable(RemoveSelectedConversations);
    }

    public void CreateNewConversation()
    {
        if (_database && _conversationGroup)
        {
            _conversationGroup.CreateConversation(_database);
            _currentConversationList = _conversationGroup.conversations;
            _searchField?.SetValueWithoutNotify("");
            RefreshTable();
            FocusOnLastElement();
        }
        else Debug.LogWarning("Conversations Group does not exist");
    }

    private void FocusOnLastElement()
    {
        ScrollToItem(-1); // Seems that will be solved in followed versions
        var list = this.Query<VisualElement>(className: "unity-list-view__item").ToList();
        if (list.Count <= 0) return;
        int lastIndex = list.Count - 1;
        SetSelection(lastIndex);
        var itemView = list.ElementAt(lastIndex);
        var nameField = itemView.Q<TextField>(className:"conversationsTextFieldCell--textField");
        nameField.Focus();
    }

    public void SetUpScrollContainerManipulator(VisualElement scrollContainer)
    {
        scrollContainer.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Add New Conversation", a => CreateNewConversation());
            
            DropdownMenuAction.Status status = DropdownMenuAction.Status.Disabled;
            string message = "Remove Selected Conversation";
            if(selectedItems != null && selectedItems?.Count() >= 1) status = DropdownMenuAction.Status.Normal;
            if (selectedItems?.Count() > 1) message = "Remove Selected Conversations";
            evt.menu.AppendAction(message, a => RemoveSelectedConversations(),status);
        }));
    }
    
    public void SetUpSearchFieldFilterCallback(ToolbarSearchField searchField)
    {
        _searchField = searchField;
        _searchField.RegisterValueChangedCallback((evt) =>
        {
            FilterTable(evt.newValue.ToLower());
        });
    }

    private void FilterTable(string value)
    {
        if (value == "")
        {
            _currentConversationList = _conversationGroup.conversations;
            RefreshTable();
        } else
        {
            _filteredList = new List<ConversationTree>();
            _conversationGroup.conversations.ForEach(a =>
            {
                if (a.title.ToLower().Contains(value) || a.description.ToLower().Contains(value))
                {
                    _filteredList.Add(a);
                }
            });
                
            _currentConversationList = _filteredList;
            RefreshTable();
        }
    }

    private void RemoveSelectedConversations()
    {
        if(!selectedItems.Any())return;
        
        bool deleteClicked = EditorUtility.DisplayDialog(
            "Delete conversations selected",
            "Are you sure you want to delete these conversations?",
            "Delete",
            "Cancel");

        if (deleteClicked)
        {
            List<ConversationTree> auxArray = new List<ConversationTree>(_conversationGroup.conversations);
            foreach (var conversation in auxArray)
            {
                if (selectedItems.Contains(conversation))
                {
                    DeleteConversation(conversation);
                }
            }
            RefreshTable();
            ClearSelection();
        }
    }

    private VisualElement OnMakeLabelCell()
    {
        var ve = new VisualElement();
        var textField = new TextField();
        textField.AddToClassList("conversationsTextFieldCell--textField");
        ve.Add(textField);
        ve.AddToClassList("conversationsTextFieldCell");
        return ve;
    }
    
    private VisualElement OnMakeDeleteCell()
    {
        var ve = new VisualElement();
        var texture = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
        var button = new Button();
        button.AddToClassList("conversationsDeleteCell--button");
        button.focusable = false;
        ve.Add(button);
        button.Add(new Image {
            image = texture,
        });
        ve.AddToClassList("conversationsDeleteCell");
        return ve;
    }
    
    private VisualElement OnMakeEditCell()
    {
        var ve = new VisualElement();
        var texture = EditorGUIUtility.IconContent("d_editicon.sml").image;
        var button = new Button();
        button.AddToClassList("conversationsDeleteCell--button");
        button.focusable = false;
        ve.Add(button);
        button.Add(new Image {
            image = texture,
        });
        ve.AddToClassList("conversationsDeleteCell");
        return ve;
    }

    private VisualElement OnMakeStateCell()
    {
        var ve = new VisualElement();
        var label = new Label();
        label.AddToClassList("conversationsLabelCell--label");
        ve.Add(label);
        ve.AddToClassList("conversationsLabelCell");
        return ve;
    }

    private void OnBindCell(VisualElement ve, int index, int columnIndex)
    {
        if (_currentConversationList[index] == null) return;
        switch (columnIndex)
        {
            case 0:
                TextField title = ve.Q<TextField>();
                //title.bindingPath = "title";
                //title.Bind(new SerializedObject(_currentConversationList[index]));
                title.value = _currentConversationList[index].title;
                
                EventCallback<FocusOutEvent> focusOutEvent = (e) =>
                {
                    string valTrimmed = title.value.Trim(' ');
                    if (valTrimmed != "")
                    {
                        _currentConversationList[index].SetName(valTrimmed);
                        EditorUtility.SetDirty(_currentConversationList[index]);
                    }
                    title.value = _currentConversationList[index].title;
                };
                title.RegisterCallback(focusOutEvent);
                _unregisterAll += () =>
                {
                    title.UnregisterCallback(focusOutEvent);
                    title.Blur();
                };
                break;
            case 1:
                TextField descText = ve.Q<TextField>();
                descText.bindingPath = "description";
                descText.Bind(new SerializedObject(_currentConversationList[index]));
                _unregisterAll += () => descText.Blur();
                break;
            case 2:
                Label state = ve.Q<Label>();
                state.bindingPath = "conversationState";
                state.Bind(new SerializedObject(_currentConversationList[index]));
                break;
            case 3:
                Button editButton = ve.Q<Button>();
                editButton.clickable = new Clickable(()=>{
                    onEditConversation?.Invoke(_currentConversationList[index]);
                });
                break;
            case 4: 
                Button deleteButton = ve.Q<Button>();
                deleteButton.clickable = new Clickable(()=>{
                    bool deleteClicked = EditorUtility.DisplayDialog(
                        "Delete conversation selected",
                        "Are you sure you want to delete this conversation?",
                        "Delete",
                        "Cancel");

                    if (deleteClicked)
                    {
                        DeleteConversation(_currentConversationList[index]);
                        RefreshTable();
                        ClearSelection();
                    }
                });
                break;
        }
    }

    private void DeleteConversation(ConversationTree conversation)
    {
        _conversationGroup.DeleteConversation(conversation);
        if (_currentConversationList.Contains(conversation)) _currentConversationList.Remove(conversation);
    }

    private void AddColumns()
    {
        columns?.Add(new Column
        {
            name = "titleColumn",
            title = "Title",
            bindCell = (x, y) => { OnBindCell(x, y, 0); },
            makeCell = OnMakeLabelCell,
            width = 200,
            minWidth = 100,
            sortable = true
        });
        columns?.Add(new Column
        {
            name = "descriptionColumn",
            title = "Description",
            bindCell = (x, y) => { OnBindCell(x, y, 1); },
            makeCell = OnMakeLabelCell,
            stretchable = true,
            minWidth = 100,
            sortable = true
        });
        columns?.Add(new Column
        {
            name = "stateColumn",
            title = "State",
            bindCell = (x, y) => { OnBindCell(x, y, 2); },
            makeCell = OnMakeStateCell,
            stretchable = true,
            maxWidth = 75,
            minWidth = 75,
            sortable = true
        });
        columns?.Add(new Column
        {
            name = "edit",
            title = "",
            bindCell = (x, y) => { OnBindCell(x, y, 3); },
            makeCell = OnMakeEditCell,
            stretchable = false,
            sortable = false,
            maxWidth = 70,
            minWidth = 70
        });
        columns?.Add(new Column
        {
            name = "delete",
            title = "",
            bindCell = (x, y) => { OnBindCell(x, y, 4); },
            makeCell = OnMakeDeleteCell,
            stretchable = false,
            sortable = false,
            maxWidth = 70,
            minWidth = 70
        });
    }

    private void ColumnSortChange()
    {
        foreach (var sortColumnDescription in sortedColumns)
        {
            if (sortColumnDescription.columnName.Contains("titleColumn"))
            {
                if (sortColumnDescription.direction == SortDirection.Ascending)
                {
                    _currentConversationList.Sort((x,y) => String.CompareOrdinal(x.title,y.title));
                }
                else
                {
                    _currentConversationList.Sort((x,y) => String.CompareOrdinal(y.title,x.title));
                }
            }else if (sortColumnDescription.columnName.Contains("descriptionColumn"))
            {
                if (sortColumnDescription.direction == SortDirection.Ascending)
                {
                    _currentConversationList.Sort((x, y) => String.CompareOrdinal(x.description, y.description));
                }
                else
                {
                    _currentConversationList.Sort((x, y) => String.CompareOrdinal(y.description, x.description));
                }
            }

            RefreshTable();
        }
    }
    private void RefreshTable()
    {
        ClearAllValueChangedCallbacks();
        itemsSource = _currentConversationList;
        RefreshItems();
    }
    
    private void ClearAllValueChangedCallbacks()
    {
        _unregisterAll?.Invoke();
        _unregisterAll = null;
    }
}

