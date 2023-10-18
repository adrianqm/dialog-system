using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ConversationsView : VisualElement
{
    public new class UxmlFactory:  UxmlFactory<ConversationsView, ConversationsView.UxmlTraits> {}
    
    public ConversationsView()
    {
        string uriFile = "Assets/dialog-system/Editor/Custom Views/Conversations View/ConversationsView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);
    }
}
