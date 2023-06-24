
using System;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.BaseCommands.Merge.Xml;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;
using ObjectField = UnityEditor.UIElements.ObjectField;

public class ActorsListView : MultiColumnListView
{
    public new class UxmlFactory : UxmlFactory<ActorsListView, UxmlTraits>{}
    
    private List<Actor> _filteredList;
    private List<Actor> _currentActorList;
    private ActorsTree _actorsTree;
    private Action _unregisterAll;
    private ToolbarSearchField _searchField;
    

    public ActorsListView()
    {
        Undo.undoRedoPerformed += OnUndoRedo;
        columnSortingChanged += ColumnSortChange;
    }

    private void OnUndoRedo()
    {
        _searchField?.SetValueWithoutNotify("");
        SetupTable(_actorsTree);
    }

    public void SetupTable(ActorsTree actorsTree)
    {
        if (actorsTree == null) return;

        _actorsTree = actorsTree;
        _currentActorList = _actorsTree.actors;
        
        // Create new table
        if(itemsSource == null) AddColumns();
        itemsSource = _currentActorList;
        RefreshTable();

        fixedItemHeight = 55;
        reorderable = true;
        sortingEnabled = true;
        reorderMode = ListViewReorderMode.Animated;
        selectionType = SelectionType.Multiple;
        
    }

    public void CreateNewActor()
    {
        if (_actorsTree)
        {
            _actorsTree.CreateActor();
            _currentActorList = _actorsTree.actors;
            _searchField?.SetValueWithoutNotify("");
            RefreshTable();
            ClearSelection();
            ScrollToItem(-1); // Seems that will be solved in followed versions
            AddToSelection(_actorsTree.actors.Count -1);
        }
        else Debug.LogWarning("Actors Tree does not exist for the current Database");
    }

