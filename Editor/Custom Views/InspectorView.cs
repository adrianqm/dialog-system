
using AQM.Tools;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
  public new class UxmlFactory:  UxmlFactory<InspectorView, InspectorView.UxmlTraits> {}

  private NodeView _currentNodeView;
  private DialogInspectorView _currentContainer;
  
  internal void ShowDialogInspector(NodeView nodeView)
  {
      CreateDialogInspector(nodeView);
  }

  private void CreateDialogInspector(NodeView nodeView)
  {
      ClearInspector();
      DialogInspectorView container = new DialogInspectorView(nodeView);
      Add(container);
      _currentNodeView = nodeView;
      _currentContainer = container;
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
  
  public Node GetCurrentNode()
  {
      return _currentNodeView?.node;
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

