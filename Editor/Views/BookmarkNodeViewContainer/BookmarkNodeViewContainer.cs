using AQM.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class BookmarkNodeViewContainer : VisualElement
{
    public new class UxmlFactory: UxmlFactory<StartNodeViewContainer, UxmlTraits> { }
    
    private readonly string uxmlName = "BookmarkNodeViewContainer";
    private Label _bookmarkLabel;
    private BookmarkSO _bookmark;
    public BookmarkNodeViewContainer(BookmarkNodeSO defaultBookmark)
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath, uxmlName);
        uxml.CloneTree(this);
        
        _bookmarkLabel = this.Q<Label>();
        _bookmark = defaultBookmark.bookmark;
        if (_bookmark)
        {
            //_bookmarkLabel.bindingPath = "bookmarkTitle";
            //_bookmarkLabel.Bind(new SerializedObject(_bookmark));
            _bookmarkLabel.Unbind();
            _bookmarkLabel.TrackSerializedObjectValue( new SerializedObject(_bookmark), UpdateBackground);
            _bookmarkLabel.text = _bookmark.bookmarkTitle;
            _bookmarkLabel.style.backgroundColor = _bookmark.bgColor;
            _bookmarkLabel.style.backgroundColor = _bookmark.bgColor;
        }
    }
    private void UpdateBackground(SerializedObject serializedObject)
    {
        _bookmarkLabel.text = _bookmark.bookmarkTitle;
        _bookmarkLabel.style.backgroundColor = _bookmark.bgColor;
        _bookmarkLabel.style.backgroundColor = _bookmark.bgColor;
    }
    
}