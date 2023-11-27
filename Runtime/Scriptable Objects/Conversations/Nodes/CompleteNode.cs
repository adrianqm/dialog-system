using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class CompleteNode : Node
    {
        public override void OnRunning()
        {
            Debug.Log("Complete");
        }
        public override Node Clone()
        {
            CompleteNode node = Instantiate(this);
            return node;
        }
    }
}