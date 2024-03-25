using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class IdleState : ConversationState
    {
        public IdleState(ConversationContext context, ConversationStateMachine.EConversationState key) : base(context, key)
        {}
    }
}
