using UnityEngine.UIElements;

public class BranchNodeViewContainer: VisualElement
{
    public new class UxmlFactory: UxmlFactory<BranchNodeViewContainer, UxmlTraits> { }
    
    private readonly string uxmlName = "BranchNodeViewContainer";
    public BranchNodeViewContainer()
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath, uxmlName);
        uxml.CloneTree(this);
    }
}