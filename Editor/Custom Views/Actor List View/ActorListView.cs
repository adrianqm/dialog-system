using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ActorListView : VisualElement
{
    public new class UxmlFactory:  UxmlFactory<ActorListView, ActorListView.UxmlTraits> {}

    private ActorMultiColumListView _actorMultiColumList;
    public ActorListView()
    {
        string uriFile = "Assets/dialog-system/Editor/Custom Views/Actor List View/ActorListView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);
        
        SetUpToolbar();
    }
    
    private void SetUpToolbar()
    {
        ToolbarMenu menuBar = this.Q<ToolbarMenu>("actorsToolbar");
        VisualElement textElement = menuBar.ElementAt(0);
        textElement.AddToClassList("plusElement");
        textElement.Add(new Image {
            image = EditorGUIUtility.IconContent("Toolbar Plus").image
        });
        menuBar.menu.AppendAction("Create Actor", a => AddActor());
        
        _actorMultiColumList = this.Q<ActorMultiColumListView>();
    }

    void AddActor()
    {
        _actorMultiColumList.CreateNewActor();
    }
}
