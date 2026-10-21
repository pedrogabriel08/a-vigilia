using System;
using UnityEngine;

namespace Vigilia
{
    public enum AttackPhase { Idle, PreAttack, Active }

    // Ciclo comum. Cada ameaça implementa apenas sua apresentação e reação.
    public abstract class AttackEvent : MonoBehaviour
    {
        [SerializeField, Min(.1f)] private float preAttackDuration = 5f;

        protected GameManager Game { get; private set; }
        public float PreAttackDuration => preAttackDuration;
        public AttackPhase Phase { get; private set; }
        public virtual bool CanStart => isActiveAndEnabled && Phase == AttackPhase.Idle;
        public event Action<AttackEvent> Finished;

        public void BeginPreAttack(GameManager game)
        {
            if (!CanStart || game == null || !game.IsPlaying)
                return;
            Game = game;
            Phase = AttackPhase.PreAttack;
            OnPreAttackStarted();
        }

        public void BeginAttack()
        {
            if (Phase != AttackPhase.PreAttack || Game == null || !Game.IsPlaying)
                return;
            Phase = AttackPhase.Active;
            OnAttackStarted();
        }

        public void Cancel() => Finish();

        protected void Finish()
        {
            if (Phase == AttackPhase.Idle)
                return;
            Phase = AttackPhase.Idle;
            OnCleanup();
            Game = null;
            Finished?.Invoke(this);
        }

        protected virtual void OnDisable() => Finish();
        protected abstract void OnPreAttackStarted();
        protected abstract void OnAttackStarted();
        protected abstract void OnCleanup();
    }
}
