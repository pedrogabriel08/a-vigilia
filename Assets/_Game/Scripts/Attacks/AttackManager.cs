using UnityEngine;

namespace Vigilia
{
    [DisallowMultipleComponent]
    public sealed class AttackManager : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private AttackEvent[] attacks;
        [SerializeField, Min(.1f)] private float minAttackInterval = 8f;
        [SerializeField, Min(.1f)] private float maxAttackInterval = 14f;

        public AttackEvent CurrentAttack { get; private set; }
        public float IntervalRemaining { get; private set; }
        public float PreAttackRemaining { get; private set; }
        public bool IsRunning => isActiveAndEnabled && game != null && game.IsPlaying;

        private void OnEnable()
        {
            if (game == null)
                return;
            game.StateChanged += OnGameStateChanged;
            OnGameStateChanged(game.State);
        }

        private void Start()
        {
            if (game != null && attacks != null && attacks.Length > 0)
                return;
            Debug.LogError("AttackManager: conecte o GameManager e pelo menos um ataque.", this);
            enabled = false;
        }

        private void OnDisable()
        {
            if (game != null)
                game.StateChanged -= OnGameStateChanged;
            StopEvents();
        }

        private void Update() => AdvanceScheduler(Time.deltaTime);

        private void AdvanceScheduler(float deltaTime)
        {
            if (!IsRunning || deltaTime <= 0f)
                return;

            if (CurrentAttack != null)
            {
                if (CurrentAttack.Phase == AttackPhase.PreAttack)
                {
                    PreAttackRemaining = Mathf.Max(0f, PreAttackRemaining - deltaTime);
                    if (PreAttackRemaining <= 0f)
                        CurrentAttack.BeginAttack();
                }
                return;
            }

            IntervalRemaining = Mathf.Max(0f, IntervalRemaining - deltaTime);
            if (IntervalRemaining > 0f)
                return;

            var selected = ChooseAttack();
            if (selected == null)
            {
                ScheduleNext();
                return;
            }

            CurrentAttack = selected;
            PreAttackRemaining = selected.PreAttackDuration;
            selected.Finished += OnAttackFinished;
            selected.BeginPreAttack(game);
        }

        private AttackEvent ChooseAttack()
        {
            if (attacks == null)
                return null;
            int available = 0;
            foreach (var attack in attacks)
                if (attack != null && attack.CanStart)
                    available++;
            if (available == 0)
                return null;

            int choice = Random.Range(0, available);
            foreach (var attack in attacks)
                if (attack != null && attack.CanStart && choice-- == 0)
                    return attack;
            return null;
        }

        private void OnGameStateChanged(GameState state)
        {
            StopEvents();
            if (state == GameState.Playing)
                ScheduleNext();
        }

        private void OnAttackFinished(AttackEvent attack)
        {
            if (attack != CurrentAttack)
                return;
            attack.Finished -= OnAttackFinished;
            CurrentAttack = null;
            PreAttackRemaining = 0f;
            if (IsRunning)
                ScheduleNext();
        }

        private void ScheduleNext()
        {
            float minimum = Mathf.Max(.1f, minAttackInterval);
            float maximum = Mathf.Max(minimum, maxAttackInterval);
            IntervalRemaining = Random.Range(minimum, maximum);
        }

        private void StopEvents()
        {
            var active = CurrentAttack;
            CurrentAttack = null;
            IntervalRemaining = 0f;
            PreAttackRemaining = 0f;
            if (active == null)
                return;
            active.Finished -= OnAttackFinished;
            active.Cancel();
        }

        private void OnValidate()
        {
            minAttackInterval = Mathf.Max(.1f, minAttackInterval);
            maxAttackInterval = Mathf.Max(minAttackInterval, maxAttackInterval);
        }
    }
}
