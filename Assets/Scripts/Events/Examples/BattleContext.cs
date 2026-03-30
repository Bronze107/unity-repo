using Game.Events;

namespace Game.Events.Examples
{
    public sealed class BattleContext
    {
        public EventBus Bus { get; } = new EventBus();
    }
}
