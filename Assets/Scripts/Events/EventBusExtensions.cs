using System;

namespace Game.Events
{
    public static class EventBusExtensions
    {
        public static EventBinding<T> Subscribe<T>(this EventBus bus, object owner, Action<T> callback)
            where T : struct, IGameEvent
        {
            return bus.Subscribe(callback, owner);
        }

        public static void Unsubscribe<T>(this EventBus bus, object owner, Action<T> callback)
            where T : struct, IGameEvent
        {
            bus.Unsubscribe(callback, owner);
        }
    }
}
