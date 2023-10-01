using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootNode : ParentNode
{
    public override void OnRunning()
    {
        Debug.Log("Root");
    }
}
