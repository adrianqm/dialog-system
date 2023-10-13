
using System;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace AQM.Tools
{
    public class ConversationRunner : InteractableBase
    {
        public static Action<ConversationTree> onStartConversation;
        
        public ConversationTree conversationTree;

        private DialogSystemDatabase _clonedDatabase;

        private void Awake()
        {
            DialogSystemController.onDatabaseCloned += OnDatabaseCloned;
        }

        private void OnDatabaseCloned(DialogSystemDatabase db)
        {
            _clonedDatabase = db;
        }

        public override void Interact()
        {
            if (_clonedDatabase)
            {
                conversationTree = _clonedDatabase.conversations[1];
                if (conversationTree)
                {
                    onStartConversation?.Invoke(conversationTree);
                } 
            }
        }
    }
}