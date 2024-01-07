using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ConversationListView : VisualElement {
    
    private readonly string assetName = "ConversationListView";
    public new class UxmlFactory:  UxmlFactory<ConversationListView, ConversationListView.UxmlTraits> {}
    private ToolbarSearchField _conversationsSearchField;
    private ConversationMultiColumListView _conversationMultiColumListView;
    
    public ConversationListView()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
        uxml.CloneTree(this);

        SetUpToolbar();
    }
    
    private void SetUpToolbar()
    {
        ToolbarMenu menuBar = this.Q<ToolbarMenu>("conversationsToolbar");
        VisualElement textElement = menuBar.ElementAt(0);
        textElement.AddToClassList("plusElement");
        textElement.Add(new Image {
            image = EditorGUIUtility.IconContent("Toolbar Plus").image
        });
        menuBar.menu.AppendAction("Create Conversation", a => AddConversation());
        
        _conversationMultiColumListView = this.Q<ConversationMultiColumListView>();
    }

    void AddConversation()
    {
        _conversationMultiColumListView.CreateNewConversation();
    }
}
