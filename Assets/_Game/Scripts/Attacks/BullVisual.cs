using UnityEngine;

namespace Vigilia
{
    // As artes podem ser trocadas ou reposicionadas dentro desses dois grupos.
    [DisallowMultipleComponent]
    public sealed class BullVisual : MonoBehaviour
    {
        [SerializeField] private GameObject normalAppearance;
        [SerializeField] private GameObject strangeAppearance;
        public bool IsStrange { get; private set; }

        public void SetStrange(bool value)
        {
            IsStrange = value;
            if (normalAppearance != null)
                normalAppearance.SetActive(!value);
            if (strangeAppearance != null)
                strangeAppearance.SetActive(value);
        }

        private void OnDisable() => SetStrange(false);
    }
}
