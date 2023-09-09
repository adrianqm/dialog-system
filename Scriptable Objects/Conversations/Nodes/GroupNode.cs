using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[System.Serializable]
public class GroupNode : ScriptableObject
{
    [HideInInspector] public string guid;
    [HideInInspector] public Vector2 position;
    public string title;
    
    public GroupNode Clone()
    {
        GroupNode node = Instantiate(this);
        return node;
    }
}
