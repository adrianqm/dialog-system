namespace AQM.Tools
{
    public class DSDialog: DSNode
    {
        
        private string _message;

        public string Message => _message;
        
        public DSDialog(Actor actor, string message) : base(actor)
        {
            _message = message;
        }
    }
}