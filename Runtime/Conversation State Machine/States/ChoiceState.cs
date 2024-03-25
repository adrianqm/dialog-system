using System.Collections.Generic;
#if LOCALIZATION_EXIST
using UnityEngine.Localization.Settings;
#endif

namespace AQM.Tools
{
    public class ChoiceState : ConversationState
    {
        private readonly ConversationStateMachine _stateMachine;
        private ChoiceNodeSO _currentNode;
        
        public ChoiceState(ConversationContext context, ConversationStateMachine.EConversationState key) : base(context, key)
        {
            _stateMachine = context.ConversationStateMachine;
        }

        public override void EnterState()
        {
            _currentNode = context.CurrentNode as ChoiceNodeSO;
            DSNode dsData = GetData();
            _stateMachine.onShowNode(dsData);
        }

        public override void ExitState()
        {
            _currentNode.OnCompleteNode();
        }

        public override void GetNextNode(int option = 0)
        {
            NodeSO nextNode;
            if (option >= 0)
            {
                _currentNode.choices[option].OnSelected();
                nextNode = _currentNode.choices[option].port.targetNodes[0];
            }
            else
            {
                _currentNode.OnDefaultSelected();
                nextNode = _currentNode.defaultPort.targetNodes[0];
            }
            _stateMachine.TransitionToNode(nextNode);
        }
        
        public override DSNode GetCurrentData()
        {
            return GetData();
        }
        
        private DSNode GetData()
        {
#if LOCALIZATION_EXIST
            DialogSystemDatabase db = context.Database;
            if (!db.localizationActivated) return _currentNode.GetData();
            string message = "";
            List<string> choices = new List<string>();
            var translatedValue =
                LocalizationSettings.StringDatabase.GetLocalizedString(db.tableCollectionName, _currentNode.guid);
            if(translatedValue != null)
            {
                message = translatedValue;
            }
            
            _currentNode.choices.ForEach(c =>
            {
                var choiceEntry = LocalizationSettings.StringDatabase.GetLocalizedString(db.tableCollectionName, c.guid);
                if(choiceEntry != null)
                {
                    choices.Add(choiceEntry);
                }
            });
            return new DSChoice(_currentNode.actor,message,choices);
#else
            return _currentNode.GetData();
#endif
        }
    }
}
