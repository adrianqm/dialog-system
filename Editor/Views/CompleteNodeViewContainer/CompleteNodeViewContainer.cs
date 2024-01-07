using UnityEngine.UIElements;

public class CompleteNodeViewContainer : VisualElement
{
    public new class UxmlFactory: UxmlFactory<CompleteNodeViewContainer, UxmlTraits> { }
    
    private readonly string uxmlName = "CompleteNodeViewContainer";
    public CompleteNodeViewContainer()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath, uxmlName);
        uxml.CloneTree(this);
    }
}