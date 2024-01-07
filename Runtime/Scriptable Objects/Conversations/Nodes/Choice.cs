using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace AQM.Tools
{
    public class Choice : ScriptableObject
    {
        public string guid;
        public string choiceMessage;
        public PortSO port;

        public void Init(ChoiceNodeSO originNode, string defaultText)
        {
            guid = GUID.Generate().ToString();
            name = $"Choice-{guid}";
            choiceMessage = defaultText;
            //hideFlags = HideFlags.HideInHierarchy;
            port = originNode.CreateOutputPort();
        }
        
        public void SaveAs(DialogSystemDatabase db)
        {
            AssetDatabase.AddObjectToAsset(this,db);
            port.SaveAs(db);
        }
        
        public Choice Clone()
        {
            Choice choice = Instantiate(this);
            //choice.children = children.ConvertAll(c => c.Clone());
            return choice;
        }
    }
}
