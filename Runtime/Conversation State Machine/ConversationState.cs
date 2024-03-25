using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public abstract class ConversationState : BaseState<ConversationStateMachine.EConversationState>
    {
        protected ConversationContext context;

        protected ConversationState(ConversationContext context, ConversationStateMachine.EConversationState key) : base(key)
        {
            this.context = context;
        }

        public virtual void GetNextNode(int option = 0) {}

        public virtual DSNode GetCurrentData()
        {
            return null;
        }
    }
}
