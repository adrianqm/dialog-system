using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogNode : Node
{
    public Actor actor;
    [TextArea] public string message;

    public override void OnRunning()
    {
        NodeState = State.Running;
        Debug.Log(message);
    }
}
