
using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands.Merge.Xml;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ActorsListView : MultiColumnListView
{
    public new class UxmlFactory : UxmlFactory<ActorsListView, UxmlTraits>
    {
    }

    private List<Actor> _list;
    private ActorsTree _actorsTree;

    public ActorsListView()
    {
        Undo.undoRedoPerformed += OnUndoRedo;
    }
    
    private void OnUndoRedo()
    {
        SetupTable(_actorsTree);
        AssetDatabase.SaveAssets();
    }

    public void SetupTable(ActorsTree actorsTree)
    {
        if (!actorsTree) return;
        _actorsTree = actorsTree;
        
        // Clear previous
        columns?.Clear();
        itemsSource = null;
        
        // Create new table
        AddColumns();

        fixedItemHeight = 55;
        sortingEnabled = true;
        itemsSource = actorsTree.actors;
        reorderable = true;
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
    
    private VisualElement OnMakeToogleCell()
    {
        var ve = new VisualElement();
        var toggle = new Toggle();
        ve.Add(toggle);
        ve.AddToClassList("actorsToogleCell");
        return ve;
    }

    private void OnBindCell(VisualElement ve, int index, int columnIndex)
    {
        if (columnIndex == 0)
        {
            ObjectField spritePicker = ve.Q<ObjectField>();
            spritePicker.bindingPath = "actorImage";
            spritePicker.Bind(new SerializedObject(_actorsTree.actors[index]));
            
            ve.Q<Image>().sprite = _actorsTree.actors[index].actorImage;
            spritePicker.RegisterValueChangedCallback((evt) =>
            {
                ve.Q<Image>().sprite = _actorsTree.actors[index].actorImage;
            });
        }
        
        if (columnIndex == 1)
        {
            TextField text = ve.Q<TextField>();
            text.bindingPath = "fullName";
            text.Bind(new SerializedObject(_actorsTree.actors[index]));
        }

        if (columnIndex == 2)
        {
            TextField text = ve.Q<TextField>();
            text.bindingPath = "description";
            text.Bind(new SerializedObject(_actorsTree.actors[index]));
        }

        if (columnIndex == 3)
        {
            Toggle text = ve.Q<Toggle>();
            text.bindingPath = "isPlayer";
            text.Bind(new SerializedObject(_actorsTree.actors[index]));
        }
    }

    public void CreateNewActor()
    {
        if (_actorsTree)
        {
            Type actorType = typeof(Actor);
            Actor newActor = ScriptableObject.CreateInstance(actorType) as Actor;

            if (newActor)
            {
                newActor.name = actorType.Name;
                newActor.guid = GUID.Generate().ToString();
                newActor.fullName = "defaultName";
                newActor.description = "defaultDesc";
                Undo.RecordObject(_actorsTree, "Actors Tree (CreateActor)");
                _actorsTree.actors.Add(newActor);

                if (!Application.isPlaying)
                {
                    AssetDatabase.AddObjectToAsset( newActor,_actorsTree);
                }
                Undo.RegisterCreatedObjectUndo(newActor, "Actors Tree (CreateActor)");
                AssetDatabase.SaveAssets();
                
                //Add element to list
                SetupTable(_actorsTree);
            }
            else Debug.LogWarning("Not possible to create new actor");
        }
        else Debug.LogWarning("Actors Tree does not exist for the current Database");
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
            makeCell = OnMakeToogleCell,
            stretchable = false,
            maxWidth = 70,
            minWidth = 70
        });
    }
}
