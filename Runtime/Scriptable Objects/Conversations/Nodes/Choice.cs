using System;
using System.Collections.Generic;
using Blackboard.Actions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Action = Blackboard.Actions.Action;

namespace AQM.Tools
{
    public class Choice : ScriptableObject
    {
        public string guid;
        public string choiceMessage;
        public PortSO port;
        public RequirementsSO requirements;
        public ActionList actionList;

        public bool CheckConditions()
        {
            Requirements requirementsList = new Requirements(requirements);
            return requirementsList.CheckRequirementsGoal();
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
            actionList = ScriptableObject.CreateInstance<ActionList>();
        }
        
        public void SaveAs(DialogSystemDatabase db)
        {
            AssetDatabase.AddObjectToAsset(this,db);
            AssetDatabase.AddObjectToAsset(requirements,db);
            foreach (ConditionSO cond in requirements.conditions)
                AssetDatabase.AddObjectToAsset(cond, db);
            foreach (Action action in actionList.actions)
                AssetDatabase.AddObjectToAsset(action, db);
            port.SaveAs(db);
        }
        
        private void OnDestroy()
        {
            Undo.DestroyObjectImmediate(requirements);
            foreach (ConditionSO cond in requirements.conditions)
                Undo.DestroyObjectImmediate(cond);
        }
#endif
    }
}
