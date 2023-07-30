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
    public ActorsTree actorsTree;

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
            conTree.Create(conversationsPath,this,"Conversation Title", "Default Description");
            conversations.Add(conTree);
            EditorUtility.SetDirty(conTree);
            //Create Actors
            actorsTree = ScriptableObject.CreateInstance<ActorsTree>();
            actorsTree.Create(dbFolderPath);
            
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

    public void RegisterUndoOperation(ActorMultiColumListView actorMultiColumLisView, DialogSystemView treeView)
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
        _actorMultiColumListView = actorMultiColumLisView;
        _treeView = treeView;
        Undo.undoRedoPerformed += OnUndoRedo;
    }
    
    private void OnUndoRedo()
    {
        _actorMultiColumListView.SetupTableAndCleanSearch(actorsTree);
        _treeView.MarkDirtyRepaint();
        _treeView.PopulateView(conversations[0]);
        AssetDatabase.SaveAssets();
    }
#endif
}
