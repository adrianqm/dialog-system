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
    public DialogInspectorView(NodeView nodeView,ActorsTree actorsTree)
    {
        string uriFile = "Assets/dialog-system/Custom Views/DialogInspectorView/DialogInspectorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);

        _nodeView = nodeView;
        _node = nodeView.node as DialogNode;
        
        
        
        if (_node != null)
        {
            TextField messageLabel = this.Q<TextField>("messageTextField");
            messageLabel.bindingPath = "message";
            messageLabel.Bind(new SerializedObject(_node));
            
            Button findActorButton = this.Q<Button>("find-actor-button");
            VisualElement actorImage = this.Q<VisualElement>("actor-image");
            Label actorName = this.Q<Label>("actor-name");
            
            if (_node.actor)
            {
                actorImage.style.backgroundImage = new StyleBackground(_node.actor.actorImage);
                actorName.text = _node.actor.fullName;
            }
            findActorButton.clickable = new Clickable(() =>
            {
                NonPlayerActorsSearchProvider provider =
                    ScriptableObject.CreateInstance<NonPlayerActorsSearchProvider>();
                provider.SetUp(actorsTree.actors,
                    (actorSelected) =>
                    {
                        actorImage.style.backgroundImage = new StyleBackground(actorSelected.actorImage);
                        actorName.text = actorSelected.fullName;
                        _node.actor = actorSelected;
                    });
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    provider);
            });
        }
    }
}
