using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
    public new class UxmlFactory:  UxmlFactory<InspectorView, InspectorView.UxmlTraits> {}

    private Editor editor;
    public InspectorView() {}

    internal void UpdateSelection(NodeView nodeView)
    {
        ClearInspector();
        editor = Editor.CreateEditor(nodeView.node);
        IMGUIContainer container = new IMGUIContainer(() =>
        {
            if (editor.target != null)
            {
                editor.OnInspectorGUI();
            }
        });
        Add(container);
    }

    internal void UpdateWithConversationName(ConversationTree conversationTree)
    {
        ClearInspector();
        editor = Editor.CreateEditor(conversationTree);
        IMGUIContainer container = new IMGUIContainer(() =>
        {
            if (editor.target != null)
            {
                editor.OnInspectorGUI();
            }
        });
        Add(container);
    }

    private void ClearInspector()
    {
        Clear();
        Object.DestroyImmediate(editor);
    }
}
