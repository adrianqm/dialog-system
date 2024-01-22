
using System;
using AQM.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class ConversationsView : VisualElement
{
    private readonly string assetName = "ConversationsView";
    public new class UxmlFactory:  UxmlFactory<ConversationsView, ConversationsView.UxmlTraits> {}

    private InspectorView _inspectorView;
    private DialogSystemView _treeView;
    private Label _conversationNameLabel;
    private VisualElement _topConversationBar;
    private Action _unregisterAll;
    private ConversationTree _currentTree;
    private Button _bookmarkButton;
    
    public ConversationsView()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
        uxml.CloneTree(this);
        
        _inspectorView = this.Q<InspectorView>();
        _treeView = this.Q<DialogSystemView>();
        _conversationNameLabel = this.Q<Label>(className: "conversation-name-label");
        _topConversationBar = this.Q("top-conversation-bar");
        
        SetUpBookmarkButton();
        
        // To remove Hover style
        _treeView.RegisterCallback<ClickEvent>((evt) =>
        {
            ClearButtonHovers();
        });
        
        RegisterConversationHeaderButton("show-minimap", "d_AnimatorController On Icon", () =>
        {
            _treeView.ChangeMinimapDisplay();
        });
            
            
        RegisterConversationHeaderButton("frame-nodes", "d_GridLayoutGroup Icon", () =>
        {
            _treeView.FrameAllNodes();
        });
    }

    public void SetUpBackButton(Action onClick)
    {
        RegisterConversationHeaderButton("conversation-list", "ListView@8x", onClick.Invoke);
    }

    public void SetUpTree(ConversationTree tree)
    {
        _currentTree = tree;
        
        // Show conversation inspector
        _inspectorView.ShowConversationInspector(tree);
            
        if (_conversationNameLabel == null)
        {
            _conversationNameLabel = this.Q<Label>("conversation-name-label");
        }

        if (_conversationNameLabel != null)
        {
            ClearAllValueCallbacks();
            _topConversationBar.style.display = DisplayStyle.Flex;
            _conversationNameLabel.bindingPath = "title";
            _conversationNameLabel.Bind(new SerializedObject(tree));
            EventCallback<ClickEvent> clickEvent = (e) =>
            {
                _conversationNameLabel.AddToClassList("conversation-name-label--selected");
                _conversationNameLabel.RemoveFromClassList("conversation-name-label");
                SetConversationNameSelected();
                
                _bookmarkButton.RemoveFromClassList("bookmark-btn--selected");
                _bookmarkButton.AddToClassList("bookmark-btn");
            };

            _conversationNameLabel.RegisterCallback(clickEvent);
            _unregisterAll += () => _conversationNameLabel.UnregisterCallback(clickEvent);
        }
        
        _treeView?.PopulateViewAndFrameNodes(tree); 
    }

    public void ClearConversationView()
    {
        _inspectorView.ClearInspector();
        _treeView.ClearGraph();
        ClearButtonHovers();
        _topConversationBar.style.display = DisplayStyle.None;
    }

    public void ClearButtonHovers()
    {
        if (_conversationNameLabel != null)
        {
            _conversationNameLabel.RemoveFromClassList("conversation-name-label--selected");
            _conversationNameLabel.AddToClassList("conversation-name-label");
        }
            
        if (_bookmarkButton != null)
        {
            _bookmarkButton.RemoveFromClassList("bookmark-btn--selected");
            _bookmarkButton.AddToClassList("bookmark-btn");
        }
    }
    
    public void SetConversationNameSelected()
    {
        _treeView.ClearSelection();
        _inspectorView.ShowConversationInspector(_currentTree);
    }

    public void SetConversationNameAndRefresh()
    {
        SetConversationNameSelected();
        _treeView?.PopulateViewAndFrameNodes(_currentTree);
    }

    private void SetUpBookmarkButton()
    {
        _bookmarkButton = this.Q<Button>("bookmark-btn");
        _bookmarkButton.clickable = new Clickable(() =>
        {
            _bookmarkButton.AddToClassList("bookmark-btn--selected");
            _bookmarkButton.RemoveFromClassList("bookmark-btn");
            
            _treeView.ClearSelection();
            _inspectorView.ShowBookmarksInspector(_treeView, _currentTree);
            
            _conversationNameLabel.RemoveFromClassList("conversation-name-label--selected");
            _conversationNameLabel.AddToClassList("conversation-name-label");
        });
    }
    
    private void RegisterConversationHeaderButton(string buttonName,string iconTextureName, System.Action callback)
    {
        VisualElement buttonConversationList= this.Q(buttonName);
        Button conversationListButton = new Button();
        var conversationListTexture = EditorGUIUtility.IconContent(iconTextureName).image;
        conversationListButton.AddToClassList("conversation-bar--button");
        buttonConversationList.Add(conversationListButton);
        conversationListButton.Add(new Image {
            image = conversationListTexture,
        });
        conversationListButton.clickable = new Clickable(()=>
        {
            callback.Invoke();
        });
    }
    
    private void ClearAllValueCallbacks()
    {
        _unregisterAll?.Invoke();
        _unregisterAll = null;
    }
}
