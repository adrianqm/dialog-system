using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class BookmarkNodeSO : NodeSO
    {
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
        
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    NodeState = State.Initial;
                    break;
                case PlayModeStateChange.EnteredPlayMode: break;
            }
        }
#endif
    }
}