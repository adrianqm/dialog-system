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
    public class ConversationDropdown  : VisualElement
    {
        private readonly string assetName = "DropdownElement";
        public new class UxmlFactory:  UxmlFactory<ConversationDropdown, ConversationDropdown.UxmlTraits> {}
        
        public Action<ConversationTree> onConversationSelected;
        
        public ConversationTree conversationSelected;
        
        private Label nameLabel;
        private Button buttonPopup;
        
        public ConversationDropdown()
        {
            VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
            uxml.CloneTree(this);

            style.flexGrow = 1;
            
            nameLabel = this.Q<Label>("name-label");

            buttonPopup = this.Q<Button>();
            buttonPopup.clickable = new Clickable(OpenSearchWindow);
            
            UpdateButtonText();
        }
        public void SetName(string conversationName)
        {
            nameLabel.text = conversationName;
            nameLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }
        
        private void OpenSearchWindow()
        {
            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            
            var conversationPairs = GetConversationPairs();

            var conversationSearchProvider = ScriptableObject.CreateInstance<ConversationSearchProvider>();
            conversationSearchProvider.Init(conversationPairs, SetConversation);
            
            SearchWindow.Open(new SearchWindowContext(mousePos), conversationSearchProvider);
        }
        
        private List<KeyValuePair<ConversationTree, string>> GetConversationPairs()
        {
            return DialogSystemController.Instance.GetConversationPairs();
        }
        
        public void SetConversation(ConversationTree newConversationSelected)
        {
            if(conversationSelected == newConversationSelected)
                return;
            BindConversation(newConversationSelected);
            UpdateButtonText();
            onConversationSelected?.Invoke(newConversationSelected);
        }

        void BindConversation(ConversationTree conversation)
        {
            if(conversationSelected != null)
                conversationSelected.onNameChanged -= UpdateButtonText;
            
            conversationSelected = conversation;

            if(conversation != null)
                conversationSelected.onNameChanged += UpdateButtonText;
        }
        
        private void UpdateButtonText()
        {
            string conversationPath = "";

            if (conversationSelected != null)
                conversationPath = DialogSystemController.Instance.GetConversationPath(conversationSelected);
            
            buttonPopup.text = conversationPath;
        }
    }
}

