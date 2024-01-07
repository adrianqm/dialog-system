using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AQM.Tools
{
    public class ConversationInspectorView : VisualElement
    {
        private ConversationTree _conversationTree;
        
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
    }
}
