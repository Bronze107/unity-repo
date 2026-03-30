namespace Game.Events
{
    public struct DamageEvent : IGameEvent
    {
        public int TargetId;
        public int Amount;

        public DamageEvent(int targetId, int amount)
        {
            TargetId = targetId;
            Amount = amount;
        }
    }

    public struct DeadEvent : IGameEvent
    {
        public int TargetId;

        public DeadEvent(int targetId)
        {
            TargetId = targetId;
        }
    }

    public struct GoldChangedEvent : IGameEvent
    {
        public int CurrentGold;

        public GoldChangedEvent(int currentGold)
        {
            CurrentGold = currentGold;
        }
    }
}
