using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class BranchNodeSO : NodeSO
    {
        public BranchSO branch;
        public PortSO TrueOutputPort => outputPorts[0];
        public PortSO FalseOutputPort => outputPorts[1];

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
        public override NodeSO Clone()
        {
            BranchNodeSO nodeSo = Instantiate(this);
            return nodeSo;
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            Undo.DestroyObjectImmediate(branch);
        }
    }
}