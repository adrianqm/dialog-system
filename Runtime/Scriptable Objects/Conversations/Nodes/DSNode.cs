
namespace AQM.Tools
{
    public class DSNode
    {
        private Actor _actor;
        private float _delayTime;

        public Actor Actor => _actor;
        public float DelayTime => _delayTime;

        protected DSNode(Actor actor, float delayTime)
        {
            _actor = actor;
            _delayTime = delayTime;
        }
    }
}