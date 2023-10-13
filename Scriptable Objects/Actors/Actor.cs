using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Actor", menuName = "AQM/Tools/Dialog System/Actor", order = 4)]
public class Actor : ScriptableObject
{
    public string guid;
    public Sprite actorImage;
    [TextArea] public string fullName;
    [TextArea] public string description;
    public Color bgColor;
    public Action<Actor> onDestroyActor;

    public Actor Clone()
    {
        Actor actor = Instantiate(this);
        actor.actorImage = actorImage;
        actor.description = description;
        actor.bgColor = bgColor;
        actor.fullName = fullName;
        return actor;
    }
    
    private void OnDestroy()
    {
        onDestroyActor?.Invoke(this);
        onDestroyActor = null;
    }
}
