using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableNode
{
    public string guid;
    public Vector2Serializable position;
    public List<SerializableNodeChild> children = new ();
    public SerializableGroupNode group;
}