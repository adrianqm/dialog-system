using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEngine;

public class ConversationTrigger: MonoBehaviour
{
    public ConversationTree triggerConversation;
    public Transform lookAtPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (!triggerConversation) return;
        DDEvents.onStartConversation?.Invoke(triggerConversation);
        DDEvents.actorConversationStarted?.Invoke(lookAtPoint);
    }
}
