using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
    public new class UxmlFactory:  UxmlFactory<InspectorView, InspectorView.UxmlTraits> {}

    internal void ShowDialogInspector(NodeView nodeView,List<Actor> actors)
    {
        Clear();
        DialogInspectorView container = new DialogInspectorView(nodeView, actors);
        Add(container);
    }

    internal void ShowConversationInspector(ConversationTree conversationTree)
    {
        Clear();
        ConversationInspectorView container = new ConversationInspectorView(conversationTree);
        Add(container);
    }
    
    internal void ClearInspector()
    {
        Clear();
    }
}
