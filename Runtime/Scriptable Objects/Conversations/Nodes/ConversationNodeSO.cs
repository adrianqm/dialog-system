
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class ConversationNodeSO: NodeSO
    {
        public Actor actor;
        [TextArea] public string message;
        public RequirementsSO requirements;
        
        public override NodeSO Clone()
        {
            ConversationNodeSO node = Instantiate(this);
            return node;
        }
        
#if UNITY_EDITOR
		public override void OnRunning()
        {
            NodeState = State.Running;
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
    }
#endif
}