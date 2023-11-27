using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    [System.Serializable]
    public class ParentNode : Node
    {
        [HideInInspector] public List<Node> children = new();

        public override void OnRunning()
        {
            NodeState = State.Running;
        }

        public override Node Clone()
        {
            ParentNode node = Instantiate(this);
            return node;
        }
    }
}
