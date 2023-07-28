using System;
using UnityEditor;
using UnityEngine.UIElements;
public class DatabaseCreatorView : VisualElement
{
    public Action OnCloseModal;
    private TextField _dbTitle, _dbDesc;
    public new class UxmlFactory:  UxmlFactory<DatabaseCreatorView, DatabaseCreatorView.UxmlTraits> {}

    public DatabaseCreatorView()
    {
        string uriFile = "Assets/dialog-system/Custom Views/Database Creator View/DatabaseCreatorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);
        
        _dbTitle = this.Q<TextField>("db-title-text-field");
        _dbTitle.value = "Default Title";
        _dbDesc = this.Q<TextField>("db-description-text-field");
        _dbDesc.value = "Default Description";
        Button closeButton = this.Q<Button>("db-editor-close");
        var texture = EditorGUIUtility.IconContent("winbtn_win_close@2x").image;
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
}
