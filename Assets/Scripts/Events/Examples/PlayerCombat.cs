using UnityEngine;
using Game.Events;

namespace Game.Events.Examples
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        public void TakeDamage(int targetId, int damage)
        {
            EventBus.Global.Publish(new DamageEvent(targetId, damage));

            if (damage > 100)
            {
                EventBus.Global.Publish(new DeadEvent(targetId));
            }
        }
    }
}
