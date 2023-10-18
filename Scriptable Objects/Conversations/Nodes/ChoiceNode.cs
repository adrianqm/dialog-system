using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ChoiceNode : Node
{
    public Actor actor;
    [TextArea] public string message;
    public List<Choice> choices = new ();
    public Action<int> onChoiceSelected;

    public Choice CreateChoice(DialogSystemDatabase db, string defaultText = "")
    {
        Choice node = CreateInstance(typeof(Choice)) as Choice;
        node.name = "Choice";
        node.guid = GUID.Generate().ToString();
        node.choiceMessage = defaultText;
        
        node.hideFlags = HideFlags.HideInHierarchy;
        Undo.RecordObject(this, "Conversation Tree (CreateNode)");
        choices ??= new List<Choice>();
        choices.Add(node);
        
        if (!Application.isPlaying)
        {
            AssetDatabase.AddObjectToAsset(node,db);
        }
        Undo.RegisterCreatedObjectUndo(node, "Choice Node (CreateChoice)");
        AssetDatabase.SaveAssets();
        return node;
    }
    
    public void DeteleChoice(Choice node)
    {
        Undo.RecordObject(this, "Choice Node (DeleteChoice)");
        choices.Remove(node);
        
        //AssetDatabase.RemoveObjectFromAsset(node);
        Undo.DestroyObjectImmediate(node);
        AssetDatabase.SaveAssets();
    }
    
    public override void OnRunning()
    {
        NodeState = State.Running;
    }
    
    public override Node Clone()
    {
        ChoiceNode node = Instantiate(this);
        //node.choices = choices.ConvertAll(c => c.Clone());
        return node;
    }
}
