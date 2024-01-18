using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AQM.Tools
{
    public class BookmarksInspectorView : VisualElement
    {
        private ConversationTree _conversationTree;
        private List<BookmarkSO> _bookmarks = new List<BookmarkSO>();
        private ListView _listView;
        
        private readonly string assetName = "BookmarksInspectorView";
        public BookmarksInspectorView(ConversationTree conversationTree)
        {
            VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
            uxml.CloneTree(this);

            _conversationTree = conversationTree;
            if (_conversationTree != null)
            {
                _listView = this.Q<ListView>("bookmarksList");
                _bookmarks = _conversationTree.bookmarks;
                SetUp();
                RegisterCallbacks();
            }
        }

        private void  SetUp()
        {
            _listView.itemsSource = _bookmarks;
            _listView.makeItem = MakeItem;
            _listView.bindItem = (element, i) => BindItem(element, i);
        }
        
        private VisualElement MakeItem() => new BookmarkView();

        private void BindItem(VisualElement ve, int index)
        {
            var bookmarkView = ve as BookmarkView;
            bookmarkView?.BindCondition(_bookmarks[index]);
        }

        private void RegisterCallbacks()
        {
        
            _listView.Q<Button>("unity-list-view__add-button").clickable = new Clickable(() =>
            {
                _conversationTree.CreateBookmark(DSData.instance.database);
                _bookmarks = _conversationTree.bookmarks;
                //_searchField?.SetValueWithoutNotify("");
                _listView.RefreshItems();
                FocusOnLastElement();
            });
        
            _listView.Q<Button>("unity-list-view__remove-button").clickable = new Clickable(() =>
            {
                if (_listView.selectedIndex == -1) return;
                RemoveBookmark(_bookmarks[_listView.selectedIndex]);
                _bookmarks = _conversationTree.bookmarks;
                _listView.RefreshItems();
            });
        }
        
        private void FocusOnLastElement()
        {
            _listView.ScrollToItem(-1); // Seems that will be solved in followed versions
            var list = this.Query<VisualElement>(className: "unity-list-view__item").ToList();
            if (list.Count <= 0) return;
            int lastIndex = list.Count - 1;
            _listView.SetSelection(lastIndex);
            var itemView = list.ElementAt(lastIndex);
            var nameField = itemView.Q<TextField>(className:"bookmark--title");
            nameField.Focus();
        }

        private void RemoveBookmark(BookmarkSO bookmark)
        {
            _conversationTree.DeleteBookmark(bookmark);
        }
    }
}
