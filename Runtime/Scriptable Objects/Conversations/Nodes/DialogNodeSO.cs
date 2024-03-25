
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AQM.Tools
{
    public class DialogNodeSO : ConversationNodeSO
    {
        public PortSO outputPort;
        
        public override DSNode GetData()
        {
            return new DSDialog(actor, message);
        }
        
#if UNITY_EDITOR
        public override void Init(Vector2 position)
        {
            base.Init(position);
            name = $"DialogNode-{guid}";
        }
        
        protected override void CreateDefaultOutputPorts()
        {
            outputPort = PortFactory.Create("", this);
            outputPorts.Add(outputPort);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}

