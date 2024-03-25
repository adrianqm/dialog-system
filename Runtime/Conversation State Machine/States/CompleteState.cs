using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class CompleteState : ConversationState
    {
        private readonly ConversationStateMachine _stateMachine;
        
        public CompleteState(ConversationContext context, ConversationStateMachine.EConversationState key) : base(context, key)
        {
            _stateMachine = context.ConversationStateMachine;
        }

        public override void EnterState()
        {
            _stateMachine.FinalizeConversation();
        }
    }
}
