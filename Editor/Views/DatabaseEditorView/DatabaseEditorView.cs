using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DatabaseEditorView : VisualElement
{
    public Action OnCloseModal;
    private TextField _dbTitle, _dbDesc;
    private readonly string assetName = "DatabaseEditorView";
    public new class UxmlFactory:  UxmlFactory<DatabaseEditorView, DatabaseEditorView.UxmlTraits> {}

    public DatabaseEditorView()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
        uxml.CloneTree(this);

        _dbTitle = this.Q<TextField>("db-title-text-field");
        _dbDesc = this.Q<TextField>("db-description-text-field");
        Button closeButton = this.Q<Button>("db-editor-close");
        var texture = EditorGUIUtility.IconContent("CrossIcon").image;
        closeButton.focusable = false;
        closeButton.Add(new Image {
            image = texture,
        });
        closeButton.clicked += OnCloseButtonClicked;
    
        //Debug Mode
        Toggle debugActivated = this.Q<Toggle>("debug-activated");
        debugActivated.value = DSData.instance.debugMode;
        debugActivated.RegisterValueChangedCallback((e) =>
        {
            DSData.instance.debugMode = e.newValue;
        });
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

