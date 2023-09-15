using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog System Database", menuName = "AQM/Tools/Dialog System/Dialog System Database", order = 1)]
public class DialogSystemDatabase : ScriptableObject
{
    public string title;
    public string description;
    [HideInInspector] public string guid;
    public List<ConversationTree> conversations;
    public List<Actor> actors;

    private ActorMultiColumListView _actorMultiColumListView;
    private DialogSystemView _treeView;
    
#if  UNITY_EDITOR
    public void Create(string title, string description)
    {
        this.title = title;
        this.description = description;
        conversations = new List<ConversationTree>();
        
        if (!Application.isPlaying)
        {
            guid = AssetDatabase.CreateFolder("Assets/dialog-system/Scriptable Objects/Data", title+" Database");
            string dbFolderPath = AssetDatabase.GUIDToAssetPath(guid);
            string dbPath = dbFolderPath + '/'+title+".asset";
            
            //Create Conversations
            string conGuid = AssetDatabase.CreateFolder(dbFolderPath, "Conversations");
            string conversationsPath = AssetDatabase.GUIDToAssetPath(conGuid);
            ConversationTree conTree = ScriptableObject.CreateInstance<ConversationTree>();
            conTree.Create(this,"Conversation Title", "Default Description");
            conversations.Add(conTree);
            
            EditorUtility.SetDirty(conTree);
            
            //Create Actors
            CreateActor();
            
            //Create Final db
            AssetDatabase.CreateAsset(this, dbPath);
        }
        AssetDatabase.SaveAssets();
    }

    public void Delete()
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.SaveAssets();
    }
    
    public void CreateActor()
    {
        
        Actor newActor = ScriptableObject.CreateInstance(typeof(Actor)) as Actor;
        if (newActor)
        {
            newActor.name = "Actor";
            newActor.guid = GUID.Generate().ToString();
            newActor.fullName = "defaultName";
            newActor.description = "defaultDesc";
            newActor.hideFlags = HideFlags.HideInHierarchy;
            
            //Undo.RecordObject(this, "Actors Tree (CreateActor)");
            actors.Add(newActor);
            
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset( newActor,this);
            }
            //Undo.RegisterCreatedObjectUndo(newActor, "Actors Tree (CreateActor)");
            AssetDatabase.SaveAssets();
        }
    }
    
    public void DeteleActor(Actor actor)
    {
        //Undo.RecordObject(this, "Actors Tree (DeleteActor)");
        actors.Remove(actor); 
        //AssetDatabase.RemoveObjectFromAsset(node);
        
        Undo.DestroyObjectImmediate(actor);
        AssetDatabase.SaveAssets();
    }
#endif
}
