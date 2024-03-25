
using System;

namespace AQM.Tools
{
    public abstract class BaseState<TEState> where TEState : Enum
    {
        public BaseState(TEState key)
        {
            StateKey = key;
        }
        public TEState StateKey { get; private set; }
        
        public virtual void EnterState() {}
        public virtual void ExitState() {}
        public virtual void UpdateState() {}
    }  
}
