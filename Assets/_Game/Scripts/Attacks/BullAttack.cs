using UnityEngine;

namespace Vigilia
{
    [DisallowMultipleComponent]
    public sealed class BullAttack : AttackEvent
    {
        [SerializeField] private BullVisual[] bulls;
        [SerializeField] private QTEManager qte;
        [SerializeField, Min(1)] private int requiredPresses = 12;
        [SerializeField, Min(.1f)] private float qteDuration = 8f;
        [SerializeField, Min(0f)] private float damagePerSecond = 5f;
        public BullVisual SelectedBull { get; private set; }

        public override bool CanStart => base.CanStart && CountAvailableBulls() > 0;

        public void RepelDuringPreAttack()
        {
            if (Phase == AttackPhase.PreAttack && Game != null && Game.IsPlaying)
                Finish();
        }

        protected override void OnPreAttackStarted()
        {
            int choice = Random.Range(0, CountAvailableBulls());
            foreach (var bull in bulls)
            {
                if (bull == null || !bull.isActiveAndEnabled)
                    continue;
                if (choice-- != 0)
                    continue;
                SelectedBull = bull;
                SelectedBull.SetStrange(true);
                return;
            }
            Finish();
        }

        protected override void OnAttackStarted()
        {
            if (Game.Baby.IsProtected || qte == null) { Finish(); return; }
            if (!qte.StartMashQTE(requiredPresses, qteDuration, OnQTEResult, ApplyQTEDamage)) Finish();
        }

        private void ApplyQTEDamage(float seconds)
        {
            if (Game != null && Game.IsPlaying) Game.Baby.TakeDamage(damagePerSecond * seconds);
        }
        private void OnQTEResult(QTEResult result) { if (Phase == AttackPhase.Active) Finish(); }

        protected override void OnCleanup()
        {
            if (qte != null) qte.Cancel();
            if (SelectedBull != null)
                SelectedBull.SetStrange(false);
            SelectedBull = null;
        }

        private int CountAvailableBulls()
        {
            int count = 0;
            if (bulls == null)
                return count;
            foreach (var bull in bulls)
                if (bull != null && bull.isActiveAndEnabled)
                    count++;
            return count;
        }
    }
}
