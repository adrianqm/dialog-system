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
        public static ConversationTree CreateConversation(DialogSystemDatabase db, string title, string description)
        {
            ConversationTree tree = ScriptableObject.CreateInstance(typeof(ConversationTree)) as ConversationTree;
            if (!tree) return null;
            tree.title = title;
            tree.description = description;
            tree.guid = GUID.Generate().ToString();
            tree.name = title;
            tree.hideFlags = HideFlags.HideInHierarchy;
            
            AssetDatabase.AddObjectToAsset(tree,db);
            return tree;
        }
        
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
        
        public static bool AddGroupToNode(Node node,GroupNode group)
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
        public static void RemoveGroupFromNode(Node node)
        {
            if (!node) return;
            Undo.RecordObject(node, "Conversation Tree (RemoveGroupFromNode)");
            node.group = null;
            EditorUtility.SetDirty(node);
        }
        
        public static void SetGroupTitle(GroupNode group, string newTitle)
        {
            if (!group) return;
            Undo.RecordObject(group, "Conversation Tree (SetGroupTitle)");
            group.title = newTitle;
            EditorUtility.SetDirty(group);
        }
        
        public static Node CreateNode(DialogSystemDatabase db,ConversationTree tree, System.Type type, Vector2 position)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            if (!node) return null;
            
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node.position = position;

            AddNodeToList(db,tree,node);
#if LOCALIZATION_EXIST
            if(node is not StartNode && node is not CompleteNode) LocalizationUtils.SetDefaultLocaleEntry(node.guid,"");
#endif
            return node;
        }
        
        private static void AddNodeToList(DialogSystemDatabase db,ConversationTree tree, Node node)
        {
            node.hideFlags = HideFlags.HideInHierarchy;
            Undo.RecordObject(tree, "Conversation Tree (CreateNode)");
            tree.nodes ??= new List<Node>();
            tree.nodes.Add(node);
            
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node,db);
            }
            Undo.RegisterCreatedObjectUndo(node, "Conversation Tree (CreateNode)");
            AssetDatabase.SaveAssets();
        }
        
        public static DialogNode CreateDialogNodeCopy(DialogSystemDatabase db,ConversationTree tree,  System.Type type, Vector2 position, SerializableDialogNode nodeToCopy)
        {
            DialogNode node = ScriptableObject.CreateInstance(type) as DialogNode;
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
        
        public static ChoiceNode CreateChoiceNodeToCopy(DialogSystemDatabase db,ConversationTree tree, Vector2 position, SerializableChoiceNode nodeToCopy, Dictionary<string, Choice> choiceMap)
        {
            Type type = typeof(ChoiceNode);
            ChoiceNode node = ScriptableObject.CreateInstance(type) as ChoiceNode;
            if (!node) return null;
            
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node.position = position;
            node.actor = db.actors.Find(actor => actor.guid == nodeToCopy.actorGuid);
            node.message = nodeToCopy.message;
            node.choices = new List<Choice>();
            foreach (var serializableChoice in nodeToCopy.choices)
            {
                Choice choice = ChoiceUtils.CreateChoice(db, node, serializableChoice.choiceMessage);
                choiceMap.Add(serializableChoice.guid, choice);
            }

            AddNodeToList(db,tree,node);
#if LOCALIZATION_EXIST
            LocalizationUtils.AddCopyKeyToCollection(node.guid,nodeToCopy.guid);
#endif
            return node;
        }
        
        public static void DeleteNode(ConversationTree tree, Node node)
        {
            Undo.RecordObject(tree, "Conversation Tree (DeleteNode)");
            tree.nodes.Remove(node);
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }
        
        public static bool AddChild (Node parent, Node child, Port parentPort = null)
        {
            bool hasAdded = false;
            ParentNode parentNode = parent as ParentNode;
            if(parentNode && !parentNode.children.Contains(child) )
            {
                Undo.RecordObject(parentNode, "Conversation Tree (AddChild)");
                parentNode.children.Add(child);
                EditorUtility.SetDirty(parentNode);
                hasAdded = true;
            }
            
            ChoiceNode choiceNode = parent as ChoiceNode;
            if(choiceNode && parentPort != null)
            {
                NodeView  choiceView = parentPort.node as NodeView;
                Choice choice = choiceView?.FindPortChoice(parentPort);
                if (choice)
                {
                    Undo.RecordObject(choice, "Conversation Tree (AddChoice)");
                    choice.children.Add(child);
                    EditorUtility.SetDirty(choice);
                    hasAdded = true;
                }
            }

            return hasAdded;
        }
        
        public static void RemoveChild(Node parent, Node child, Port parentPort = null)
        {
            ParentNode parentNode = parent as ParentNode;
            if(parentNode)
            {
                Undo.RecordObject(parentNode, "Conversation Tree (RemoveChild)");
                parentNode.children.Remove(child);
                EditorUtility.SetDirty(parentNode);
            }
            
            ChoiceNode choiceNode = parent as ChoiceNode;
            if(choiceNode && parentPort != null)
            {
                NodeView  choiceView = parentPort.node as NodeView;
                Choice choice = choiceView?.FindPortChoice(parentPort);
                if (choice)
                {
                    Undo.RecordObject(choice, "Conversation Tree (RemoveChild)");
                    choice.children.Remove(child);
                    EditorUtility.SetDirty(choice);
                }
            }
        }
        
    }
}
