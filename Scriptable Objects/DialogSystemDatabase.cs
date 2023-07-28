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
    public List<ConversationTree> conversations;
    public ActorsTree actorsTree;

    private ActorMultiColumListView _actorMultiColumListView;
    private DialogSystemView _treeView;

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
}
