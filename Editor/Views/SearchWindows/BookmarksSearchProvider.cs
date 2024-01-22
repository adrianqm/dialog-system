using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BookmarksSearchProvider : ScriptableObject, ISearchWindowProvider
{
    private BookmarkSO _startBookmark;
    private List<BookmarkSO> _bookmarks;
    private Action<BookmarkSO> _onSetIndexCallback;
    private Texture2D _indentationIcon;
    private bool _showNonSelected;
    
    public void SetUp(ConversationTree tree, Action<BookmarkSO> callback, bool showNonSelected = false)
    {
        _startBookmark = tree.startBookmark;
        _bookmarks = tree.bookmarks;
        _onSetIndexCallback = callback;
        _showNonSelected = showNonSelected;
    
        _indentationIcon = new Texture2D(1, 1);
        _indentationIcon.SetPixel(0, 0, Color.clear);
        _indentationIcon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> searchlist = new List<SearchTreeEntry>();
        searchlist.Add(new SearchTreeGroupEntry(new GUIContent("Bookmarks"), 0));

        if (!_showNonSelected) // Add [START]
        {
            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(_startBookmark.bookmarkTitle,_indentationIcon));
            entry.level = 1;
            entry.userData = _startBookmark;
            searchlist.Add(entry);
        }
        foreach (var bookmark in _bookmarks) // Add All bookmarks
        {
            if(_showNonSelected && bookmark.goToNode != null) continue;
            
            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(bookmark.bookmarkTitle,_indentationIcon));
            entry.level = 1;
            entry.userData = bookmark;
            searchlist.Add(entry);
        }
        return searchlist;
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        _onSetIndexCallback?.Invoke((BookmarkSO) searchTreeEntry.userData);
        return true;
    }
}

