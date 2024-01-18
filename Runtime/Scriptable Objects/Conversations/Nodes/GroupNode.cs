using UnityEditor;
using UnityEngine;

[System.Serializable]
public class GroupNode : ScriptableObject
{
    [HideInInspector] public string guid;
    [HideInInspector] public Vector2 position;
    public string title;
    
    public GroupNode Clone()
    {
        GroupNode node = Instantiate(this);
        return node;
    }
#if UNITY_EDITOR
    public void SetGroupTitle(string newTitle)
    {
        Undo.RecordObject(this, "Conversation Tree (SetGroupTitle)");
        title = newTitle;
        EditorUtility.SetDirty(this);
    }
#endif
    
}
