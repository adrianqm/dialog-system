using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogNodeViewContainer : VisualElement {
    public new class UxmlFactory: UxmlFactory<StartNodeViewContainer, UxmlTraits> { }
        
    private readonly string uxmlName = "DialogNodeViewContainer";
    public DialogNodeViewContainer()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath, uxmlName);
        uxml.CloneTree(this);
    }
}
