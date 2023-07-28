using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarHeaderView : VisualElement
{
    public Action<DialogSystemDatabase> OnDatabaseSelected;
    public Action OnEditDatabase, OnCreateDatabase, OnRemoveDatabase;
    private readonly Label _databaseTitle;
    public new class UxmlFactory:  UxmlFactory<ToolbarHeaderView, ToolbarHeaderView.UxmlTraits> {}

    public ToolbarHeaderView()
    {
        string uriFile = "Assets/dialog-system/Custom Views/Toolbar Header View/ToolbarHeaderView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);
        
        _databaseTitle =this.Q<Label>("database-name");
        SetUpAddNewButton();
        SetUpDeleteButton();
        SetUpEditButton();
        VisualElement fieldContainer = this.Q("db-object-container-field");
        var dbSelector = new ObjectField
        {
            objectType = typeof(DialogSystemDatabase)
        };
        fieldContainer.Add(dbSelector);
        
        //dbSelector.bindingPath = "actorImage";
        //dbSelector.Bind(new SerializedObject(_currentActorList[index]));
        EventCallback<ChangeEvent<UnityEngine.Object>> changeEvent = (e) =>
        {
            if (e.newValue == null) return;
            var db = e.newValue as DialogSystemDatabase;
            OnDatabaseSelected?.Invoke(db);
        };
        dbSelector.RegisterValueChangedCallback(changeEvent);
    }
    
    private void SetUpAddNewButton()
    {
        VisualElement deleteButton =this.Q("db-add-new-button");
        var texture = EditorGUIUtility.IconContent("Toolbar Plus").image;
        var button = new Button();
        button.AddToClassList("db-button");
        button.focusable = false;
        deleteButton.Add(button);
        button.Add(new Image {
            image = texture,
        });
        button.clicked += OpenNewDatabase;
    }
    
    private void SetUpDeleteButton()
    {
        VisualElement deleteButton =this.Q("db-remove-current");
        var texture = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
        var button = new Button();
        button.AddToClassList("db-button");
        button.focusable = false;
        deleteButton.Add(button);
        button.Add(new Image {
            image = texture,
        });
        button.clicked += OpenConfirmationModal;
    }
    
    private void SetUpEditButton()
    {
        VisualElement deleteButton =this.Q("db-edit-button");
        var texture = EditorGUIUtility.IconContent("d_AudioMixerController On Icon").image;
        var button = new Button();
        button.AddToClassList("db-button");
        button.focusable = false;
        deleteButton.Add(button);
        button.Add(new Image {
            image = texture,
        });
        button.clicked += OpenEditModal;
    }

    private void OpenEditModal()
    {
        OnEditDatabase?.Invoke();
    }

    private void OpenNewDatabase()
    {
        OnCreateDatabase?.Invoke();
    }

    private void OpenConfirmationModal()
    {
        OnRemoveDatabase?.Invoke();
    }
    
    public void SetUpSelector(DialogSystemDatabase db)
    {
        _databaseTitle.bindingPath = "title";
        _databaseTitle.Bind(new SerializedObject(db));
    }
}