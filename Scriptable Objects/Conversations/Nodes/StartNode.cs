using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartNode : ParentNode
{
    public override void OnRunning()
    {
        Debug.Log("Start");
    }
}
