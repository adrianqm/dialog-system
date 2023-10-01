using System.Collections.Generic;
using UnityEngine;

public class Choice : ScriptableObject
{
    public string guid;
    public string choiceMessage;
    public List<Node> children = new();
}
