using System.Collections.Generic;

namespace AQM.Tools.Serializable
{
    public class SerializableChoice
    {
        public string guid;
        public string choiceMessage;
        public List<SerializableNodeChild> children = new();
    }
}