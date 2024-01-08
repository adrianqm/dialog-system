
using Blackboard.Actions;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class ConversationNodeSO: NodeSO
    {
        public Actor actor;
        [TextArea] public string message;
        public RequirementsSO requirements;
        public ActionList actionList;

        public override void Init(Vector2 position)
        {
            base.Init(position);
            requirements = ScriptableObject.CreateInstance<RequirementsSO>();
            requirements.Init(guid);
            actionList = ScriptableObject.CreateInstance<ActionList>();
        }

        public override bool CheckConditions()
        {
            Requirements requirementsList = new Requirements(requirements);
            return requirementsList.CheckRequirementsGoal();
        }

        public override void SaveAs(DialogSystemDatabase db)
        {
            base.SaveAs(db);
            AssetDatabase.AddObjectToAsset(requirements,db);
            foreach (ConditionSO cond in requirements.conditions)
                AssetDatabase.AddObjectToAsset(cond, db);
            foreach (Action action in actionList.actions)
                AssetDatabase.AddObjectToAsset(action, db);
        }

        public override NodeSO Clone()
        {
            ConversationNodeSO node = Instantiate(this);
            return node;
        }
        
#if UNITY_EDITOR
      	private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    NodeState = State.Initial;
                    break;
                case PlayModeStateChange.EnteredPlayMode: break;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Undo.DestroyObjectImmediate(requirements);
            foreach (ConditionSO cond in requirements.conditions)
                Undo.DestroyObjectImmediate(cond);
            EditorUtility.SetDirty(this);
        }
    }
#endif
}