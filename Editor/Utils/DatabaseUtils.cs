using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public static class DatabaseUtils
    {
        public static void CreateDatabase(string title, string description)
        {
            DialogSystemDatabase db = ScriptableObject.CreateInstance(typeof(DialogSystemDatabase)) as DialogSystemDatabase;
            if (!db) return;
            
            db.title = title;
            db.description = description;
            db.conversations = new List<ConversationTree>();
            
            if (!Application.isPlaying)
            {
                db.guid = AssetDatabase.CreateFolder("Assets/dialog-system/Scriptable Objects/Data", title+" Database");
                string dbFolderPath = AssetDatabase.GUIDToAssetPath(db.guid);
                string dbPath = dbFolderPath + '/'+title+".asset";
                
                //Create Conversations
                ConversationTree conTree = ScriptableObject.CreateInstance<ConversationTree>();
                ConversationUtils.CreateConversation(db,"Conversation Title", "Default Description");
                db.conversations.Add(conTree);
                
                EditorUtility.SetDirty(conTree);
                
                //Create Actors
                CreateActor(db);
                
                //Create Final db
                AssetDatabase.CreateAsset(db, dbPath);
            }
            AssetDatabase.SaveAssets();
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
    }
}
