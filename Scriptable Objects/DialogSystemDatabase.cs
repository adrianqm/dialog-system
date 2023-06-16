using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog System Database", menuName = "AQM/Tools/Dialog System/Dialog System Database", order = 1)]
public class DialogSystemDatabase : ScriptableObject
{
    public string title;
    public string description;
    public List<ConversationTree> conversations;
    public ActorsTree actorsTree;
}
