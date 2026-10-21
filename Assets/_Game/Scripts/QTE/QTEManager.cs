using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Vigilia
{
    public enum QTEResult { Success, Failure, Cancelled }
    public enum QTEType { Mash, Hold }

    public sealed class QTEManager : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private Key mashKey = Key.Space;
        private Action<QTEResult> onResult;
        private Action<float> onActiveTime;
        private bool holding;
        public bool IsActive { get; private set; }
        public QTEType Type { get; private set; }
        public float TimeRemaining { get; private set; }
        public float Progress { get; private set; }
        public float Required { get; private set; }
        public event Action Started;
        public event Action Ended;

        private void OnEnable()
        {
            if (game != null) game.StateChanged += OnGameStateChanged;
        }
        private void OnDisable()
        {
            if (game != null) game.StateChanged -= OnGameStateChanged;
            Cancel();
        }
        private void OnGameStateChanged(GameState state) => Cancel();

        public bool StartMashQTE(int presses, float duration, Action<QTEResult> result, Action<float> activeTime = null) =>
            Begin(QTEType.Mash, Mathf.Max(1, presses), duration, result, activeTime);
        public bool StartHoldQTE(float holdSeconds, float duration, Action<QTEResult> result, Action<float> activeTime = null) =>
            Begin(QTEType.Hold, Mathf.Max(.1f, holdSeconds), duration, result, activeTime);

        private bool Begin(QTEType type, float required, float duration, Action<QTEResult> result, Action<float> activeTime)
        {
            if (IsActive || game == null || !game.IsPlaying || duration <= 0f) return false;
            Type = type;
            Required = required;
            TimeRemaining = duration;
            Progress = 0f;
            holding = false;
            onResult = result;
            onActiveTime = activeTime;
            IsActive = true;
            Started?.Invoke();
            return true;
        }
        private void Update()
        {
            if (!IsActive) return;
            if (Type == QTEType.Mash && mashKey != Key.None && Keyboard.current != null &&
                Keyboard.current[mashKey].wasPressedThisFrame) RegisterPress();
            Advance(Time.deltaTime);
        }
        private void Advance(float deltaTime)
        {
            if (!IsActive || game == null || !game.IsPlaying || deltaTime <= 0f) return;
            float step = Mathf.Min(deltaTime, TimeRemaining);
            if (Type == QTEType.Hold && holding) step = Mathf.Min(step, Required - Progress);
            TimeRemaining = Mathf.Max(0f, TimeRemaining - step);
            if (Type == QTEType.Hold && holding) Progress = Mathf.Min(Required, Progress + step);
            onActiveTime?.Invoke(step);
            if (!IsActive) return; // O dano pode ter causado derrota e cancelado o desafio.
            if (Progress >= Required) Resolve(QTEResult.Success);
            else if (TimeRemaining <= 0f) Resolve(QTEResult.Failure);
        }
        public void RegisterPress()
        {
            if (!IsActive || Type != QTEType.Mash || game == null || !game.IsPlaying) return;
            Progress = Mathf.Min(Required, Progress + 1f);
            if (Progress >= Required) Resolve(QTEResult.Success);
        }
        public void SetHolding(bool value) => holding = value && IsActive && Type == QTEType.Hold;
        public void Cancel()
        {
            if (IsActive) Resolve(QTEResult.Cancelled);
        }
        private void Resolve(QTEResult result)
        {
            if (!IsActive) return;
            IsActive = false;
            holding = false;
            var callback = onResult;
            onResult = null;
            onActiveTime = null;
            Ended?.Invoke();
            callback?.Invoke(result);
        }
    }
}
