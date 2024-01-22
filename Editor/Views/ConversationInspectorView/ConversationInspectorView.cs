using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AQM.Tools
{
    public class ConversationInspectorView : VisualElement
    {
        private ConversationTree _conversationTree;
        private SpritePreviewElement _actorSprite;
        private Actor _defaultActor;
        private Label _actorName;
        private SerializedObject _actorSerialized;
        private Button _findActorButton;
        private VisualElement _actorContainer;
        
        private readonly string assetName = "ConversationInspectorView";
        public ConversationInspectorView(ConversationTree conversationTree)
        {
            VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
            uxml.CloneTree(this);

            _conversationTree = conversationTree;
            if (_conversationTree != null)
            {
                BindTitle();
                BindDescription();
                GetAndBindActor(_conversationTree.defaultActor, (actor) =>
                {
                    _conversationTree.defaultActor = actor;
                    EditorUtility.SetDirty(_conversationTree);
                });
            }
        }

        private void BindTitle()
        {
            TextField titleLabel = this.Q<TextField>("titleTextField");
            titleLabel.bindingPath = "title";
            titleLabel.Bind(new SerializedObject(_conversationTree));
        }
    
        private void BindDescription()
        {
            TextField descLabel = this.Q<TextField>("descriptionTextField");
            descLabel.bindingPath = "description";
            descLabel.Bind(new SerializedObject(_conversationTree));
        }

        private void GetAndBindActor(Actor actor, Action<Actor> onSelectActor)
        {
            _actorSprite = this.Q<SpritePreviewElement>("actor-sprite");
            _actorName = this.Q<Label>("actor-name");
            _actorSprite.bindingPath = "actorImage";
            _actorName.bindingPath = "fullName";
            _findActorButton = this.Q<Button>("find-actor-button");
            _findActorButton.Add(new Image {
                image = EditorGUIUtility.IconContent("d_pick_uielements").image
            });
            _actorContainer = this.Q("actor-container");
            
            if (actor)
            {
                BindActor(actor);
            }
            else
            {
                _actorSprite.value = UIToolkitLoader.LoadSprite(DialogSystemEditor.RelativePath, "unknown-person.jpg");
                _actorName.text = "Unknown Default Actor";
            }
            _findActorButton.clickable = new Clickable(() =>
            {
                ActorsSearchProvider provider =
                    ScriptableObject.CreateInstance<ActorsSearchProvider>();
                provider.SetUp(DSData.instance.database.actors,
                    (actorSelected) =>
                    {
                        BindActor(actorSelected);
                        onSelectActor.Invoke(actorSelected);
                    });
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    provider);
            });
        }
        
        private void BindActor(Actor selectedActor)
        {
            _actorContainer.Unbind();
            _defaultActor = selectedActor;
            _actorSerialized = new SerializedObject(selectedActor);
            _actorContainer.TrackSerializedObjectValue(_actorSerialized, HandleActorChanges);
            _actorSprite.Bind(_actorSerialized);
            _actorName.Bind(_actorSerialized);
            _actorContainer.style.backgroundColor = _defaultActor.bgColor;
        }
        
        private void HandleActorChanges(SerializedObject serializedObject)
        {
            _actorContainer.style.backgroundColor = _defaultActor.bgColor;
        }
    }
}
