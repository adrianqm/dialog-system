using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class BookmarkSO : ScriptableObject
    {
        public string guid;
        [TextArea] public string bookmarkTitle;
        public NodeSO goToNode;
        public Color bgColor;
        
#if UNITY_EDITOR
        public void Init(string title,NodeSO node, Color color)
        {
            guid = GUID.Generate().ToString();
            name = $"Bookmark-{guid}";
            bookmarkTitle = title;
            goToNode = node;
            bgColor = color;
        }
        
        public void SaveAs(DialogSystemDatabase db)
        {
            AssetDatabase.AddObjectToAsset(this, db);
        }
#endif
    }
}
