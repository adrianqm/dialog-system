using System;
using UnityEditor;
using UnityEngine.UIElements;

public class ConfirmationModalView : VisualElement
{
    public Action OnCloseModal;
    public Action OnConfirmModal;
    private string defaultText = "Are you Sure?";
    private Label title;
    private readonly string assetName = "ConfirmationModalView";
    public new class UxmlFactory:  UxmlFactory<ConfirmationModalView, ConfirmationModalView.UxmlTraits> {}

    public ConfirmationModalView()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
        uxml.CloneTree(this);
    
        title = this.Q<Label>("title");
        Button closeButton = this.Q<Button>("close");
        var texture = EditorGUIUtility.IconContent("CrossIcon").image;
        closeButton.focusable = false;
        closeButton.Add(new Image {
            image = texture,
        });
        closeButton.clicked += OnCloseButtonClicked;
    
        Button yesButton = this.Q<Button>("yes");
        yesButton.clicked += OnConfirmationClicked;
        Button noButton = this.Q<Button>("no");
        noButton.clicked += OnCloseButtonClicked;
    }

    public void UpdateModalText(string updtedText)
    {
        title.text = updtedText;
    }

    private void OnCloseButtonClicked()
    {
        title.text = defaultText;
        OnCloseModal?.Invoke();
    }

    private void OnConfirmationClicked()
    {
        title.text = defaultText; 
        OnConfirmModal?.Invoke();
    }
}

