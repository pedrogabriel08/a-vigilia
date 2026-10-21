using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vigilia
{
    public sealed class WardrobeController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameManager game;
        [SerializeField] private QTEManager qte;
        [SerializeField] private ItemManager items;
        [SerializeField] private GameObject contents;
        [SerializeField] private Text label;
        public bool IsOpen { get; private set; }

        private void OnEnable()
        {
            if (game != null)
                game.StateChanged += OnGameStateChanged;
            if (qte != null) qte.Started += Close;
            SetOpen(false);
        }

        private void OnDisable()
        {
            if (game != null)
                game.StateChanged -= OnGameStateChanged;
            if (qte != null) qte.Started -= Close;
            SetOpen(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && game != null && game.IsPlaying &&
                (qte == null || !qte.IsActive))
                SetOpen(!IsOpen);
        }

        private void OnGameStateChanged(GameState state) => SetOpen(false);
        private void Close() => SetOpen(false);

        private void SetOpen(bool value)
        {
            IsOpen = value;
            if (!value && items != null)
                items.ReturnHeldItem();
            if (contents != null)
                contents.SetActive(value);
            if (label != null)
                label.text = value ? "ARMÁRIO · FECHAR" : "ARMÁRIO · ABRIR";
        }
    }
}
