using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DatabaseSelectorView : VisualElement
{
    public Action<DialogSystemDatabase> OnDatabaseSelected;
    public Action OnCreateNewDatabaseClicked;
    private ObjectField _dbSelector;
    private readonly string assetName = "DatabaseSelectorView";
    public new class UxmlFactory:  UxmlFactory<DatabaseSelectorView, DatabaseSelectorView.UxmlTraits> {}

    public DatabaseSelectorView()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
        uxml.CloneTree(this);

        Button createNewButton = this.Q<Button>("db-create-new-button");
        createNewButton.clicked += OnCreateNewButtonClicked;
        VisualElement fieldContainer = this.Q("db-object-container-field");
        _dbSelector = new ObjectField
        {
            objectType = typeof(DialogSystemDatabase)
        };
        fieldContainer.Add(_dbSelector);
    
        RegisterCallbacks();
    }

    public void ClearDatabaseSelection()
    {
        _dbSelector.value = null;
    }

    private void RegisterCallbacks()
    {
        EventCallback<ChangeEvent<UnityEngine.Object>> changeEvent = (e) =>
        {
            if (e.newValue != null)
            {
                OnDatabaseSelected?.Invoke(e.newValue as DialogSystemDatabase);
            }
        };

        _dbSelector.RegisterValueChangedCallback(changeEvent);
    }


    private void OnCreateNewButtonClicked()
    {
        OnCreateNewDatabaseClicked?.Invoke();
    }
}

