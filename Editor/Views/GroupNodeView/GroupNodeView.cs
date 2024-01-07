using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GroupNodeView : Group
{
    public GroupNode group;

    public GroupNodeView(GroupNode group)
    {
        this.group = group;
        this.title = group.title;
        this.viewDataKey = group.guid;
        
        SetPosition(new Rect(this.group.position, Vector2.zero));
    }

    public sealed override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        Undo.RecordObject(group, "Conversation Tree (Set Group Position)");
        group.position.x = newPos.xMin;
        group.position.y = newPos.yMin;
        EditorUtility.SetDirty(group);
    }
}
