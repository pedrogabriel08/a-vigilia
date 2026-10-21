using UnityEngine;

namespace Vigilia
{
    public sealed class FireAttack : AttackEvent
    {
        [SerializeField] private QTEManager qte;
        [SerializeField] private GameObject smokeSignal;
        [SerializeField] private GameObject flameSignal;
        [SerializeField, Min(1)] private int requiredPresses = 12;
        [SerializeField, Min(.1f)] private float qteDuration = 8f;
        [SerializeField, Min(0f)] private float damagePerSecond = 5f;
        public void RepelDuringPreAttack()
        { if (Phase == AttackPhase.PreAttack && Game != null && Game.IsPlaying) Finish(); }
        protected override void OnPreAttackStarted()
        {
            if (smokeSignal != null) smokeSignal.SetActive(true);
            if (flameSignal != null) flameSignal.SetActive(true);
        }
        protected override void OnAttackStarted()
        {
            if (Game.Baby.IsProtected || qte == null) { Finish(); return; }
            if (!qte.StartMashQTE(requiredPresses, qteDuration, OnResult, ApplyDamage)) Finish();
        }
        private void ApplyDamage(float seconds)
        { if (Game != null && Game.IsPlaying) Game.Baby.TakeDamage(damagePerSecond * seconds); }
        private void OnResult(QTEResult result) { if (Phase == AttackPhase.Active) Finish(); }
        protected override void OnCleanup()
        {
            if (qte != null) qte.Cancel();
            if (smokeSignal != null) smokeSignal.SetActive(false);
            if (flameSignal != null) flameSignal.SetActive(false);
        }
    }
}
