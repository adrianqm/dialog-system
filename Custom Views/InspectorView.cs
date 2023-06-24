using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
    public new class UxmlFactory:  UxmlFactory<InspectorView, InspectorView.UxmlTraits> {}

    internal void ShowDialogInspector(NodeView nodeView,ActorsTree actorsTree)
    {
        Clear();
        DialogInspectorView container = new DialogInspectorView(nodeView, actorsTree);
        Add(container);
    }

    internal void ShowConversationInspector(ConversationTree conversationTree)
    {
        Clear();
        ConversationInspectorView container = new ConversationInspectorView(conversationTree);
        Add(container);
    }
}
