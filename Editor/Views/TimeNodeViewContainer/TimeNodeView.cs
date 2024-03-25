
using AQM.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class TimeNodeView  : NodeView
{
    private TimerNodeSO _timerNodeSo;
    private FloatField _floatField;
    private Label _elapsedLabel;
    private VisualElement _elapsedTimeContainer;
    
    public TimeNodeView(NodeSO nodeSo) : base(nodeSo)
    {
        topContainer.style.flexDirection = FlexDirection.Row;
        inputContainer.AddToClassList("singleInputContainer");
        outputContainer.AddToClassList("singleOutputContainer");
        
        var nodeViewContainer = new TimeNodeViewContainer();
        extensionContainer.Add(nodeViewContainer);
        CreateDefaultInputPorts();
        CreateDefaultOutputPorts();
        
        BindTime(node);
        RefreshExpandedState();
    }

    private void BindTime(NodeSO node){
        _timerNodeSo = node as TimerNodeSO;

        SerializedObject timeObject = new SerializedObject(_timerNodeSo);
        
        _floatField = this.Q<FloatField>("time-float");
        _floatField.bindingPath = "time";
        _floatField.Bind(timeObject);

        _elapsedTimeContainer = this.Q("elapsed-time");
        _elapsedTimeContainer.Unbind();
        _elapsedLabel = this.Q<Label>("elapsed-time-label");
        _elapsedLabel.bindingPath = "timeRemaining";
        _elapsedLabel.Bind(timeObject);
        _elapsedTimeContainer.TrackSerializedObjectValue(timeObject, HandleElapsedTimeChanges);
    }
    
    private void HandleElapsedTimeChanges(SerializedObject serializedObject)
    {
        _elapsedTimeContainer.style.visibility = _timerNodeSo.timeRemaining > 0 ? Visibility.Visible : Visibility.Hidden;
    }
}
