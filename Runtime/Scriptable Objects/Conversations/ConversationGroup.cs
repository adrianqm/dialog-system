using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class ConversationGroup: ScriptableObject
    {
        public string guid;
        public string title;
        public List<ConversationGroup> groups;
        public List<ConversationTree> conversations;
        
        public ConversationGroup Clone()
        {
            ConversationGroup clone = Instantiate(this);

            foreach (var group in groups)
            {
                clone.groups.Add(group.Clone());
            }
            
            clone.conversations = new List<ConversationTree>();
            conversations.ForEach((conversation) =>
            {
                ConversationTree clonedConversation = conversation.Clone();
                clone.conversations.Add(clonedConversation);
            });

            return clone;
        }
    }
}