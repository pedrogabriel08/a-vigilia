using System;
using UnityEngine;

namespace Vigilia
{
    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float gameDuration = 150f;
        [SerializeField] private BabyController baby;

        public GameState State { get; private set; } = GameState.Waiting;
        public float RemainingTime { get; private set; }
        public float GameDuration => gameDuration;
        public BabyController Baby => baby;
        public bool IsPlaying => State == GameState.Playing;

        public event Action<GameState> StateChanged;
        public event Action<float> TimeChanged;

        private void OnEnable()
        {
            if (baby != null)
                baby.Died += OnBabyDied;
        }

        private void Start()
        {
            if (baby == null)
            {
                Debug.LogError("GameManager: conecte o BabyController no Inspector.", this);
                enabled = false;
                return;
            }

            RemainingTime = gameDuration;
            baby.SetDamageEnabled(false);
            TimeChanged?.Invoke(RemainingTime);
            StateChanged?.Invoke(State);
        }

        private void OnDisable()
        {
            if (baby != null)
            {
                baby.Died -= OnBabyDied;
                baby.SetDamageEnabled(false);
            }
        }

        private void Update() => AdvanceClock(Time.deltaTime);

        private void AdvanceClock(float deltaTime)
        {
            if (!IsPlaying || deltaTime <= 0f)
                return;

            RemainingTime = Mathf.Max(0f, RemainingTime - deltaTime);
            if (RemainingTime <= 0f)
                Finish(baby.IsAlive ? GameState.Victory : GameState.Defeat);

            TimeChanged?.Invoke(RemainingTime);
        }

        // Também reinicia a partida sem recarregar a cena.
        public void StartGame()
        {
            if (baby == null || IsPlaying)
                return;

            baby.SetDamageEnabled(false);
            baby.ResetState();
            RemainingTime = gameDuration;
            State = GameState.Playing;
            baby.SetDamageEnabled(true);
            TimeChanged?.Invoke(RemainingTime);
            StateChanged?.Invoke(State);
        }

        private void OnBabyDied() => Finish(GameState.Defeat);

        private void Finish(GameState result)
        {
            if (!IsPlaying)
                return;

            State = result;
            baby.SetDamageEnabled(false);
            baby.ClearProtection();
            StateChanged?.Invoke(State);
        }
    }
}
