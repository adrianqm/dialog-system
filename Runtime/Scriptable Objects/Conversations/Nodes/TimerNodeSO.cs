using System;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class TimerNodeSO : NodeSO
    {
        public float time;
        public float timeRemaining;
        public PortSO outputPort;

        public override void OnInitial()
        {
            base.OnInitial();
            timeRemaining = 0;
        }

        public override NodeSO Clone()
        {
            TimerNodeSO nodeSo = Instantiate(this);
            return nodeSo;
        }
        
#if UNITY_EDITOR
        public override void Init(Vector2 position)
        {
            base.Init(position);
            name = $"TimerNode-{guid}";
            outputPort = outputPorts[0];
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
                    timeRemaining = 0;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    timeRemaining = 0;
                    break;
            }
        }
#endif
    }
}