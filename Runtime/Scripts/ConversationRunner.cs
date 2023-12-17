
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace AQM.Tools
{
    public class ConversationRunner: MonoBehaviour, IInteractable
    {
        public ConversationTree conversationTree;
        public Transform lookAtPoint;
        private DialogSystemDatabase _clonedDatabase;

        public DialogSystemDatabase ClonedDatabase => _clonedDatabase;

        private void Awake()
        {
            if(conversationTree != null) DialogSystemController.onDatabaseCloned += OnDatabaseCloned;
        }

        private void OnDatabaseCloned(DialogSystemDatabase db)
        {
            _clonedDatabase = db;
            conversationTree = _clonedDatabase.FindConversation(conversationTree.guid);
        }

        public void Interact()
        {
            if (!_clonedDatabase || !conversationTree) return;
            DDEvents.onStartConversation?.Invoke(conversationTree);
            DDEvents.actorConversationStarted?.Invoke(lookAtPoint);
        }
    }
}