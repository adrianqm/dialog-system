using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class BookmarkState : ConversationState
    {
        private readonly ConversationStateMachine _stateMachine;
        private BookmarkNodeSO _currentNode;

        public BookmarkState(ConversationContext context, ConversationStateMachine.EConversationState key) : base(context, key)
        {
            _stateMachine = context.ConversationStateMachine;
        }

        public override void EnterState()
        {
            _currentNode = context.CurrentNode as BookmarkNodeSO;
            GetNextNode();
        }

        public override void GetNextNode(int option = 0)
        {
            NodeSO nextNode = null;
            if(_currentNode.bookmark.goToNode != null)
            {
                nextNode = _currentNode.bookmark.goToNode;
            }
            _stateMachine.TransitionToNode(nextNode);
        }
    }
}
