using UnityEngine;
using UnityEngine.UI;

namespace Vigilia
{
    // Ferramenta temporária da etapa 1. Não representa itens nem um QTE real.
    public sealed class PrototypeDebugControls : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private Text drainButtonText;
        [SerializeField, Min(0f)] private float damagePerSecond = 5f;
        private bool draining;

        private void Awake()
        {
            debugPanel.SetActive(Debug.isDebugBuild);
        }

        private void OnEnable()
        {
            if (game != null)
                game.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (game != null)
                game.StateChanged -= OnStateChanged;
            draining = false;
        }

        private void Update()
        {
            if (draining && game.IsPlaying)
                game.Baby.TakeDamage(damagePerSecond * Time.deltaTime);
        }

        public void Damage25()
        {
            if (game.IsPlaying)
                game.Baby.TakeDamage(25f);
        }

        public void Protect15Seconds()
        {
            if (game.IsPlaying)
                game.Baby.Protect(15f);
        }

        public void ToggleDrain()
        {
            if (!game.IsPlaying)
                return;
            draining = !draining;
            RefreshLabel();
        }

        private void OnStateChanged(GameState state)
        {
            draining = false;
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (drainButtonText != null)
                drainButtonText.text = draining ? "PARAR DANO" : "DANO CONTÍNUO 5/s";
        }
    }
}
