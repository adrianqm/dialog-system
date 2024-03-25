using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    [System.Serializable]
    public abstract class NodeSO : ScriptableObject
    {
        [HideInInspector] public string guid;
        public Vector2 position;
        public GroupNode group;
        public BookmarkSO bookmark;
        public List<PortSO> inputPorts = new();
        public List<PortSO> outputPorts = new();
        
        public enum State
        {
            Initial,
            Running,
            Unreachable,
            Finished,
            Visited,
            VisitedUnreachable
        }
        
        private State _nodeState = State.Initial;
        public State NodeState
        {
            get => _nodeState;
            set => _nodeState = value;
        }
        
        public virtual bool CheckConditions() { return true; }
        public virtual void OnCompleteNode() {}
        
        public virtual DSNode GetData() { return null; }
        public abstract NodeSO Clone();
        
        public virtual void OnRunning() { NodeState = State.Running; }
        public virtual void OnCompleted() { NodeState = State.Finished; }
        
#if UNITY_EDITOR
        public virtual void Init(Vector2 position)
        {
            this.guid = GUID.Generate().ToString();
            this.position = position;
            //hideFlags = HideFlags.HideInHierarchy;

            CreateDefaultInputPorts();
            CreateDefaultOutputPorts();
        }
        
        protected virtual void CreateDefaultInputPorts()
        {
            var inputPort = PortFactory.Create("", this);
            inputPorts.Add(inputPort);
            EditorUtility.SetDirty(this);
        }
        
        protected virtual void CreateDefaultOutputPorts()
        {
            var outputPort = PortFactory.Create("", this);
            outputPorts.Add(outputPort);
            EditorUtility.SetDirty(this);
        }
        
        public void SetPosition(Rect newPos)
        {
            Undo.RecordObject(this, "Conversation Tree (Set Position)");
            position.x = newPos.xMin;
            position.y = newPos.yMin;
            EditorUtility.SetDirty(this);
        }
        
        public void SortAllOutputPorts()
        {
            foreach (var port in outputPorts)
            {
                port.SortPort();
            }
        }
        
        public void SortOutputPort(string portId)
        {
            var port = outputPorts.First(port => port.id == portId);
            if(!port) return;
            port.SortPort();
        }
        public void AddChild (string originPortId, NodeSO child)
        {
            var port = outputPorts.First(port => port.id == originPortId);
            port.AddTargetNode(child);
        }
        
        public void RemoveChild(string originPortName, NodeSO child)
        { 
            var port = outputPorts.First(port => port.id == originPortName);
            port.RemoveTargetNode(child);
        }
        
        public virtual void SaveAs(DialogSystemDatabase db)
        {
            AssetDatabase.AddObjectToAsset(this,db);
            
            foreach (PortSO inputPort in inputPorts)
                inputPort.SaveAs(db);
            foreach (PortSO outputPort in outputPorts)
                outputPort.SaveAs(db);
        }

        public virtual void OnDestroy()
        {
            foreach (PortSO inputPort in inputPorts)
            {
                Undo.DestroyObjectImmediate(inputPort);
            }
            
            foreach (PortSO outputPort in outputPorts)
            {
                Undo.DestroyObjectImmediate(outputPort);
            }
            EditorUtility.SetDirty(this);
        }
#endif
        
    }
}

