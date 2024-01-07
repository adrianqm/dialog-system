using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools.Serializable;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace AQM.Tools
{
    public static class ConversationUtils
    {
        public static GroupNode CreateGroup (DialogSystemDatabase db, ConversationTree tree, string groupTitle, Vector2 position)
        {
            GroupNode groupNode = ScriptableObject.CreateInstance(typeof(GroupNode)) as GroupNode;
            if (!groupNode) return null;
            
            groupNode.name = groupTitle;
            groupNode.title = groupTitle;
            groupNode.guid = GUID.Generate().ToString();
            groupNode.position = position;
            groupNode.hideFlags = HideFlags.HideInHierarchy;
            
            Undo.RecordObject(tree, "Conversation Tree (CreateGroup)");
            tree.groups ??= new List<GroupNode>();
            tree.groups.Add(groupNode);
            
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(groupNode,db);
            }
            Undo.RegisterCreatedObjectUndo(groupNode, "Conversation Tree (CreateGroup)");
            AssetDatabase.SaveAssets();
            return groupNode;
        }
        
        public static void DeleteGroupNode(ConversationTree tree, GroupNode groupNode)
        {
            Undo.RecordObject(tree, "Conversation Tree (DeleteGroupNode)");
            tree.groups.Remove(groupNode);
            Undo.DestroyObjectImmediate(groupNode);
            AssetDatabase.SaveAssets();
        }
        
        public static bool AddGroupToNode(NodeSO node,GroupNode group)
        {
            bool hasBeenAdded = false;
            if(node  && group)
            {
                Undo.RecordObject(node, "Conversation Tree (AddGroupToNode)");
                node.group = group;
                EditorUtility.SetDirty(node);
                hasBeenAdded = true;
            }

            return hasBeenAdded;
        }
        public static void RemoveGroupFromNode(NodeSO node)
        {
            if (!node) return;
            Undo.RecordObject(node, "Conversation Tree (RemoveGroupFromNode)");
            node.group = null;
            EditorUtility.SetDirty(node);
        }
        
        
        private static void AddNodeToList(DialogSystemDatabase db,ConversationTree tree, NodeSO node)
        {
            
        }
        
        public static DialogNodeSO CreateDialogNodeCopy(DialogSystemDatabase db,ConversationTree tree,  System.Type type, Vector2 position, SerializableDialogNode nodeToCopy)
        {
            DialogNodeSO node = ScriptableObject.CreateInstance(type) as DialogNodeSO;
            if (!node) return null;
            
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node.position = position;
            node.actor = db.actors.Find(actor => actor.guid == nodeToCopy.actorGuid);
            node.message = nodeToCopy.message;

            AddNodeToList(db,tree,node);
#if LOCALIZATION_EXIST
            LocalizationUtils.AddCopyKeyToCollection(node.guid,nodeToCopy.guid);
#endif
            return node;
        }
        
        public static ChoiceNodeSO CreateChoiceNodeToCopy(DialogSystemDatabase db,ConversationTree tree, Vector2 position, SerializableChoiceNode nodeToCopy, Dictionary<string, Choice> choiceMap)
        {
            Type type = typeof(ChoiceNodeSO);
            ChoiceNodeSO node = ScriptableObject.CreateInstance(type) as ChoiceNodeSO;
            if (!node) return null;
            
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node.position = position;
            node.actor = db.actors.Find(actor => actor.guid == nodeToCopy.actorGuid);
            node.message = nodeToCopy.message;
            node.choices = new List<Choice>();
            foreach (var serializableChoice in nodeToCopy.choices)
            {
                //Choice choice = ChoiceUtils.CreateChoice(db, node, serializableChoice.choiceMessage);
                //choiceMap.Add(serializableChoice.guid, choice);
            }

            AddNodeToList(db,tree,node);
#if LOCALIZATION_EXIST
            LocalizationUtils.AddCopyKeyToCollection(node.guid,nodeToCopy.guid);
#endif
            return node;
        }
        
    }
}
