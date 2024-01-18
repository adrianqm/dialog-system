using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class BookmarkNodeSO : NodeSO
    {
        public BookmarkSO bookmark;
        public override NodeSO Clone()
        {
            BookmarkNodeSO nodeSo = Instantiate(this);
            return nodeSo;
        }

#if UNITY_EDITOR

        public override void Init(Vector2 position)
        {
            base.Init(position);
            name = $"BookmarkNode-{guid}";
        }

        public void SetUpBookmark(BookmarkSO bookmarkSo)
        {
            bookmark = bookmarkSo;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}