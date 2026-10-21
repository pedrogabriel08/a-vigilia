using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vigilia
{
    public enum ItemId { Scissors, Rosary, Broom }

    [Serializable]
    public sealed class ItemSettings
    {
        public ItemId item = ItemId.Scissors;
        [Min(0f)] public float cooldownSeconds = 5f;
    }

    [DisallowMultipleComponent]
    public sealed class ItemManager : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private QTEManager qte;
        [SerializeField] private ItemSettings[] items = { new ItemSettings() };
        private readonly Dictionary<ItemId, float> cooldownEnds = new Dictionary<ItemId, float>();
        private float elapsed;

        public ItemId? HeldItem { get; private set; }
        public bool IsPlaying => isActiveAndEnabled && game != null && game.IsPlaying;
        public bool CanUseItems => IsPlaying && (qte == null || !qte.IsActive);
        public event Action Changed;

        private void OnEnable()
        {
            ResetItems();
            if (game != null)
                game.StateChanged += OnGameStateChanged;
            if (qte != null) qte.Started += ReturnHeldItem;
        }

        private void OnDisable()
        {
            if (game != null)
                game.StateChanged -= OnGameStateChanged;
            if (qte != null) qte.Started -= ReturnHeldItem;
            ReturnHeldItem();
        }

        private void Update() => AdvanceCooldowns(Time.deltaTime);

        private void AdvanceCooldowns(float deltaTime)
        {
            if (!IsPlaying || deltaTime <= 0f)
                return;
            bool coolingDown = false;
            foreach (float end in cooldownEnds.Values)
                if (end > elapsed)
                {
                    coolingDown = true;
                    break;
                }
            elapsed += deltaTime;
            if (coolingDown)
                Changed?.Invoke();
        }

        public float GetCooldownRemaining(ItemId item) =>
            cooldownEnds.TryGetValue(item, out float end) ? Mathf.Max(0f, end - elapsed) : 0f;

        public float GetCooldownDuration(ItemId item)
        {
            if (items != null)
                foreach (var settings in items)
                    if (settings != null && settings.item == item)
                        return Mathf.Max(0f, settings.cooldownSeconds);
            return 0f;
        }

        public bool IsReady(ItemId item) => cooldownEnds.ContainsKey(item) && GetCooldownRemaining(item) <= 0f;

        public bool TryPickUp(ItemId item)
        {
            if (!CanUseItems || HeldItem.HasValue || !IsReady(item))
                return false;
            HeldItem = item;
            Changed?.Invoke();
            return true;
        }

        public bool TryUseOn(ItemDropTarget target)
        {
            if (!HeldItem.HasValue)
                return false;
            ItemId item = HeldItem.Value;
            if (!CanUseItems || target == null || !target.Accepts(item))
            {
                ReturnHeldItem();
                return false;
            }

            // Local válido consome o uso, mesmo quando não existe uma ameaça.
            HeldItem = null;
            cooldownEnds[item] = elapsed + GetCooldownDuration(item);
            Changed?.Invoke();
            target.ApplyItem();
            return true;
        }

        public void ReturnHeldItem()
        {
            if (!HeldItem.HasValue)
                return;
            HeldItem = null;
            Changed?.Invoke();
        }

        private void OnGameStateChanged(GameState state) => ResetItems();

        private void ResetItems()
        {
            HeldItem = null;
            elapsed = 0f;
            cooldownEnds.Clear();
            if (items != null)
                foreach (var settings in items)
                    if (settings != null && !cooldownEnds.ContainsKey(settings.item))
                        cooldownEnds.Add(settings.item, 0f);
            Changed?.Invoke();
        }
    }
}
