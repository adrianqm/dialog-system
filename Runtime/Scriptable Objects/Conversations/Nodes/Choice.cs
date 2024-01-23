using System;
using System.Collections.Generic;
using Blackboard.Commands;
using Blackboard.Requirement;
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
        public RequirementsSO requirements;
        public CommandList actionList;

        public bool CheckConditions()
        {
            Requirements requirementsList = new Requirements(requirements);
            return requirements.conditions.Count == 0 || requirementsList.AreFulfilled;
        }

        public void OnSelected()
        {
            actionList.Execute();
        }

        public Choice Clone()
        {
            Choice choice = Instantiate(this);
            //choice.children = children.ConvertAll(c => c.Clone());
            return choice;
        }
        
#if UNITY_EDITOR
        public void Init(ChoiceNodeSO originNode, string defaultText)
        {
            guid = GUID.Generate().ToString();
            name = $"Choice-{guid}";
            choiceMessage = defaultText;
            //hideFlags = HideFlags.HideInHierarchy;
            port = originNode.CreateOutputPort();
            requirements = ScriptableObject.CreateInstance<RequirementsSO>();
            requirements.Init(guid);
            actionList = ScriptableObject.CreateInstance<CommandList>();
            actionList.Init(guid);
        }
        
        public void SaveAs(DialogSystemDatabase db)
        {
            AssetDatabase.AddObjectToAsset(this,db);
            AssetDatabase.AddObjectToAsset(requirements,db);
            foreach (ConditionSO cond in requirements.conditions)
                AssetDatabase.AddObjectToAsset(cond, db);
            AssetDatabase.AddObjectToAsset(actionList,db);
            foreach (Command command in actionList.commands)
                AssetDatabase.AddObjectToAsset(command, db);
            port.SaveAs(db);
        }
        
        private void OnDestroy()
        {
            Undo.DestroyObjectImmediate(requirements);
            foreach (ConditionSO cond in requirements.conditions)
                Undo.DestroyObjectImmediate(cond);
            Undo.DestroyObjectImmediate(actionList);
            foreach (Command c in actionList.commands)
                Undo.DestroyObjectImmediate(c);
        }
#endif
    }
}
