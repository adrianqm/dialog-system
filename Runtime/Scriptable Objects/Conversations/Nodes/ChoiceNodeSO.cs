using System;
using System.Collections;
using System.Collections.Generic;
using Blackboard.Commands;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    [System.Serializable]
    public class ChoiceNodeSO : ConversationNodeSO
    {
        public List<Choice> choices = new ();
        public PortSO defaultPort;
        public CommandList defaultActionList;

        public void OnDefaultSelected()
        {
            defaultActionList.Execute();
        }
        
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
            defaultActionList = ScriptableObject.CreateInstance<CommandList>();
            defaultActionList.Init(guid);
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
        
        public override void SaveAs(DialogSystemDatabase db)
        {
            base.SaveAs(db);
            AssetDatabase.AddObjectToAsset(defaultActionList,db);
            foreach (Command action in defaultActionList.commands)
                AssetDatabase.AddObjectToAsset(action, db);
        }

        public override void OnDestroy()
        {
            //EditorUtility.SetDirty(this);
            foreach (Choice choice in choices)
                Undo.DestroyObjectImmediate(choice);
            
            Undo.DestroyObjectImmediate(defaultActionList);
            foreach (Command c in defaultActionList.commands)
                Undo.DestroyObjectImmediate(c);
            base.OnDestroy();
        }
#endif
    }
}

