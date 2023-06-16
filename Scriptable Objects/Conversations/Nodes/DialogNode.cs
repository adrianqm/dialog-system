using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogNode : Node
{
    [TextArea] public string message;

    public override void OnRunning()
    {
        NodeState = State.Running;
        Debug.Log(message);
    }
}
