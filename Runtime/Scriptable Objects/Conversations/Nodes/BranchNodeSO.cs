using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class BranchNodeSO : NodeSO
    {
        public BranchSO branch;
        public PortSO TrueOutputPort => outputPorts[0];
        public PortSO FalseOutputPort => outputPorts[1];
        
        public override NodeSO Clone()
        {
            BranchNodeSO nodeSo = Instantiate(this);
            return nodeSo;
        }
        
#if UNITY_EDITOR
        public override void Init(Vector2 position)
        {
            base.Init(position);
            name = $"BranchNode-{guid}";
            branch = ScriptableObject.CreateInstance<BranchSO>();
            branch.Init(guid);
        }

        protected override void CreateDefaultOutputPorts()
        {
            var trueOutputPort = PortFactory.Create("True", this);
            outputPorts.Add(trueOutputPort);
            
            var falseOutputPort = PortFactory.Create("False", this);
            outputPorts.Add(falseOutputPort);
        }

        public override void SaveAs(DialogSystemDatabase db)
        {
            base.SaveAs(db);
            branch.SaveAs(db);
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
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            Undo.DestroyObjectImmediate(branch);
        }
#endif
    }
}