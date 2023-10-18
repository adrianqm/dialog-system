using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationSearchProvider : DataSearchProvider<ConversationTree>
{
    public override void Init(List<KeyValuePair<ConversationTree, string>> quests, Action<ConversationTree> callback)
    {
        base.Init(quests, callback);

        searchTreeTitle = "Conversations";
    }
}
