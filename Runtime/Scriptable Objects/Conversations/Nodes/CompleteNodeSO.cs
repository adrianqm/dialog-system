using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class CompleteNodeSO : NodeSO
    {
        public override void Init(Vector2 position)
        {
            base.Init(position);
            name = $"CompleteNode-{guid}";
        }
        
        public override NodeSO Clone()
        {
            CompleteNodeSO nodeSo = Instantiate(this);
            return nodeSo;
        }

        protected override void CreateDefaultOutputPorts(){}
        
#if UNITY_EDITOR
        public override void OnRunning()
        {
            Debug.Log("Complete");
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
        
        public override void OnDestroy() {}
#endif
    }
}
