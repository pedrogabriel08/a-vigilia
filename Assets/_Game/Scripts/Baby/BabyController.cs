using System;
using UnityEngine;

namespace Vigilia
{
    [DisallowMultipleComponent]
    public sealed class BabyController : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;

        private bool damageEnabled;
        private float protectionRemaining;

        public float Health { get; private set; }
        public float MaxHealth => maxHealth;
        public float ProtectionRemaining => protectionRemaining;
        public bool IsProtected => protectionRemaining > 0f;
        public bool IsAlive => Health > 0f;

        public event Action<float, float> HealthChanged;
        public event Action<bool> ProtectionChanged;
        public event Action Died;

        private void Awake() => ResetState();
        private void Update() => AdvanceProtection(Time.deltaTime);

        public void ResetState()
        {
            Health = maxHealth;
            ClearProtection();
            HealthChanged?.Invoke(Health, maxHealth);
        }

        public void SetDamageEnabled(bool value) => damageEnabled = value;

        // Aceita frações de vida: o QTE poderá enviar danoPorSegundo * deltaTime.
        public bool TakeDamage(float amount)
        {
            if (!damageEnabled || !IsAlive || IsProtected || amount <= 0f ||
                float.IsNaN(amount) || float.IsInfinity(amount))
                return false;

            Health = Mathf.Max(0f, Health - amount);
            HealthChanged?.Invoke(Health, maxHealth);
            if (!IsAlive)
                Died?.Invoke();
            return true;
        }

        public void Protect(float duration)
        {
            if (!damageEnabled || !IsAlive || duration <= 0f ||
                float.IsNaN(duration) || float.IsInfinity(duration))
                return;

            bool wasProtected = IsProtected;
            protectionRemaining = duration;
            if (!wasProtected)
                ProtectionChanged?.Invoke(true);
        }

        public void ClearProtection()
        {
            bool wasProtected = IsProtected;
            protectionRemaining = 0f;
            if (wasProtected)
                ProtectionChanged?.Invoke(false);
        }

        private void AdvanceProtection(float deltaTime)
        {
            if (!damageEnabled || !IsProtected || deltaTime <= 0f)
                return;

            protectionRemaining = Mathf.Max(0f, protectionRemaining - deltaTime);
            if (!IsProtected)
                ProtectionChanged?.Invoke(false);
        }
    }
}
