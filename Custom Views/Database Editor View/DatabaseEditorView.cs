using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DatabaseEditorView : VisualElement
{
    public Action OnCloseModal;
    private TextField _dbTitle, _dbDesc;
    public new class UxmlFactory:  UxmlFactory<DatabaseEditorView, DatabaseEditorView.UxmlTraits> {}

    public DatabaseEditorView()
    {
        string uriFile = "Assets/dialog-system/Custom Views/Database Editor View/DatabaseEditorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);

        _dbTitle = this.Q<TextField>("db-title-text-field");
        _dbDesc = this.Q<TextField>("db-description-text-field");
        Button closeButton = this.Q<Button>("db-editor-close");
        var texture = EditorGUIUtility.IconContent("winbtn_win_close@2x").image;
        closeButton.focusable = false;
        closeButton.Add(new Image {
            image = texture,
        });
        closeButton.clicked += OnCloseButtonClicked;
    }

    public void SetUpEditor(DialogSystemDatabase db)
    {
        _dbTitle.bindingPath = "title";
        _dbTitle.Bind(new SerializedObject(db));
        _dbDesc.bindingPath = "description";
        _dbDesc.Bind(new SerializedObject(db));
    }

    private void OnCloseButtonClicked()
    {
        OnCloseModal?.Invoke();
    }
}
