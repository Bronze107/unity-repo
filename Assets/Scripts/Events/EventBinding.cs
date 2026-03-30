using System;

namespace Game.Events
{
    public readonly struct EventBinding<T> : IEventBinding where T : struct, IGameEvent
    {
        private readonly EventBus _bus;
        private readonly Action<T> _callback;
        private readonly object _owner;

        public EventBinding(EventBus bus, Action<T> callback, object owner)
        {
            _bus = bus;
            _callback = callback;
            _owner = owner;
        }

        public void Dispose()
        {
            if (_bus == null || _callback == null)
            {
                return;
            }

            _bus.Unsubscribe(_callback, _owner);
        }
    }
}
