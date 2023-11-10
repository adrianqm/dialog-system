using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
    public new class UxmlFactory:  UxmlFactory<InspectorView, InspectorView.UxmlTraits> {}

    private NodeView _currentNodeView;
    private DialogInspectorView _currentContainer;
    
    internal void ShowDialogInspector(NodeView nodeView,List<Actor> actors)
    {
        
        if (nodeView.node != _currentNodeView?.node)
        {
            ClearInspector();
            DialogInspectorView container = new DialogInspectorView(nodeView, actors);
            Add(container);
            _currentNodeView = nodeView;
            _currentContainer = container;
        }
        else
        {
            _currentContainer.SoftUpdate(nodeView, actors);
        }
        
    }

    internal void ShowConversationInspector(ConversationTree conversationTree)
    {
        ClearInspector();
        ConversationInspectorView container = new ConversationInspectorView(conversationTree);
        Add(container);
        ResetDialogContainerValues();

    }

    private void ResetDialogContainerValues()
    {
        _currentNodeView = null;
        _currentContainer = null;
    }
    
    internal void ClearInspector()
    {
        if (_currentContainer != null)
        {
            _currentContainer.ClearViewCallbacks();
        }
        Clear();
    }
}
