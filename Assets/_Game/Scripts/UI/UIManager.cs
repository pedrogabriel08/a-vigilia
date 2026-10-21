using UnityEngine;
using UnityEngine.UI;

namespace Vigilia
{
    [DisallowMultipleComponent]
    public sealed class UIManager : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private Text timerText;
        [SerializeField] private Text healthText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Text protectionText;
        [SerializeField] private GameObject protectionAura;
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultTitle;
        [SerializeField] private Text resultDescription;

        private void OnEnable()
        {
            if (game == null || game.Baby == null)
                return;

            game.StateChanged += RefreshState;
            game.TimeChanged += RefreshTime;
            game.Baby.HealthChanged += RefreshHealth;
            game.Baby.ProtectionChanged += RefreshProtection;
        }

        private void Start()
        {
            if (game == null || game.Baby == null)
                return;

            RefreshState(game.State);
            RefreshTime(game.IsPlaying ? game.RemainingTime : game.GameDuration);
            RefreshHealth(game.Baby.Health, game.Baby.MaxHealth);
            RefreshProtection(game.Baby.IsProtected);
        }

        private void OnDisable()
        {
            if (game == null || game.Baby == null)
                return;

            game.StateChanged -= RefreshState;
            game.TimeChanged -= RefreshTime;
            game.Baby.HealthChanged -= RefreshHealth;
            game.Baby.ProtectionChanged -= RefreshProtection;
        }

        private void Update()
        {
            if (game != null && game.Baby != null && game.Baby.IsProtected)
                protectionText.text = $"PROTEGIDO  {Mathf.CeilToInt(game.Baby.ProtectionRemaining)}s";
        }

        private void RefreshTime(float remaining)
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, remaining));
            timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
        }

        private void RefreshHealth(float current, float maximum)
        {
            healthText.text = $"VIDA DO BEBÊ  {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(maximum)}";
            healthFill.fillAmount = maximum > 0f ? current / maximum : 0f;
            healthFill.rectTransform.anchorMax = new Vector2(healthFill.fillAmount, 1f);
            healthFill.color = Color.Lerp(new Color(0.85f, 0.25f, 0.22f),
                new Color(0.48f, 0.75f, 0.55f), healthFill.fillAmount);
        }

        private void RefreshProtection(bool active)
        {
            protectionAura.SetActive(active);
            protectionText.text = active ? "PROTEGIDO" : "SEM PROTEÇÃO";
        }

        private void RefreshState(GameState state)
        {
            startPanel.SetActive(state == GameState.Waiting);
            bool finished = state == GameState.Victory || state == GameState.Defeat;
            resultPanel.SetActive(finished);
            if (!finished)
                return;

            bool victory = state == GameState.Victory;
            resultTitle.text = victory ? "AMANHECEU" : "A VIGÍLIA TERMINOU";
            resultDescription.text = victory
                ? "Você protegeu seu filho até o fim da noite."
                : "A vida do bebê chegou a zero.";
        }
    }
}
