using System;
using AQM.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class ToolbarHeaderView : VisualElement
{
    public Action<DialogSystemDatabase> onDatabaseSelected;
    public Action onEditDatabase, onCreateDatabase, onRemoveDatabase, onLocalizeDatabase;
    private readonly Label _databaseTitle;
    private readonly string assetName = "ToolbarHeaderView";
    public new class UxmlFactory:  UxmlFactory<ToolbarHeaderView, ToolbarHeaderView.UxmlTraits> {}

    public ToolbarHeaderView()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
        uxml.CloneTree(this);
        
        _databaseTitle =this.Q<Label>("database-name");
        SetUpAddNewButton();
        SetUpDeleteButton();
        SetUpLocalizationButton();
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
            onDatabaseSelected?.Invoke(db);
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

    private void SetUpLocalizationButton()
    {
        VisualElement localizationButton =this.Q("db-localization-button");
        var texture = EditorGUIUtility.IconContent("d_SearchQueryAsset Icon").image;
        var button = new Button();
        button.AddToClassList("db-button");
        button.focusable = false;
        localizationButton.Add(button);
        button.Add(new Image {
            image = texture,
        });
        button.clicked += OpenLocalizationModal;
    }

    private void OpenEditModal()
    {
        onEditDatabase?.Invoke();
    }
    
    private void OpenLocalizationModal()
    {
        onLocalizeDatabase?.Invoke();
    }

    private void OpenNewDatabase()
    {
        onCreateDatabase?.Invoke();
    }

    private void OpenConfirmationModal()
    {
        onRemoveDatabase?.Invoke();
    }
    
    public void SetUpSelector(DialogSystemDatabase db)
    {
        _databaseTitle.bindingPath = "title";
        _databaseTitle.Bind(new SerializedObject(db));
    }
}
