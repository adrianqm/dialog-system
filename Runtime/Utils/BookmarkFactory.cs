using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{

    public static class BookmarkFactory
    {
        public static BookmarkSO CreateBookmark()
        {
            BookmarkSO bookmark = ScriptableObject.CreateInstance<BookmarkSO>();
            bookmark.Init();
            return  bookmark;
        }
    }
}
