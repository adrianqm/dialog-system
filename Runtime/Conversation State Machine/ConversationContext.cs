using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class ConversationContext 
    {
        public DialogSystemDatabase Database { get; }
        public ConversationTree Conversation { get; }
        public ConversationStateMachine ConversationStateMachine { get; }
        public NodeSO CurrentNode { get; set; }

        public ConversationContext(ConversationStateMachine conversationStateMachine, DialogSystemDatabase db, ConversationTree conversation)
        {
            ConversationStateMachine = conversationStateMachine;
            Conversation = conversation;
            Database = db;
        }
    }  
}
