using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class BranchState : ConversationState
    {
        private readonly ConversationStateMachine _stateMachine;
        private BranchNodeSO _currentNode;

        public BranchState(ConversationContext context, ConversationStateMachine.EConversationState key) : base(context, key)
        {
            _stateMachine = context.ConversationStateMachine;
        }

        public override void EnterState()
        {
            _currentNode = context.CurrentNode as BranchNodeSO;
            GetNextNode();
        }

        public override void GetNextNode(int option = 0)
        {
            NodeSO nextNode;
            if(_currentNode.branch.CheckConditions())
            {
                nextNode = _currentNode.TrueOutputPort.targetNodes[0];
            }else
            {
                nextNode = _currentNode.FalseOutputPort.targetNodes[0];
            }
            _stateMachine.TransitionToNode(nextNode);
        }
    }
}
