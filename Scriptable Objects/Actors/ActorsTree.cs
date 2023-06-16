using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Actors Tree", menuName = "AQM/Tools/Dialog System/Actors Tree", order = 3)]
public class ActorsTree : ScriptableObject
{
    public List<Actor> actors = new ();
}
