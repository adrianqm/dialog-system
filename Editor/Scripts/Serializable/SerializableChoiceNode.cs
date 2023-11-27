using System.Collections.Generic;

namespace AQM.Tools.Serializable
{
    [System.Serializable]
    public class SerializableChoiceNode : SerializableNode
    {
        public string actorGuid;
        public string message;
        public List<SerializableChoice> choices = new();
    }
}