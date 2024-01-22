using System.Collections.Generic;
using Blackboard.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class DatabaseFactory
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
                string dbPath ="Assets/SO/Dialog Designer/"+title+".asset";
                ScriptableObjectUtility.SaveAsset(db,dbPath);
                db.guid = AssetDatabase.AssetPathToGUID(dbPath);
                
                //Create Conversations
                ConversationGroup conGroup = db.CreateConversationGroup("Default Group");
                conGroup.CreateConversation(db);
                
                //Create Actors
                db.CreateActor();
            }
            AssetDatabase.SaveAssets();
            return db;
        }
    }
}