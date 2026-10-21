using UnityEngine;
using UnityEngine.UI;

namespace Vigilia
{
    public sealed class QTEView : MonoBehaviour
    {
        [SerializeField] private QTEManager qte;
        [SerializeField] private BabyController baby;
        [SerializeField] private Text babyHealth;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text title, detail, timer, buttonLabel;
        [SerializeField] private Image progress;
        private void Update()
        {
            bool active = qte != null && qte.IsActive;
            if (panel.activeSelf != active) panel.SetActive(active);
            if (!active) return;
            if(babyHealth!=null&&baby!=null)
            {
                babyHealth.text=$"VIDA DO BEBÊ  {Mathf.CeilToInt(baby.Health)} / {Mathf.CeilToInt(baby.MaxHealth)}";
                babyHealth.color=baby.Health<35f?new Color(1f,.45f,.4f):Color.white;
            }
            bool mash = qte.Type == QTEType.Mash;
            title.text = mash ? "AFASTE A BRUXA" : "SEGURE A PORTA";
            detail.text = mash ? $"{qte.Progress:0} / {qte.Required:0} · CLIQUE OU PRESSIONE ESPAÇO"
                : $"RESISTÊNCIA  {qte.Progress:0.0} / {qte.Required:0.0}s";
            timer.text = $"{qte.TimeRemaining:0.0}s";
            buttonLabel.text = mash ? "REPELIR — CLIQUE RAPIDAMENTE" : "MANTENHA PRESSIONADO";
            progress.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(qte.Progress / qte.Required), 1f);
        }
    }
}
