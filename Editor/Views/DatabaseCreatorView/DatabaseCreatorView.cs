using System;
using AQM.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DatabaseCreatorView : VisualElement
{
    public Action OnCloseModal;
    public Action<DialogSystemDatabase> OnCreatedDatabase;
    private TextField _dbTitle, _dbDesc;
    private string defaultTitle = "Default Title";
    private string defaultDescription= "Default Description";
    private readonly string assetName = "DatabaseCreatorView";
    public new class UxmlFactory:  UxmlFactory<DatabaseCreatorView, DatabaseCreatorView.UxmlTraits> {}

    public DatabaseCreatorView()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
        uxml.CloneTree(this);
        
        _dbTitle = this.Q<TextField>("db-title-text-field");
        _dbDesc = this.Q<TextField>("db-description-text-field");
        ResetInputs();

        SetUpCloseButton();
        SetUpCreateButton();
    }

    public void ResetInputs()
    {
        _dbTitle.value = defaultTitle;
        _dbDesc.value = defaultDescription;
    }

    void SetUpCloseButton()
    {
        Button closeButton = this.Q<Button>("db-editor-close");
        var texture = EditorGUIUtility.IconContent("CrossIcon").image;
        closeButton.focusable = false;
        closeButton.Add(new Image {
            image = texture,
        });
        closeButton.clicked += OnCloseButtonClicked;
    }
    
    private void OnCloseButtonClicked()
    {
        OnCloseModal?.Invoke();
    }

    void SetUpCreateButton()
    {
        Button closeButton = this.Q<Button>("db-create");
        closeButton.clicked += OnCreateDatabase;
    }
    
    private void OnCreateDatabase()
    {
        DialogSystemDatabase newDb = DatabaseFactory.CreateDatabase(_dbTitle.value, _dbDesc.value);
        OnCreatedDatabase?.Invoke(newDb);
    }
}
