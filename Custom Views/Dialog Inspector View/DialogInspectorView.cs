using System;
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
    private List<Actor> _actors;
    public DialogInspectorView(NodeView nodeView,List<Actor> actors)
    {
        string uriFile = "Assets/dialog-system/Custom Views/Dialog Inspector View/DialogInspectorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);
        
        _nodeView = nodeView;
        _actors = actors;

        if (nodeView.node is RootNode)
        {
            
        }
        else if(nodeView.node is DialogNode)
        {
            var node = nodeView.node as DialogNode;
            PopulateDialogInspector(node);
        }else if (nodeView.node is ChoiceNode)
        {
            var node = nodeView.node as ChoiceNode;
            PopulateChoiceInspector(node);
        }
    }

    private void PopulateDialogInspector(DialogNode node)
    {
        if (node == null) return;

        BindMessage(node);
        BindActor(node.actor, (actor) =>
        {
            node.actor = actor;
            EditorUtility.SetDirty(node);
        });
    }

    private void PopulateChoiceInspector(ChoiceNode node)
    {
        if (node == null) return;
        
        BindMessage(node);
        BindActor(node.actor,(actor) =>
        {
            node.actor = actor;
            EditorUtility.SetDirty(node);
        });
    }

    private void BindMessage(Node node)
    {
        TextField messageLabel = this.Q<TextField>("messageTextField");
        messageLabel.bindingPath = "message";
        messageLabel.Bind(new SerializedObject(node));
    }

    private void BindActor(Actor actor, Action<Actor> onSelectActor)
    {
        Button findActorButton = this.Q<Button>("find-actor-button");
        //VisualElement actorImage = this.Q<VisualElement>("actor-image");
        SpritePreviewElement actorSprite = this.Q<SpritePreviewElement>("actor-sprite");
        Label actorName = this.Q<Label>("actor-name");
        actorSprite.bindingPath = "actorImage";
        actorName.bindingPath = "fullName";
            
        if (actor)
        {
            actorSprite.Bind(new SerializedObject(actor));
            actorName.Bind(new SerializedObject(actor));
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
            provider.SetUp(_actors,
                (actorSelected) =>
                {
                    actorSprite.Bind(new SerializedObject(actorSelected));
                    actorName.Bind(new SerializedObject(actorSelected));
                    _nodeView.UpdateActorToBind(actorSelected);
                    onSelectActor.Invoke(actorSelected);
                });
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                provider);
        });
    }
}
