using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NonPlayerActorsSearchProvider : ScriptableObject, ISearchWindowProvider
{
    private List<Actor> _nonPlayerActors;
    private Action<Actor> onSetIndexCallback;
    public void SetUp(List<Actor> actors, Action<Actor> callback)
    {
        _nonPlayerActors = actors.FindAll(actor => !actor.isPlayer);
        onSetIndexCallback = callback;
    }
    
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> searchlist = new List<SearchTreeEntry>();
        searchlist.Add(new SearchTreeGroupEntry(new GUIContent("Non Player Actors"), 0));

        foreach (var actor in _nonPlayerActors)
        {
            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(actor.fullName));
            entry.level = 1;
            entry.userData = actor;
            searchlist.Add(entry);
        }
        return searchlist;
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        onSetIndexCallback?.Invoke((Actor) searchTreeEntry.userData);
        return true;
    }
}
