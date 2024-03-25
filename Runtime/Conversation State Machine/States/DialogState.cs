#if LOCALIZATION_EXIST
using UnityEngine.Localization.Settings;
#endif

namespace AQM.Tools
{
    public class DialogState : ConversationState
    {
        private readonly ConversationStateMachine _stateMachine;
        private DialogNodeSO _currentNode;
        
        public DialogState(ConversationContext context, ConversationStateMachine.EConversationState key) : base(context, key)
        {
            _stateMachine = context.ConversationStateMachine;
        }

        public override void EnterState()
        {
            _currentNode = context.CurrentNode as DialogNodeSO;
            DSNode dsData = GetData();
            _stateMachine.onShowNode(dsData);
        }
        
        public override void ExitState()
        {
            _currentNode.OnCompleteNode();
        }

        public override void GetNextNode(int option = 0)
        {
            NodeSO nextNode = _currentNode.outputPort.targetNodes[0];
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
            DSDialog dsDialog = null;
            var translatedValue =
                LocalizationSettings.StringDatabase.GetLocalizedString(db.tableCollectionName, _currentNode.guid);
            if(translatedValue != null)
            {
                dsDialog = new DSDialog(_currentNode.actor, translatedValue);
            }
            return dsDialog;
#else
            return _currentNode.GetData();
#endif
        }
    }
}
