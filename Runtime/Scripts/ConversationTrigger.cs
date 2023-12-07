using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEngine;

public class ConversationTrigger: MonoBehaviour
{
    public ConversationTree triggerConversation;
    public Transform lookAtPoint;

    private DialogSystemDatabase _clonedDatabase;

    public DialogSystemDatabase ClonedDatabase => _clonedDatabase;

    private void Awake()
    {
        DialogSystemController.onDatabaseCloned += OnDatabaseCloned;
    }
    
    private void OnDatabaseCloned(DialogSystemDatabase db)
    {
        _clonedDatabase = db;
        //triggerConversation = _clonedDatabase.conversations.Find(c => c.guid == triggerConversation.guid);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (!_clonedDatabase ||!triggerConversation) return;
        DDEvents.onStartConversation?.Invoke(triggerConversation);
        DDEvents.actorConversationStarted?.Invoke(lookAtPoint);
    }
}
