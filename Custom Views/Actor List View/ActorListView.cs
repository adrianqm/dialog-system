using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ActorListView : VisualElement
{
    public new class UxmlFactory:  UxmlFactory<ActorListView, ActorListView.UxmlTraits> {}
    
    public ActorListView()
    {
        string uriFile = "Assets/dialog-system/Custom Views/Actor List View/ActorListView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);
    }
}
