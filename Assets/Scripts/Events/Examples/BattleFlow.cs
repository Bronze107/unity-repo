using UnityEngine;
using Game.Events;

namespace Game.Events.Examples
{
    public sealed class BattleFlow : MonoBehaviour
    {
        private BattleContext _battleContext;

        private void Awake()
        {
            _battleContext = new BattleContext();
            _battleContext.Bus.Subscribe(this, OnBattleDamage);
        }

        private void OnDestroy()
        {
            _battleContext.Bus.RemoveOwner(this);
        }

        public void SimulateBattleHit(int targetId, int damage)
        {
            _battleContext.Bus.Publish(new DamageEvent(targetId, damage));
        }

        private void OnBattleDamage(DamageEvent evt)
        {
            Debug.Log($"[Local Battle Bus] Target:{evt.TargetId} Damage:{evt.Amount}");
        }
    }
}
