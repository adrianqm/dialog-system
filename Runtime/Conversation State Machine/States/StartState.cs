using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class StartState : ConversationState
    {
        private readonly ConversationStateMachine _stateMachine;
        private StartNodeSO _currentNode;
        
        public StartState(ConversationContext context, ConversationStateMachine.EConversationState key) : base(context, key)
        {
           _stateMachine = context.ConversationStateMachine;
        }

        public override void EnterState()
        {
            _stateMachine.onConversationStarted.Invoke();
            _currentNode = context.CurrentNode as StartNodeSO;
            GetNextNode();
        }

        public override void GetNextNode(int option = 0)
        {
            NodeSO nextNode = _currentNode.outputPort.targetNodes[0];
            _stateMachine.TransitionToNode(nextNode);
        }
    }
}
