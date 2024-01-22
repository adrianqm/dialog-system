using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{

    public static class BookmarkFactory
    {
#if UNITY_EDITOR
        public static BookmarkSO CreateBookmark(string title,NodeSO node, Color color)
        {
            BookmarkSO bookmark = ScriptableObject.CreateInstance<BookmarkSO>();
            bookmark.Init(title,node,color);
            return  bookmark;
        }
#endif
    }
}
