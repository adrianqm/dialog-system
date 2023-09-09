using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogInspectorView : VisualElement
{
    private NodeView _nodeView;
    private DialogNode _node;
    private ActorsTree _actorsTree;
    public DialogInspectorView(NodeView nodeView,ActorsTree actorsTree)
    {
        string uriFile = "Assets/dialog-system/Custom Views/Dialog Inspector View/DialogInspectorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);

        _nodeView = nodeView;
        _node = nodeView.node as DialogNode;
        _actorsTree = actorsTree;
        
        PopulateInspector();
    }

    private void PopulateInspector()
    {
        if (_node != null)
        {
            TextField messageLabel = this.Q<TextField>("messageTextField");
            messageLabel.bindingPath = "message";
            messageLabel.Bind(new SerializedObject(_node));
            
            Button findActorButton = this.Q<Button>("find-actor-button");
            //VisualElement actorImage = this.Q<VisualElement>("actor-image");
            SpritePreviewElement actorSprite = this.Q<SpritePreviewElement>("actor-sprite");
            Label actorName = this.Q<Label>("actor-name");
            actorSprite.bindingPath = "actorImage";
            actorName.bindingPath = "fullName";
            
            if (_node.actor)
            {
                actorSprite.Bind(new SerializedObject(_node.actor));
                actorName.Bind(new SerializedObject(_node.actor));
            }
            else
            {
                actorSprite.value = Resources.Load<Sprite>( "unknown-person" );
                actorName.text = "Unknown Dialog Actor";
            }
            findActorButton.clickable = new Clickable(() =>
            {
                ActorsSearchProvider provider =
                    ScriptableObject.CreateInstance<ActorsSearchProvider>();
                provider.SetUp(_actorsTree.actors,
                    (actorSelected) =>
                    {
                        actorSprite.Bind(new SerializedObject(actorSelected));
                        actorName.Bind(new SerializedObject(actorSelected));
                        _node.actor = actorSelected;
                        _nodeView.UpdateActorToBind(actorSelected);
                        AssetDatabase.SaveAssets();
                    });
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    provider);
            });
        }
    }
}
