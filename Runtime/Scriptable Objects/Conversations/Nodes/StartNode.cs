using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class StartNode : ParentNode
    {
        public override void OnRunning()
        {
            Debug.Log("Start");
        }
    }
}
