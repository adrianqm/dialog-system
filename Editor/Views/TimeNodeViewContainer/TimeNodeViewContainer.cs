using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TimeNodeViewContainer: VisualElement
{
    public new class UxmlFactory: UxmlFactory<TimeNodeViewContainer, UxmlTraits> { }
    
    private readonly string uxmlName = "TimeNodeViewContainer";
    public TimeNodeViewContainer()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath, uxmlName);
        uxml.CloneTree(this);
    }
}