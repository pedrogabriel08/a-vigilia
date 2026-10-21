using UnityEngine;
using UnityEngine.EventSystems;

namespace Vigilia
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public sealed class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler,
        IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private ItemManager items;
        [SerializeField] private ItemId item = ItemId.Scissors;
        [SerializeField] private RectTransform dragLayer;
        private RectTransform rect;
        private CanvasGroup group;
        private Transform home;
        private Vector2 homeMin, homeMax, homePivot, homeSize, homePosition;
        private Vector3 homeScale;
        private Quaternion homeRotation;
        private int homeIndex;
        private bool dragging;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            group = GetComponent<CanvasGroup>();
        }

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
            if (dragging)
            {
                if (items != null && items.HeldItem == item)
                    items.ReturnHeldItem();
                ReturnHome();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left ||
                items == null || dragLayer == null || !items.TryPickUp(item))
                return;

            home = rect.parent;
            homeIndex = rect.GetSiblingIndex();
            homeMin = rect.anchorMin;
            homeMax = rect.anchorMax;
            homePivot = rect.pivot;
            homeSize = rect.sizeDelta;
            homePosition = rect.anchoredPosition;
            homeScale = rect.localScale;
            homeRotation = rect.localRotation;
            Vector2 visibleSize = rect.rect.size;
            dragging = true;
            rect.SetParent(dragLayer, true);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = visibleSize;
            group.blocksRaycasts = false;
            MoveTo(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragging)
                MoveTo(eventData);
        }

        public void DropOn(ItemDropTarget target)
        {
            if (dragging)
                items.TryUseOn(target);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging)
                return;
            if (items != null && items.HeldItem == item)
                items.ReturnHeldItem();
            ReturnHome();
            Refresh();
        }

        // Um clique simples no item não fecha o armário por propagação.
        public void OnPointerClick(PointerEventData eventData) { }

        private void MoveTo(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer,
                eventData.position, eventData.pressEventCamera, out var position))
                rect.anchoredPosition = position;
        }

        private void Refresh()
        {
            if (group == null)
                return;
            if (dragging && (items == null || items.HeldItem != item))
                ReturnHome();
            group.alpha = items != null && items.IsPlaying && items.IsReady(item) ? 1f : .4f;
        }

        private void ReturnHome()
        {
            if (!dragging)
                return;
            dragging = false;
            if (home != null)
            {
                rect.SetParent(home, false);
                rect.SetSiblingIndex(homeIndex);
                rect.anchorMin = homeMin;
                rect.anchorMax = homeMax;
                rect.pivot = homePivot;
                rect.sizeDelta = homeSize;
                rect.anchoredPosition = homePosition;
                rect.localScale = homeScale;
                rect.localRotation = homeRotation;
            }
            group.blocksRaycasts = true;
        }
    }
}
