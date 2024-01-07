using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class ActorListView : VisualElement
{
    private readonly string assetName = "ActorListView";
    public new class UxmlFactory:  UxmlFactory<ActorListView, ActorListView.UxmlTraits> {}

    private ActorMultiColumListView _actorMultiColumList;
    public ActorListView()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
        uxml.CloneTree(this);
        
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
