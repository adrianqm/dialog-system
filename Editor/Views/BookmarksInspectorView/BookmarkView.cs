using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AQM.Tools
{
    public class BookmarkView : VisualElement
    {
        private const string AssetName = "Views/BookmarksInspectorView/BookmarkView.uxml";

        private BookmarkSO _bookmark;
        private TextField _titleText;
        private ColorField _colorField;
        private Button _focusBtn;
        private VisualElement _mark;
        private VisualElement _markSelected;
        
        public BookmarkView()
        {
            string path = Path.Combine(DialogSystemEditor.RelativePath, AssetName);
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            uxml.CloneTree(this);
            
            _titleText = this.Q<TextField>();
            _colorField = this.Q<ColorField>();
            _focusBtn = this.Q<Button>();
            _mark = this.Q("mark");
            _markSelected = this.Q("mark-selected");
            _focusBtn.Add(new Image {
                image = EditorGUIUtility.IconContent("d_Search Icon").image
            });
        }

        public void BindBookmark(DialogSystemView graphView, BookmarkSO bookmark)
        {
            if (bookmark == null) return;
            this.Unbind();
            _bookmark = bookmark;
            SerializedObject serializedObject = new SerializedObject(bookmark);
            _titleText.bindingPath = "bookmarkTitle";
            _titleText.Bind(serializedObject);
            _colorField.bindingPath = "bgColor";
            _colorField.Bind(serializedObject);
            
            this.TrackSerializedObjectValue(serializedObject, UpdateBookmark);
            CheckMark();
            SetUpFocusBtn(graphView);
        }

        private void UpdateBookmark(SerializedObject ob)
        {
            CheckMark();
        }

        private void CheckMark()
        {
            if (_bookmark.goToNode != null)
            {
                _mark.AddToClassList("hidden");
                _markSelected.RemoveFromClassList("hidden");
                _focusBtn.RemoveFromClassList("hidden");
            }
            else
            {
                _markSelected.AddToClassList("hidden");
                _focusBtn.AddToClassList("hidden");
                _mark.RemoveFromClassList("hidden");
            }
        }

        private void SetUpFocusBtn(DialogSystemView graphView)
        {
            _focusBtn.clickable = new Clickable(() =>
            {
                if (_bookmark.goToNode != null)
                {
                    graphView.FrameNode(_bookmark.goToNode.guid);
                }
            });
        }
    }
}
