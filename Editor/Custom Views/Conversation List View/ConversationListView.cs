using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ConversationListView : VisualElement
{
    public new class UxmlFactory:  UxmlFactory<ConversationListView, ConversationListView.UxmlTraits> {}
    
    public ConversationListView()
    {
        string uriFile = "Assets/dialog-system/Editor/Custom Views/Conversation List View/ConversationListView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);
    }
}
