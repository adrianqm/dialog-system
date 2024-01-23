
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace AQM.Tools
{
    public class ConversationRunner: MonoBehaviour, IInteractable
    {
        public ConversationTree conversation;
        public Transform lookAtPoint;

        public void Interact()
        {
            if (!conversation) return;
            DDEvents.onStartConversation?.Invoke(conversation);
            DDEvents.actorConversationStarted?.Invoke(lookAtPoint);
            InputManager.Instance.ToogleActionMap(InputManager.Instance.playerInputActions.UI);
        }
    }
}