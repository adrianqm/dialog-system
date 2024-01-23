namespace AQM.Tools
{
    public class DSDialog: DSNode
    {
        
        private string _message;

        public string Message => _message;
        
        public DSDialog(Actor actor, string message, float delayTime) : base(actor,delayTime)
        {
            _message = message;
        }
    }
}