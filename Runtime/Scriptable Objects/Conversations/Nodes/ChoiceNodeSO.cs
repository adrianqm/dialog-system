using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    [System.Serializable]
    public class ChoiceNodeSO : ConversationNodeSO
    {
        public List<Choice> choices = new ();
        public PortSO defaultPort;
        
        public override void Init(Vector2 position)
        {
            base.Init(position);
            name = $"ChoiceNode-{guid}";
            CreateDefaultOutputPort();
        }
        
        public override DSNode GetData()
        {
            List<string> choicesList = choices.ConvertAll(c => c.choiceMessage);
            return new DSChoice(actor,message,choicesList);
        }
        
        public override NodeSO Clone()
        {
            ChoiceNodeSO node = Instantiate(this);
            return node;
        }

        private void CreateDefaultOutputPort()
        {
            defaultPort = CreateOutputPort();
        }
        
        protected override void CreateDefaultOutputPorts(){}
#if UNITY_EDITOR
        
        public Choice CreateChoice(DialogSystemDatabase db,string defaultText = "")
        {
            Choice choice = ScriptableObject.CreateInstance(typeof(Choice)) as Choice;
            choice.Init(this, defaultText);
            
            Undo.RecordObject(this, "Choice Node (CreateChoice)");
            choices ??= new List<Choice>();
            choices.Add(choice);
            if (!Application.isPlaying)
                choice.SaveAs(db);
            
            Undo.RegisterCreatedObjectUndo(choice, "Choice Node (CreateChoice)");
            AssetDatabase.SaveAssets();
            return choice;
        }
        
        public void DeleteChoice( Choice choice)
        {
            Undo.RecordObject(this, "Choice Node (DeleteChoice)");
            choices.Remove(choice);
            RemoveOutputPort(choice.port);
            Undo.DestroyObjectImmediate(choice);
        }
        
        public PortSO CreateOutputPort()
        {
            var outputPort = PortFactory.Create("", this);
            outputPorts.Add(outputPort);
            EditorUtility.SetDirty(this);
            return outputPort;
        }
        
        public void RemoveOutputPort(PortSO port)
        {
            outputPorts.Remove(port);
            EditorUtility.SetDirty(this);
        }

        public override void OnDestroy()
        {
            EditorUtility.SetDirty(this);
            foreach (Choice choice in choices)
                Undo.DestroyObjectImmediate(choice);
            base.OnDestroy();
        }
#endif
    }
}

