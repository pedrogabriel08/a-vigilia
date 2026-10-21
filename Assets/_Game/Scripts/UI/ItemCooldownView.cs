using UnityEngine;
using UnityEngine.UI;

namespace Vigilia
{
    public sealed class ItemCooldownView : MonoBehaviour
    {
        [SerializeField] private ItemManager items;
        [SerializeField] private ItemId item = ItemId.Scissors;
        [SerializeField] private Text label;
        [SerializeField] private Image progress;

        private void OnEnable()
        {
            if (items != null)
                items.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (items != null)
                items.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (items == null || label == null || progress == null)
                return;
            float remaining = items.GetCooldownRemaining(item);
            float duration = items.GetCooldownDuration(item);
            string name = item == ItemId.Rosary ? "TERÇO" : item == ItemId.Broom ? "VASSOURA" : "TESOURA";
            label.text = remaining > 0f ? $"{name} · {Mathf.CeilToInt(remaining)}s" : $"{name} · PRONTO";
            float ready = duration > 0f ? 1f - remaining / duration : 1f;
            progress.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(ready), 1f);
        }
    }
}
