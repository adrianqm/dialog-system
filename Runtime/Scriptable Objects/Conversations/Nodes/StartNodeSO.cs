using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class StartNodeSO : NodeSO
    {
        public override NodeSO Clone()
        {
            StartNodeSO nodeSo = Instantiate(this);
            return nodeSo;
        }
        
#if UNITY_EDITOR
        
        public override void Init(Vector2 position)
        {
            base.Init(position);
            name = $"StartNode-{guid}";
        }
        
        protected override void CreateDefaultInputPorts(){}

        protected override void CreateDefaultOutputPorts()
        {
            var outputPort = PortFactory.Create("", this);
            outputPorts.Add(outputPort);
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
#endif
    }
}
