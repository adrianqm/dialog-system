using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ActorsSearchProvider : ScriptableObject, ISearchWindowProvider
{
    private List<Actor> _actors;
    private Action<Actor> _onSetIndexCallback;
    private Texture2D _indentationIcon;
    public void SetUp(List<Actor> actors, Action<Actor> callback)
    {
        _actors = actors;
        _onSetIndexCallback = callback;
    
        _indentationIcon = new Texture2D(1, 1);
        _indentationIcon.SetPixel(0, 0, Color.clear);
        _indentationIcon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> searchlist = new List<SearchTreeEntry>();
        searchlist.Add(new SearchTreeGroupEntry(new GUIContent("Actors"), 0));

        foreach (var actor in _actors)
        {
            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(actor.fullName,_indentationIcon));
            entry.level = 1;
            entry.userData = actor;
            searchlist.Add(entry);
        }
        return searchlist;
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        _onSetIndexCallback?.Invoke((Actor) searchTreeEntry.userData);
        return true;
    }
}

