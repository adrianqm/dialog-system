using AQM.Tools;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BookmarkNodeViewContainer : VisualElement
{
    public new class UxmlFactory: UxmlFactory<StartNodeViewContainer, UxmlTraits> { }
    
    private readonly string uxmlName = "BookmarkNodeViewContainer";
    private Label _bookmarkLabel;
    private BookmarkSO _bookmark;
    private ConversationTree _currentTree;
    private BookmarkNodeSO _node;
    public BookmarkNodeViewContainer(ConversationTree tree, BookmarkNodeSO defaultBookmark)
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath, uxmlName);
        uxml.CloneTree(this);
        
        _bookmarkLabel = this.Q<Label>();
        _bookmark = defaultBookmark.bookmark;
        _node = defaultBookmark;
        _currentTree = tree;
        if (_bookmark)
        {
            UpdateBookmark();
        }
        SetUpButton();
    }

    private void SetUpButton()
    {
        Button bookmarkSearchBtn =  this.Q<Button>();
        bookmarkSearchBtn.Add(new Image {
            image = EditorGUIUtility.IconContent("d_pick_uielements").image
        });
        bookmarkSearchBtn.clickable = new Clickable(() =>
        {
            BookmarksSearchProvider provider =
                ScriptableObject.CreateInstance<BookmarksSearchProvider>();
            provider.SetUp(_currentTree,
                (bookmarkSelected) =>
                {
                    _bookmark = bookmarkSelected;
                    _node.bookmark = bookmarkSelected;
                    UpdateBookmark();
                });
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                provider);
        });
    }

    private void UpdateBookmark()
    {
        _bookmarkLabel.Unbind();
        _bookmarkLabel.TrackSerializedObjectValue( new SerializedObject(_bookmark), UpdateBackground);
        _bookmarkLabel.text = _bookmark.bookmarkTitle;
        _bookmarkLabel.style.backgroundColor = _bookmark.bgColor;
    }
    
    private void UpdateBackground(SerializedObject serializedObject)
    {
        _bookmarkLabel.text = _bookmark.bookmarkTitle;
        _bookmarkLabel.style.backgroundColor = _bookmark.bgColor;
    }
    
}