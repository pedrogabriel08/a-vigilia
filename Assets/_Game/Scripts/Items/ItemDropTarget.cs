using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Vigilia
{
    // Área de interação independente da arte. A resposta é conectada no Inspector.
    public sealed class ItemDropTarget : MonoBehaviour, IDropHandler
    {
        [SerializeField] private ItemId acceptedItem = ItemId.Scissors;
        [SerializeField] private UnityEvent onUsed = new UnityEvent();
        public UnityEvent OnUsed => onUsed;

        public bool Accepts(ItemId item) => isActiveAndEnabled && item == acceptedItem;
        public void ApplyItem() => onUsed.Invoke();

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null &&
                eventData.pointerDrag.TryGetComponent<DraggableItem>(out var dragged))
                dragged.DropOn(this);
        }
    }
}
