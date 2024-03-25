
using AQM.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class TimeInspectorView : VisualElement
{
    private readonly string assetName = "TimeInspectorView";
    
    public TimeInspectorView(TimerNodeSO node)
    {
        VisualTreeAsset uxml = UIToolkitLoader.LoadUXML(DialogSystemEditor.RelativePath,assetName);
        uxml.CloneTree(this);
        
        BindTime(node);
    }
    private void BindTime(TimerNodeSO node){
        FloatField field = this.Q<FloatField>("time-float");
        field.bindingPath = "time";
        field.Bind(new SerializedObject(node));
    }
}