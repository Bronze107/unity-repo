using UnityEngine;
using Game.Events;

namespace Game.Events.Examples
{
    public sealed class BattleLogUI : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Global.Subscribe(this, OnDamage);
            EventBus.Global.Subscribe(this, OnDead);
        }

        private void OnDisable()
        {
            EventBus.Global.RemoveOwner(this);
        }

        private void OnDamage(DamageEvent evt)
        {
            Debug.Log($"Target:{evt.TargetId} Damage:{evt.Amount}");
        }

        private void OnDead(DeadEvent evt)
        {
            Debug.Log($"Target:{evt.TargetId} Dead");
        }
    }
}
