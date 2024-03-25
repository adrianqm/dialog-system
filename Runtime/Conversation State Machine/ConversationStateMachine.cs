using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace AQM.Tools
{
    public class ConversationStateMachine : StateMachine<ConversationStateMachine. EConversationState>
    {
        public Action onConversationStarted;
        public Action<DSNode> onShowNode;
        public Action onConversationEnded;
        
        public enum EConversationState
        {
            Idle, Start, Complete, Dialog, Choice, Branch, Bookmark, Time
        }

        private ConversationTree _conversation;

        private ConversationContext _context;
        
        public void Initialize(DialogSystemDatabase db , ConversationTree conversation)
        {
            _conversation = conversation;
            _context = new ConversationContext(this,db,_conversation);
            InitializeStates();
        }

        public void GetNextStep (int option = 0)
        {
            ConversationState state = currentState as ConversationState;
            state.GetNextNode(option);
        }

        public DSNode GetCurrentNode()
        {
            ConversationState state = currentState as ConversationState;
            return state.GetCurrentData();
        }

        public void TransitionToNode(NodeSO nextNode)
        {
            if (nextNode == null)
            {
                FinalizeConversation();
                return;
            }
            
            UpdateStates(nextNode);
            _context.CurrentNode = nextNode;
            
            TransitionToState(EConversationState.Idle);
            switch (nextNode)
            {
                case StartNodeSO: QueueStateTransition(EConversationState.Start); break;
                case CompleteNodeSO: QueueStateTransition(EConversationState.Complete); break;
                case DialogNodeSO: QueueStateTransition(EConversationState.Dialog); break;
                case ChoiceNodeSO: QueueStateTransition(EConversationState.Choice); break;
                case BranchNodeSO: QueueStateTransition(EConversationState.Branch); break;
                case BookmarkNodeSO: QueueStateTransition(EConversationState.Bookmark); break;
                case TimerNodeSO: QueueStateTransition(EConversationState.Time); break;
            }
        }

        public void FinalizeConversation()
        {
            _conversation.conversationState = ConversationTree.State.Completed;
            UpdateStates(_context.CurrentNode);
            onConversationEnded.Invoke();
        }

        private void UpdateStates(NodeSO nextChild)
        {
#if UNITY_EDITOR
            List<NodeSO> visitedNodes = CustomDFS.StartDFS(nextChild);
            foreach (var n in _conversation.nodes)
            {
                if (nextChild.guid == n.guid)
                {
                    if (nextChild is not CompleteNodeSO) { nextChild.OnRunning(); }
                    else { nextChild.OnCompleted(); }
                    continue;
                }
                if (visitedNodes.Find(vn => vn.guid == n.guid))
                {
                    n.NodeState = NodeSO.State.Initial;
                    continue;
                }
                if (n.NodeState == NodeSO.State.Visited) n.NodeState = NodeSO.State.VisitedUnreachable;
                else if(n.NodeState != NodeSO.State.VisitedUnreachable) n.NodeState = NodeSO.State.Unreachable;
            }
            _conversation.UpdateStatesEvent();
#endif
        }

        private void InitializeStates()
        {
            states.Add(EConversationState.Idle, new IdleState(_context,  EConversationState.Idle));
            states.Add(EConversationState.Start, new StartState(_context,  EConversationState.Start));
            states.Add(EConversationState.Complete, new CompleteState(_context,  EConversationState.Complete));
            states.Add(EConversationState.Dialog, new DialogState(_context,  EConversationState.Dialog));
            states.Add(EConversationState.Choice, new ChoiceState(_context,  EConversationState.Choice));
            states.Add(EConversationState.Branch, new BranchState(_context,  EConversationState.Branch));
            states.Add(EConversationState.Bookmark, new BookmarkState(_context,  EConversationState.Bookmark));
            states.Add(EConversationState.Time, new TimeState(_context,  EConversationState.Time));
            
            _context.CurrentNode = _conversation.startNode;
            _conversation.conversationState = ConversationTree.State.Running;
            currentState = states[EConversationState.Start];
        }
    }
}
