using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public static class ChoiceUtils
    {
        public static Choice CreateChoice(DialogSystemDatabase db, ChoiceNode choiceNode, string defaultText = "")
        {
            Choice node = ScriptableObject.CreateInstance(typeof(Choice)) as Choice;
            if (!node) return null;
            
            node.name = "Choice";
            node.guid = GUID.Generate().ToString();
            node.choiceMessage = defaultText;
            
            node.hideFlags = HideFlags.HideInHierarchy;
            Undo.RecordObject(choiceNode, "Conversation Tree (CreateNode)");
            choiceNode.choices ??= new List<Choice>();
            choiceNode.choices.Add(node);
#if LOCALIZATION_EXIST
            LocalizationUtils.SetDefaultLocaleEntry(node.guid,defaultText);
#endif
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node,db);
            }
            Undo.RegisterCreatedObjectUndo(node, "Choice Node (CreateChoice)");
            AssetDatabase.SaveAssets();
            return node;
        }
        
        public static void DeleteChoice(ChoiceNode choiceNode, Choice node)
        {
            Undo.RecordObject(choiceNode, "Choice Node (DeleteChoice)");
            choiceNode.choices.Remove(node);
            
#if LOCALIZATION_EXIST
            LocalizationUtils.RemoveKeyFromCollection(node.guid);
#endif
            Undo.DestroyObjectImmediate(node);
            //AssetDatabase.SaveAssets();
        }
    }
}

