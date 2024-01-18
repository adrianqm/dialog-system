
using AQM.Tools;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
  public new class UxmlFactory:  UxmlFactory<InspectorView, InspectorView.UxmlTraits> {}

  private NodeView _currentNodeView;
  private VisualElement _currentContainer;
  private ConversationTree _currentConversationTree;
  
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
      _currentContainer = container;
      _currentConversationTree = conversationTree;
  }
  
  internal void ShowBookmarksInspector(ConversationTree conversationTree)
  {
      ClearInspector();
      BookmarksInspectorView container = new BookmarksInspectorView(conversationTree);
      Add(container);
      ResetDialogContainerValues();
      _currentContainer = container;
      _currentConversationTree = conversationTree;
  }

  private void ResetDialogContainerValues()
  {
      _currentNodeView = null;
  }

  public void RefreshCurrentInspector()
  {
      switch (_currentContainer)
      {
          case DialogInspectorView: ShowDialogInspector(_currentNodeView);
              break;
          case BookmarksInspectorView: ShowBookmarksInspector(_currentConversationTree);
              break;
          case ConversationInspectorView: ShowConversationInspector(_currentConversationTree);
              break;
      }
  }
  
  public NodeSO GetCurrentNode()
  {
      return _currentNodeView?.node;
  }
  
  internal void ClearInspector()
  {
      if (_currentContainer != null && _currentContainer is DialogInspectorView dialogInspector)
      {
          dialogInspector.ClearViewCallbacks();
      }
      Clear();
  }
}

