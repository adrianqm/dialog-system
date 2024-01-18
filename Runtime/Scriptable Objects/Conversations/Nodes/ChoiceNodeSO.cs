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
        
        public override DSNode GetData()
        {
            List<string> choicesList = new(); 
            foreach (var choice in choices)
            {
                if (choice.CheckConditions())
                {
                    choicesList.Add(choice.choiceMessage);
                }
            }
            return new DSChoice(actor,message,choicesList);
        }
        
        public override NodeSO Clone()
        {
            ChoiceNodeSO node = Instantiate(this);
            return node;
        }
        
#if UNITY_EDITOR
        
        public override void Init(Vector2 position)
        {
            base.Init(position);
            name = $"ChoiceNode-{guid}";
            CreateDefaultOutputPort();
        }
        
        private void CreateDefaultOutputPort()
        {
            defaultPort = CreateOutputPort();
        }
        
        protected override void CreateDefaultOutputPorts(){}
        
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
            AssetDatabase.SaveAssets();
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
            //EditorUtility.SetDirty(this);
            foreach (Choice choice in choices)
                Undo.DestroyObjectImmediate(choice);
            base.OnDestroy();
        }
#endif
    }
}

