using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AQM.Tools
{
    [CustomPropertyDrawer(typeof(ConversationTree))]
    public class ConversationDrawer : PropertyDrawer
    {
        private SerializedProperty _conversationProperty;
    
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _conversationProperty = property;
        
            var conversationDropdown = new ConversationDropdown();
            conversationDropdown.SetName(property.name);
        
            if(_conversationProperty.objectReferenceValue != null)
                conversationDropdown.SetConversation((ConversationTree)_conversationProperty.objectReferenceValue);
        
            conversationDropdown.onConversationSelected += SetConversation;
        
            conversationDropdown.style.marginTop = 5;
            conversationDropdown.style.marginBottom = 5;
        
            return conversationDropdown;
        }
        private void SetConversation(ConversationTree conversation)
        {
            _conversationProperty.objectReferenceValue = conversation;
            _conversationProperty.serializedObject.ApplyModifiedProperties();
        }
    } 
}
