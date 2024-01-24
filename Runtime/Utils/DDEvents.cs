using System;
using AQM.Tools;
using UnityEngine;

namespace AQM.Tools
{
    public static class DDEvents
    {
        public static Action<ConversationTree> onStartConversation;
        public static Action<int> onGetNextNode;
        public static Action<Transform> actorConversationStarted;
    }
}
