using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    [System.Serializable]
    public class DialogNode : ParentNode
    {
        public Actor actor;
        [TextArea] public string message;

        public override DSNode GetData()
        {
            return new DSDialog(actor, message);
        }

        public override void OnRunning()
        {
            NodeState = State.Running;
        }
    }
}

