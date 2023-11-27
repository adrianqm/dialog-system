using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class Choice : ScriptableObject
    {
        public string guid;
        public string choiceMessage;
        public List<Node> children = new();

        public Choice Clone()
        {
            Choice choice = Instantiate(this);
            //choice.children = children.ConvertAll(c => c.Clone());
            return choice;
        }
    }
}
