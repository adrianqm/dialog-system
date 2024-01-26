
using Blackboard.Commands;
using Blackboard.Requirement;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class ConversationNodeSO: NodeSO
    {
        public Actor actor;
        [TextArea] public string message;
        public float delayTime;
        public RequirementsSO requirements;
        public CommandList actionList;

        public override bool CheckConditions()
        {
            Requirements requirementsList = new Requirements(requirements);
            return requirements.conditions.Count == 0 || requirementsList.AreFulfilled;
        }

        public override void OnCompleteNode()
        {
            base.OnCompleteNode();
            actionList.Execute();
        }

        public override NodeSO Clone()
        {
            ConversationNodeSO node = Instantiate(this);
            return node;
        }
        
#if UNITY_EDITOR
        public override void Init(Vector2 position)
        {
            base.Init(position);
            requirements = ScriptableObject.CreateInstance<RequirementsSO>();
            requirements.Init(guid);
            actionList = ScriptableObject.CreateInstance<CommandList>();
            actionList.Init(guid);
        }
        
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
        
        public override void SaveAs(DialogSystemDatabase db)
        {
            base.SaveAs(db);
            AssetDatabase.AddObjectToAsset(requirements,db);
            AssetDatabase.SaveAssets();
            foreach (ConditionSO cond in requirements.conditions)
            {
                AssetDatabase.AddObjectToAsset(cond, db);
                AssetDatabase.SaveAssets();
            }
            AssetDatabase.AddObjectToAsset(actionList,db);
            AssetDatabase.SaveAssets();
            foreach (Command action in actionList.commands)
            {
                AssetDatabase.AddObjectToAsset(action, db);
                AssetDatabase.SaveAssets();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Undo.DestroyObjectImmediate(requirements);
            foreach (ConditionSO cond in requirements.conditions)
                Undo.DestroyObjectImmediate(cond);
            Undo.DestroyObjectImmediate(actionList);
            foreach (Command c in actionList.commands)
                Undo.DestroyObjectImmediate(c);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}