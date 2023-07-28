using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DatabaseSelectorView : VisualElement
{
    public Action<DialogSystemDatabase> OnDatabaseSelected;
    public new class UxmlFactory:  UxmlFactory<DatabaseSelectorView, DatabaseSelectorView.UxmlTraits> {}
    
    public DatabaseSelectorView()
    {
        string uriFile = "Assets/dialog-system/Custom Views/Database Selector View/DatabaseSelectorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);

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
            OnDatabaseSelected?.Invoke(e.newValue as DialogSystemDatabase);
        };
 
        dbSelector.RegisterValueChangedCallback(changeEvent);
    }
}
