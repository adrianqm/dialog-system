using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public abstract class StateMachine<TEState> : MonoBehaviour where TEState : Enum
    {
        protected Dictionary<TEState, BaseState<TEState>> states = new ();
        protected BaseState<TEState> currentState;
        protected bool isTransitioningState = false;
        private TEState _queuedState;
        
        void Start()
        {
            currentState.EnterState();
        }

        void Update()
        {
            if (!isTransitioningState && _queuedState.Equals(currentState.StateKey)) {
                currentState.UpdateState();
            }
            else if(!isTransitioningState) {
                TransitionToState(_queuedState);
            }
        }
        
        public void QueueStateTransition(TEState stateKey)
        {
            _queuedState = stateKey; // Queue the next state to transition to
        }

        public void TransitionToState(TEState stateKey)
        {
            isTransitioningState = true;
            currentState.ExitState();
            currentState = states[stateKey];
            currentState.EnterState();
            isTransitioningState = false;
        }
    }
}
