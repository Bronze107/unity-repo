using UnityEngine;

namespace Game.Events
{
    public sealed class EventScope : MonoBehaviour
    {
        [SerializeField] private bool unbindOnDisable = true;
        [SerializeField] private bool useGlobalBus = true;

        private EventBus _localBus;

        public EventBus Bus => useGlobalBus ? EventBus.Global : (_localBus ??= new EventBus());

        private void OnDisable()
        {
            if (unbindOnDisable)
            {
                Bus.RemoveOwner(this);
            }
        }

        private void OnDestroy()
        {
            Bus.RemoveOwner(this);
        }
    }
}
