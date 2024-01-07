using System.Collections.Generic;

namespace AQM.Tools.Serializable
{
    [System.Serializable]
    public class SerializableDialogNode : SerializableNode
    {
        public string actorGuid;
        public string message;
        public List<SerializableNodeChild> children = new ();
    }
}