using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Actors Tree", menuName = "AQM/Tools/Dialog System/Actors Tree", order = 3)]
public class ActorsTree : ScriptableObject
{
    public List<Actor> actors = new();
#if  UNITY_EDITOR
    public ActorsTree Create(string path)
    {
        string conTreePath = path + "/Actors.asset";
        AssetDatabase.CreateAsset(this, conTreePath);
        CreateActor();
        return this;
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
            Undo.RecordObject(this, "Actors Tree (CreateActor)");
            actors.Add(newActor);
            
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset( newActor,this);
            }
            Undo.RegisterCreatedObjectUndo(newActor, "Actors Tree (CreateActor)");
            AssetDatabase.SaveAssets();
        }
    }
    
    public void DeteleActor(Actor actor)
    {
        Undo.RecordObject(this, "Actors Tree (DeleteActor)");
        actors.Remove(actor); 
        //AssetDatabase.RemoveObjectFromAsset(node);
        
        Undo.DestroyObjectImmediate(actor);
        AssetDatabase.SaveAssets();
    }
#endif
}
