using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class TimeState : ConversationState
    {
        private readonly ConversationStateMachine _stateMachine;
        private TimerNodeSO _currentNode;
        public float _totalTimeToWait;
        private float _elapsedTime;
        private bool _actionPerformed;

        public TimeState(ConversationContext context, ConversationStateMachine.EConversationState key) : base(context, key)
        {
            _stateMachine = context.ConversationStateMachine;
        }

        public override void EnterState()
        {
            _currentNode = context.CurrentNode as TimerNodeSO;
            _actionPerformed = false;
            _totalTimeToWait = _currentNode.time;
            _elapsedTime = 0;
            _currentNode.timeRemaining = _totalTimeToWait;
        }

        public override void UpdateState()
        {
            if (_actionPerformed) return;
            _elapsedTime += Time.deltaTime;
            
            _currentNode.timeRemaining = (int) Math.Ceiling(_totalTimeToWait - _elapsedTime);
            
            if (!(_elapsedTime >= _totalTimeToWait)) return;
            
            GetNextNode();
            _actionPerformed = true;
        }

        public override void GetNextNode(int option = 0)
        {
            NodeSO nextNode = _currentNode.outputPort.targetNodes[0];
            _stateMachine.TransitionToNode(nextNode);
        }
    }
}
