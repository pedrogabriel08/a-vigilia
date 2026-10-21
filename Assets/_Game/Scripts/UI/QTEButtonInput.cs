using UnityEngine;
using UnityEngine.EventSystems;

namespace Vigilia
{
    public sealed class QTEButtonInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private QTEManager qte;
        public void OnPointerDown(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left) qte.SetHolding(true); }
        public void OnPointerUp(PointerEventData e) => qte.SetHolding(false);
        public void OnPointerExit(PointerEventData e) => qte.SetHolding(false);
        private void OnDisable() { if (qte != null) qte.SetHolding(false); }
    }
}
