using System;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class BranchSO: ScriptableObject
    {
        public RequirementsSO requirements;
        
        public bool CheckConditions()
        {
            Requirements requirementsList = new Requirements(requirements);
            return requirementsList.CheckRequirementsGoal();
        }
        
#if UNITY_EDITOR
        public void Init(string guid)
        {
            name = $"Branch-{guid}";
            requirements = ScriptableObject.CreateInstance<RequirementsSO>();
            requirements.Init(guid);
        }
        
        public void SaveAs(DialogSystemDatabase db)
        {
            AssetDatabase.AddObjectToAsset(this, db);
            AssetDatabase.AddObjectToAsset(requirements, db);
            
            foreach (ConditionSO cond in requirements.conditions)
                AssetDatabase.AddObjectToAsset(cond, db);
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