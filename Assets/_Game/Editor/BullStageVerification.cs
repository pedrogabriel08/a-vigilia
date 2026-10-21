using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vigilia.Editor
{
    public static class BullStageVerification
    {
        private const string Report = "Library/VigiliaBullStageVerification.txt";
        private static readonly List<string> results = new List<string>();

        [MenuItem("Game Jam/Verificar eventos do boi")]
        public static void Verify()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            results.Clear();
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var randomState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(1729);
            GameManager game = null;
            AttackManager manager = null;
            try
            {
                var baby = new GameObject("Baby").AddComponent<BabyController>();
                Call(baby, "Awake");
                game = new GameObject("Game").AddComponent<GameManager>();
                Set(game, "baby", baby);
                Call(game, "OnEnable");
                Call(game, "Start");

                var bulls = new BullVisual[4];
                for (int i = 0; i < bulls.Length; i++)
                {
                    bulls[i] = new GameObject("Bull_" + i).AddComponent<BullVisual>();
                    var normal = new GameObject("Normal");
                    normal.transform.SetParent(bulls[i].transform);
                    var strange = new GameObject("Strange");
                    strange.transform.SetParent(bulls[i].transform);
                    Set(bulls[i], "normalAppearance", normal);
                    Set(bulls[i], "strangeAppearance", strange);
                    bulls[i].SetStrange(false);
                }

                var attack = new GameObject("BullAttack").AddComponent<BullAttack>();
                SetArray(attack, "bulls", bulls);
                manager = new GameObject("AttackManager").AddComponent<AttackManager>();
                Set(manager, "game", game);
                SetArray(manager, "attacks", new AttackEvent[] { attack });
                Call(manager, "OnEnable");
                Call(manager, "AdvanceScheduler", 100f);
                Check(manager.CurrentAttack == null && manager.IntervalRemaining == 0f, "Nenhum evento antes de iniciar");
                game.StartGame();
                Check(InRange(manager.IntervalRemaining, 8f, 14f), "Primeiro intervalo entre 8 e 14 segundos");
                float interval = manager.IntervalRemaining;
                Call(manager, "AdvanceScheduler", interval - .25f);
                Check(manager.CurrentAttack == null, "Intervalo mínimo respeitado");
                Call(manager, "AdvanceScheduler", .25f);
                Check(manager.CurrentAttack == attack && attack.Phase == AttackPhase.PreAttack, "Intervalo inicia pré-ataque");
                Check(StrangeCount(bulls) == 1, "Exatamente um boi estranho");
                Check(manager.PreAttackRemaining == 5f, "Janela de percepção de 5 segundos");
                var selected = attack.SelectedBull;
                Call(manager, "AdvanceScheduler", 2f);
                Check(attack.SelectedBull == selected && manager.PreAttackRemaining == 3f && StrangeCount(bulls) == 1,
                    "Sem troca de alvo ou sobreposição durante o evento");
                Call(manager, "AdvanceScheduler", 3f);
                Check(manager.CurrentAttack == null && attack.Phase == AttackPhase.Idle && StrangeCount(bulls) == 0,
                    "Fim da prévia restaura os bois");
                Check(InRange(manager.IntervalRemaining, 8f, 14f), "Novo intervalo só após terminar");
                Check(baby.Health == 100f && game.RemainingTime == 150f, "Prévia não causa dano nem controla timer global");

                var seen = new HashSet<BullVisual>();
                for (int i = 0; i < 12; i++)
                {
                    Call(manager, "AdvanceScheduler", manager.IntervalRemaining);
                    seen.Add(attack.SelectedBull);
                    attack.Cancel();
                }
                Check(seen.Count > 1 && !seen.Contains(null), "Sorteio pode selecionar bois diferentes");
                Call(manager, "AdvanceScheduler", manager.IntervalRemaining);
                Call(game, "AdvanceClock", 150f);
                Check(game.State == GameState.Victory && manager.CurrentAttack == null && StrangeCount(bulls) == 0,
                    "Vitória cancela e limpa o pré-ataque");
                Call(manager, "AdvanceScheduler", 100f);
                Check(manager.CurrentAttack == null && manager.IntervalRemaining == 0f, "Sem eventos após vitória");

                game.StartGame();
                Check(InRange(manager.IntervalRemaining, 8f, 14f) && manager.CurrentAttack == null, "Reinício recebe novo intervalo completo");
                Call(manager, "AdvanceScheduler", manager.IntervalRemaining);
                baby.TakeDamage(1000f);
                Check(game.State == GameState.Defeat && StrangeCount(bulls) == 0 && manager.CurrentAttack == null,
                    "Derrota cancela e limpa o pré-ataque");

                game.StartGame();
                Call(manager, "AdvanceScheduler", manager.IntervalRemaining);
                Call(manager, "OnDisable");
                Check(manager.CurrentAttack == null && StrangeCount(bulls) == 0, "Desativar agendador limpa o evento");
                Call(manager, "OnEnable");
                Check(InRange(manager.IntervalRemaining, 8f, 14f), "Reativar durante a partida agenda intervalo");

                SetArray(manager, "attacks", new AttackEvent[] { null });
                Call(manager, "AdvanceScheduler", manager.IntervalRemaining);
                Check(manager.CurrentAttack == null && InRange(manager.IntervalRemaining, 8f, 14f),
                    "Referência vazia não trava o agendador");
                File.WriteAllText(Report, "PASS\n" + results.Count + " verificações aprovadas.\n" + string.Join("\n", results));
                Debug.Log("Eventos do boi: " + results.Count + " verificações aprovadas.");
            }
            catch (Exception exception)
            {
                File.WriteAllText(Report, "FAIL\n" + exception + "\n" + string.Join("\n", results));
                throw;
            }
            finally
            {
                if (manager != null)
                    Call(manager, "OnDisable");
                if (game != null)
                    Call(game, "OnDisable");
                UnityEngine.Random.state = randomState;
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static bool InRange(float value, float min, float max) => value >= min && value <= max;
        private static int StrangeCount(BullVisual[] bulls)
        {
            int count = 0;
            foreach (var bull in bulls)
                if (bull.IsStrange)
                    count++;
            return count;
        }
        private static void Check(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
            results.Add("OK: " + message);
        }
        private static void Call(object target, string method, params object[] args) =>
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        private static void Set(UnityEngine.Object target, string name, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(name).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetArray(UnityEngine.Object target, string name, UnityEngine.Object[] values)
        {
            var serialized = new SerializedObject(target);
            var array = serialized.FindProperty(name);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
