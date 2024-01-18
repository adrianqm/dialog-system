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

        private TextField _titleText;
        private ColorField _colorField;
        private Button _focusBtn;
        
        public BookmarkView()
        {
            string path = Path.Combine(DialogSystemEditor.RelativePath, AssetName);
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            uxml.CloneTree(this);
            
            _titleText = this.Q<TextField>();
            _colorField = this.Q<ColorField>();
            _focusBtn = this.Q<Button>();
            _focusBtn.Add(new Image {
                image = EditorGUIUtility.IconContent("d_Selectable Icon").image
            });
        }

        public void BindCondition(BookmarkSO bookmark)
        {
            if (bookmark == null) return;
            
            _titleText.bindingPath = "bookmarkTitle";
            _titleText.Bind(new SerializedObject(bookmark));
            _titleText.value = bookmark.bookmarkTitle;
            
            _colorField.bindingPath = "bgColor";
            _colorField.Bind(new SerializedObject(bookmark));
        }
    }
}
