using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    [System.Serializable]
    public class ChoiceNode : Node
    {
        public Actor actor;
        [TextArea] public string message;
        public List<Choice> choices = new ();
        public List<Node> defaultChildren = new ();
        
        public override void OnRunning()
        {
            NodeState = State.Running;
        }
        
        public override DSNode GetData()
        {
            List<string> choicesList = choices.ConvertAll(c => c.choiceMessage);
            return new DSChoice(actor,message,choicesList);
        }
        
        public override Node Clone()
        {
            ChoiceNode node = Instantiate(this);
            //node.choices = choices.ConvertAll(c => c.Clone());
            return node;
        }
    }
}

