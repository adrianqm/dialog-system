using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ConversationInspectorView : VisualElement
{
    private ConversationTree _conversationTree;
    
    public ConversationInspectorView(ConversationTree conversationTree)
    {
        string uriFile = "Assets/dialog-system/Editor/Custom Views/Conversation Inspector View/ConversationInspectorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);

        _conversationTree = conversationTree;
        if (_conversationTree != null)
        {
            BindTitle();
            BindDescription();
        }
    }

    private void BindTitle()
    {
        TextField titleLabel = this.Q<TextField>("titleTextField");
        titleLabel.bindingPath = "title";
        titleLabel.Bind(new SerializedObject(_conversationTree));
    }
    
    private void BindDescription()
    {
        TextField descLabel = this.Q<TextField>("descriptionTextField");
        descLabel.bindingPath = "description";
        descLabel.Bind(new SerializedObject(_conversationTree));
    }
}
