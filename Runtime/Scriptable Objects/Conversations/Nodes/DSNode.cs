
namespace AQM.Tools
{
    public class DSNode
    {
        private Actor _actor;

        public Actor Actor => _actor;

        protected DSNode(Actor actor)
        {
            this._actor = actor;
        }
    }
}