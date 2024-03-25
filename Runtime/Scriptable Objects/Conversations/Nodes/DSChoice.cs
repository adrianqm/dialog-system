using System;
using System.Collections.Generic;

namespace AQM.Tools
{
    public class DSChoice: DSNode
    {
        private string _message;
        private List<string> _choices;
        public Action<int> onChoiceSelected;
        
        public string Message => _message;
        public List<string> Choices => _choices;

        public DSChoice(Actor actor, string message, List<string> choices) : base(actor)
        {
            _message = message;
            _choices = choices;
        }
    }
}