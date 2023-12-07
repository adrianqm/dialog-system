using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public static class DatabaseUtils
    {
        public static DialogSystemDatabase CreateDatabase(string title, string description)
        {
            DialogSystemDatabase db = ScriptableObject.CreateInstance(typeof(DialogSystemDatabase)) as DialogSystemDatabase;
            if (!db) return null;
            
            db.guid = GUID.Generate().ToString();
            db.title = title;
            db.description = description;
            db.conversationGroups = new List<ConversationGroup>();
            db.actors = new List<Actor>();
            
            if (!Application.isPlaying)
            {
                string dbPath ="Assets/dialog-system/Runtime/Scriptable Objects/Data/"+title+".asset";
                AssetDatabase.CreateAsset(db, dbPath);
                db.guid = AssetDatabase.AssetPathToGUID(dbPath);
                
                //Create Conversations
                ConversationGroup conGroup = CreateConversationGroup(db, "Default Group");
                CreateConversation(db,conGroup);
                
                //Create Actors
                CreateActor(db);
            }
            AssetDatabase.SaveAssets();
            return db;
        }
        
        public static void DeleteDatabase(DialogSystemDatabase db)
        {
            var path = AssetDatabase.GUIDToAssetPath(db.guid);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
        }
        
        public static void CreateActor(DialogSystemDatabase db)
        {
            
            Actor newActor = ScriptableObject.CreateInstance(typeof(Actor)) as Actor;
            if (!newActor) return;
            
            newActor.name = "Actor";
            newActor.guid = GUID.Generate().ToString();
            newActor.fullName = "defaultName";
            newActor.description = "defaultDesc";
            newActor.hideFlags = HideFlags.HideInHierarchy;
            
            db.actors.Add(newActor);
                
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset( newActor,db);
            }
            AssetDatabase.SaveAssets();
        }
        
        public static void DeleteActor(DialogSystemDatabase db, Actor actor)
        {
            db.actors.Remove(actor);
            Undo.DestroyObjectImmediate(actor);
            AssetDatabase.SaveAssets();
        }
        
        public static void CreateConversation(DialogSystemDatabase db, ConversationGroup conversationGroup)
        {
            
            ConversationTree newConversation = ScriptableObject.CreateInstance(typeof(ConversationTree)) as ConversationTree;
            if (newConversation)
            {
                newConversation.name = "ConversationTree";
                newConversation.guid = GUID.Generate().ToString();
                newConversation.title = "Default Title";
                newConversation.description = "Default Desc";
                newConversation.hideFlags = HideFlags.HideInHierarchy;
                
                //Undo.RecordObject(this, "Actors Tree (CreateActor)");
                conversationGroup.conversations.Add(newConversation);
                
                if (!Application.isPlaying)
                {
                    AssetDatabase.AddObjectToAsset( newConversation,db);
                }
                //Undo.RegisterCreatedObjectUndo(newActor, "Actors Tree (CreateActor)");
                AssetDatabase.SaveAssets();
            }
        }
        
        public static void DeleteConversation(ConversationGroup conversationGroup, ConversationTree conversation)
        {
            conversationGroup.conversations.Remove(conversation);
            Undo.DestroyObjectImmediate(conversation);
            AssetDatabase.SaveAssets();
        }
        
        public static ConversationGroup CreateConversationGroup(DialogSystemDatabase db, string title, ConversationGroup fromGroup = null)
        {
            
            ConversationGroup newGroup = ScriptableObject.CreateInstance(typeof(ConversationGroup)) as ConversationGroup;
            if (newGroup)
            {
                newGroup.name = "ConversationGroup";
                newGroup.guid = GUID.Generate().ToString();
                newGroup.title = title;
                newGroup.groups ??= new List<ConversationGroup>();
                newGroup.conversations ??= new List<ConversationTree>();
                newGroup.hideFlags = HideFlags.HideInHierarchy;
                if (!fromGroup || db.conversationGroups == null)
                {
                    db.conversationGroups??= new List<ConversationGroup>();
                    db.conversationGroups.Add(newGroup);
                }else if (fromGroup)
                {
                    fromGroup.groups.Add(newGroup);
                }
                
                if (!Application.isPlaying)
                {
                    AssetDatabase.AddObjectToAsset( newGroup,db);
                }
                AssetDatabase.SaveAssets();
            }
            return newGroup;
        }
        
        public static void DeleteConversationGroup(DialogSystemDatabase db,  ConversationGroup group, ConversationGroup fromGroup = null)
        {
            RemoveConversationGroups(db,group, fromGroup);
            AssetDatabase.SaveAssets();
        }

        private static void RemoveConversationGroups
            (DialogSystemDatabase db, ConversationGroup group, ConversationGroup fromGroup = null)
        {
            RemoveConversationGroupsInGroup(group);
            RemoveConversationsInGroup(group);

            if (fromGroup == null) db.conversationGroups.Remove(group);
            else fromGroup.groups.Remove(group);
            Undo.DestroyObjectImmediate(group);
        }

        private static void RemoveConversationGroupsInGroup(ConversationGroup group)
        {
            foreach (var conversationGroup in group.groups)
            {
                RemoveConversationGroupsInGroup(conversationGroup);
                Undo.DestroyObjectImmediate(conversationGroup);
            }
            group.groups.Clear();

            RemoveConversationsInGroup(group);
        }

        private static void RemoveConversationsInGroup(ConversationGroup group)
        {
            foreach (var groupConversation in group.conversations)
            {
                Undo.DestroyObjectImmediate(groupConversation);
            }
            group.conversations.Clear();
        }
    }
}
