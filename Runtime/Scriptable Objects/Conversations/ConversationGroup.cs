using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class ConversationGroup: ScriptableObject
    {
        public string name;
        public List<ConversationGroup> groups;
        public List<ConversationTree> conversations;

        public ConversationGroup(string name,List<ConversationTree> conversations, List<ConversationGroup> groups)
        {
            this.name = name;
            this.groups = groups;
            this.conversations = conversations;
        }
    }
}