    public void SetUpScrollContainerManipulator(VisualElement scrollContainer)
    {
        scrollContainer.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Add New Actor", a => CreateNewActor());
            
            DropdownMenuAction.Status status = DropdownMenuAction.Status.Disabled;
            string message = "Remove Selected Actor";
            if(selectedItems != null && selectedItems?.Count() >= 1) status = DropdownMenuAction.Status.Normal;
            if (selectedItems?.Count() > 1) message = "Remove Selected Actors";
            evt.menu.AppendAction(message, a => RemoveSelectedActors(),status);
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
            _currentActorList = _actorsTree.actors;
            RefreshTable();
        } else
        {
            _filteredList = new List<Actor>();
            _actorsTree.actors.ForEach(a =>
            {
                if (a.fullName.ToLower().Contains(value) || a.description.ToLower().Contains(value))
                {
                    _filteredList.Add(a);
                }
            });
                
            _currentActorList = _filteredList;
            RefreshTable();
        }
    }

    private void RemoveSelectedActors()
    {
        
        List<Actor> auxArray = new List<Actor>(_actorsTree.actors);
        foreach (var actor in auxArray)
        {
            if (selectedItems.Contains(actor))
            {
                DeleteActor(actor);
            }
        }
        RefreshTable();
        ClearSelection();
    }

    private VisualElement OnMakeImageCell()
    {
        var ve = new VisualElement();
        var image = new Image();
        ve.Add(image);
        image.AddToClassList("actorsImageCell--image");
        var imageSelector = new ObjectField();
        imageSelector.objectType = typeof(Sprite);
        ve.Add(imageSelector);
        imageSelector.AddToClassList("actorsImageCell--image-selector");
        ve.style.paddingTop = 5;
        ve.style.paddingBottom = 5;
        ve.style.height = 50;
        return ve;
    }

    private VisualElement OnMakeLabelCell()
    {
        var ve = new VisualElement();
        var textField = new TextField();
        textField.AddToClassList("actorsTextFieldCell--textField");
        ve.Add(textField);
        ve.AddToClassList("actorsTextFieldCell");
        return ve;
    }
    
    private VisualElement OnMakeToggleCell()
    {
        var ve = new VisualElement();
        var toggle = new Toggle();
        ve.Add(toggle);
        ve.AddToClassList("actorsToggleCell");
        return ve;
    }
    
    private VisualElement OnMakeDeleteCell()
    {
        var ve = new VisualElement();
        var texture = EditorGUIUtility.IconContent("winbtn_win_close").image;
        var button = new Button();
        button.AddToClassList("actorsDeleteCell--button");
        button.focusable = false;
        ve.Add(button);
        button.Add(new Image {
            image = texture,
        });
        ve.AddToClassList("actorsDeleteCell");
        return ve;
    }

    private void OnBindCell(VisualElement ve, int index, int columnIndex)
    {
        if (_currentActorList[index] == null) return;
        switch (columnIndex)
        {
            case 0:
                ObjectField spritePicker = ve.Q<ObjectField>();
                spritePicker.bindingPath = "actorImage";
                spritePicker.Bind(new SerializedObject(_currentActorList[index]));
                Image image = ve.Q<Image>();
                image.sprite = _currentActorList[index].actorImage;
                EventCallback<ChangeEvent<UnityEngine.Object>> changeEvent = (e) =>
                {
                    image.sprite = _currentActorList[index].actorImage;
                };
 
                spritePicker.RegisterValueChangedCallback(changeEvent);
                // Add Unregister callback for later
                _unregisterAll += () => spritePicker.UnregisterValueChangedCallback(changeEvent);
                break;
            case 1:
                TextField fullNameText = ve.Q<TextField>();
                fullNameText.bindingPath = "fullName";
                fullNameText.Bind(new SerializedObject(_currentActorList[index]));
                _unregisterAll += () => fullNameText.Blur();
                break;
            case 2:
                TextField descText = ve.Q<TextField>();
                descText.bindingPath = "description";
                descText.Bind(new SerializedObject(_currentActorList[index]));
                break;
            case 3:
                Toggle toggle = ve.Q<Toggle>();
                toggle.bindingPath = "isPlayer";
                toggle.Bind(new SerializedObject(_currentActorList[index]));
                break;
            case 4: 
                Button deleteButton = ve.Q<Button>();
                deleteButton.clickable = new Clickable(()=>{
                    DeleteActor(_currentActorList[index]);
                    RefreshTable();
                    ClearSelection();
                });
                break;
        }
    }

    private void DeleteActor(Actor actor)
    {
        _actorsTree.DeteleActor(actor);
        if (_currentActorList.Contains(actor)) _currentActorList.Remove(actor);
    }

    private void AddColumns()
    {
        columns?.Add(new Column
        {
            name = "imageColumn",
            title = "Image",
            bindCell = (x, y) => { OnBindCell(x, y, 0); },
            makeCell = OnMakeImageCell,
            stretchable = true,
            maxWidth = 100,
            minWidth = 100,
            sortable = false
        });
        columns?.Add(new Column
        {
            name = "fullNameColumn",
            title = "Name",
            bindCell = (x, y) => { OnBindCell(x, y, 1); },
            makeCell = OnMakeLabelCell,
            stretchable = true,
            minWidth = 75,
            sortable = true
        });
        columns?.Add(new Column
        {
            name = "descriptionColumn",
            title = "Description",
            bindCell = (x, y) => { OnBindCell(x, y, 2); },
            makeCell = OnMakeLabelCell,
            stretchable = true,
            minWidth = 100,
            sortable = true
        });
        columns?.Add(new Column
        {
            name = "isPlayerColumn",
            title = "Player",
            bindCell = (x, y) => { OnBindCell(x, y, 3); },
            makeCell = OnMakeToggleCell,
            stretchable = false,
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
            if (sortColumnDescription.columnName.Contains("fullNameColumn"))
            {
                if (sortColumnDescription.direction == SortDirection.Ascending)
                {
                    _currentActorList.Sort((x,y) => String.CompareOrdinal(x.fullName,y.fullName));
                }
                else
                {
                    _currentActorList.Sort((x,y) => String.CompareOrdinal(y.fullName,x.fullName));
                }
            }else if (sortColumnDescription.columnName.Contains("descriptionColumn"))
            {
                if (sortColumnDescription.direction == SortDirection.Ascending)
                {
                    _currentActorList.Sort((x,y) => String.CompareOrdinal(x.description,y.description));
                }
                else
                {
                    _currentActorList.Sort((x,y) => String.CompareOrdinal(y.description,x.description));
                }
            }else if (sortColumnDescription.columnName.Contains("isPlayerColumn"))
            {
                if (sortColumnDescription.direction == SortDirection.Ascending)
                {
                    _currentActorList.Sort((x, y) =>
                    {
                        return Convert.ToInt32(x.isPlayer) - Convert.ToInt32(y.isPlayer);
                    });
                }
                else
                {
                    _currentActorList.Sort((x, y) =>
                    {
                        return Convert.ToInt32(y.isPlayer) - Convert.ToInt32(x.isPlayer);
                    });
                } 
            }
            RefreshTable();
        }
    }

    private void RefreshTable()
    {
        ClearAllValueChangedCallbacks();
        itemsSource = _currentActorList;
        RefreshItems();
    }
    
    private void ClearAllValueChangedCallbacks()
    {
        _unregisterAll?.Invoke();
        _unregisterAll = null;
    }
}
