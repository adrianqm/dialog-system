using UnityEngine.UIElements;

public class StartNodeViewContainer : VisualElement
{
    public new class UxmlFactory: UxmlFactory<StartNodeViewContainer, UxmlTraits> { }
    
    private readonly string uxmlName = "StartNodeViewContainer";
    public StartNodeViewContainer()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath, uxmlName);
        uxml.CloneTree(this);
    }
}