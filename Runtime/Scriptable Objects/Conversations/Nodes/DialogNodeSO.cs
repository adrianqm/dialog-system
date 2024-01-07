
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AQM.Tools
{
    [System.Serializable]
    public class DialogNodeSO : ConversationNodeSO
    {
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
        
        public override DSNode GetData()
        {
            return new DSDialog(actor, message);
        }
    }
}

