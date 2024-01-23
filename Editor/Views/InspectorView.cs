
using System;
using AQM.Tools;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
  public new class UxmlFactory:  UxmlFactory<InspectorView, InspectorView.UxmlTraits> {}
  
  private NodeView _currentNodeView;
  private VisualElement _currentContainer;
  private ConversationTree _currentConversationTree;
  private DialogSystemView _graphView;
  
  internal void ShowDialogInspector(NodeView nodeView)
  {
      CreateDialogInspector(nodeView);
  }

  private void CreateDialogInspector(NodeView nodeView)
  {
      ClearInspector();
      if (nodeView is BranchNodeView)
      {
          BranchNodeSO branchNode = nodeView.node as BranchNodeSO;
          if (branchNode)
          {
              BranchNodeInspectorView container = new BranchNodeInspectorView(branchNode);
              Add(container);
              _currentNodeView = nodeView;
              _currentContainer = container;
          }
      }else
      {
          DialogInspectorView container = new DialogInspectorView(nodeView);
          Add(container);
          _currentNodeView = nodeView;
          _currentContainer = container;
      }
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
  
  internal void ShowBookmarksInspector(DialogSystemView graphView, ConversationTree conversationTree)
  {
      ClearInspector();
      BookmarksInspectorView container = new BookmarksInspectorView(graphView, conversationTree);
      Add(container);
      ResetDialogContainerValues();
      _currentContainer = container;
      _currentConversationTree = conversationTree;
      _graphView = graphView;
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
          case ConversationInspectorView: ShowConversationInspector(_currentConversationTree);
              break;
          case BookmarksInspectorView: ShowBookmarksInspector(_graphView, _currentConversationTree);
              break;
      }
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

