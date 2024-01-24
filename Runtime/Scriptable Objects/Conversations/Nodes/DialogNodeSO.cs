
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AQM.Tools
{
    public class DialogNodeSO : ConversationNodeSO
    {
        public override DSNode GetData()
        {
            return new DSDialog(actor, message, delayTime);
        }
        
#if UNITY_EDITOR
        public override void Init(Vector2 position)
        {
            base.Init(position);
            name = $"DialogNode-{guid}";
        }
        
        protected override void CreateDefaultOutputPorts()
        {
            var outputPort = PortFactory.Create("", this);
            outputPorts.Add(outputPort);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}

