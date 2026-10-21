using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vigilia.Editor
{
    public static class ScissorsStageVerification
    {
        private const string Report = "Library/VigiliaScissorsStageVerification.txt";
        private static readonly List<string> results = new List<string>();

        [MenuItem("Game Jam/Verificar tesoura e itens")]
        public static void Verify()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            results.Clear();
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var randomState = UnityEngine.Random.state;
            GameManager game = null;
            ItemManager items = null;
            AttackManager attacks = null;
            try
            {
                var baby = new GameObject("Baby").AddComponent<BabyController>();
                Call(baby, "Awake");
                game = new GameObject("Game").AddComponent<GameManager>();
                Set(game, "baby", baby);
                Call(game, "OnEnable");
                Call(game, "Start");
                items = new GameObject("Items").AddComponent<ItemManager>();
                Set(items, "game", game);
                Call(items, "OnEnable");
                var target = new GameObject("WindowTarget").AddComponent<ItemDropTarget>();
                int uses = 0;
                target.OnUsed.AddListener(() => uses++);

                Check(!items.TryPickUp(ItemId.Scissors) && !items.HeldItem.HasValue, "Sem coleta antes de iniciar");
                game.StartGame();
                Check(items.IsReady(ItemId.Scissors) && items.GetCooldownDuration(ItemId.Scissors) == 5f,
                    "Tesoura inicia pronta com recarga de 5s");
                Check(!items.TryPickUp((ItemId)99), "Item não configurado é recusado");
                Check(items.TryPickUp(ItemId.Scissors) && items.HeldItem == ItemId.Scissors, "Coleta registra o item carregado");
                Check(!items.TryPickUp(ItemId.Scissors), "Apenas um item pode ser carregado");
                Check(!items.TryUseOn(null) && !items.HeldItem.HasValue && items.IsReady(ItemId.Scissors) && uses == 0,
                    "Soltar fora do alvo devolve sem consumo");
                items.TryPickUp(ItemId.Scissors);
                target.enabled = false;
                Check(!items.TryUseOn(target) && items.IsReady(ItemId.Scissors) && !items.HeldItem.HasValue,
                    "Alvo desativado não consome o item");
                target.enabled = true;
                items.TryPickUp(ItemId.Scissors);
                Check(items.TryUseOn(target) && uses == 1 && !items.HeldItem.HasValue &&
                    items.GetCooldownRemaining(ItemId.Scissors) == 5f, "Uso na janela inicia recarga e devolve o item");
                Check(!items.TryPickUp(ItemId.Scissors), "Coleta bloqueada durante recarga");
                Check(!items.TryUseOn(target) && uses == 1, "Uma soltura não pode usar o item duas vezes");
                Call(items, "AdvanceCooldowns", 4.9f);
                Check(!items.IsReady(ItemId.Scissors) && !items.TryPickUp(ItemId.Scissors), "Recarga não acaba antes de 5s");
                Call(items, "AdvanceCooldowns", .1f);
                Check(items.IsReady(ItemId.Scissors) && items.TryPickUp(ItemId.Scissors), "Item disponível ao completar 5s");
                items.ReturnHeldItem();
                Check(items.IsReady(ItemId.Scissors) && !items.HeldItem.HasValue, "Devolução manual não inicia recarga");

                var visual = new GameObject("BullVisual").AddComponent<BullVisual>();
                var normal = new GameObject("Normal");
                normal.transform.SetParent(visual.transform);
                var strange = new GameObject("Strange");
                strange.transform.SetParent(visual.transform);
                Set(visual, "normalAppearance", normal);
                Set(visual, "strangeAppearance", strange);
                visual.SetStrange(false);
                var bull = new GameObject("BullAttack").AddComponent<BullAttack>();
                SetArray(bull, "bulls", new UnityEngine.Object[] { visual });
                attacks = new GameObject("Attacks").AddComponent<AttackManager>();
                Set(attacks, "game", game);
                SetArray(attacks, "attacks", new UnityEngine.Object[] { bull });
                Call(attacks, "OnEnable");
                target.OnUsed.AddListener(bull.RepelDuringPreAttack);
                Call(attacks, "AdvanceScheduler", attacks.IntervalRemaining);
                Check(bull.Phase == AttackPhase.PreAttack && visual.IsStrange, "Pré-ataque iniciado antes da defesa");
                items.TryPickUp(ItemId.Scissors);
                items.TryUseOn(target);
                Check(bull.Phase == AttackPhase.Idle && !visual.IsStrange && attacks.CurrentAttack == null,
                    "Tesoura cancela totalmente o pré-ataque e restaura o boi");
                Check(attacks.IntervalRemaining >= 8f && attacks.IntervalRemaining <= 14f &&
                    items.GetCooldownRemaining(ItemId.Scissors) == 5f, "Defesa inicia intervalo e recarga independentes");

                Call(items, "AdvanceCooldowns", 5f);
                items.TryPickUp(ItemId.Scissors);
                items.TryUseOn(target);
                Check(uses == 3 && items.GetCooldownRemaining(ItemId.Scissors) == 5f,
                    "Usar na janela sem ameaça também consome recarga");
                Call(game, "AdvanceClock", 150f);
                Check(game.State == GameState.Victory && !items.TryPickUp(ItemId.Scissors) &&
                    !items.HeldItem.HasValue, "Vitória bloqueia uso e limpa o item");
                game.StartGame();
                Check(items.IsReady(ItemId.Scissors) && items.GetCooldownRemaining(ItemId.Scissors) == 0f,
                    "Reinício restaura disponibilidade");
                items.TryPickUp(ItemId.Scissors);
                baby.TakeDamage(1000f);
                Check(game.State == GameState.Defeat && !items.HeldItem.HasValue && !items.TryPickUp(ItemId.Scissors),
                    "Derrota devolve item carregado e bloqueia coleta");
                game.StartGame();
                items.TryPickUp(ItemId.Scissors);
                items.enabled = false;
                Call(items, "OnDisable");
                Check(!items.HeldItem.HasValue && !items.TryPickUp(ItemId.Scissors), "Desativação devolve o item e bloqueia uso");
                items.enabled = true;
                Call(items, "OnEnable");
                Check(items.IsReady(ItemId.Scissors) && items.TryPickUp(ItemId.Scissors), "Reativação mantém sistema utilizável");
                VerifyDragCallbacks(items, game, target);
                File.WriteAllText(Report, "PASS\n" + results.Count + " verificações aprovadas.\n" + string.Join("\n", results));
                Debug.Log("Tesoura e itens: " + results.Count + " verificações aprovadas.");
            }
            catch (Exception exception)
            {
                File.WriteAllText(Report, "FAIL\n" + exception + "\n" + string.Join("\n", results));
                throw;
            }
            finally
            {
                if (items != null) Call(items, "OnDisable");
                if (attacks != null) Call(attacks, "OnDisable");
                if (game != null) Call(game, "OnDisable");
                UnityEngine.Random.state = randomState;
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void VerifyDragCallbacks(ItemManager items, GameManager game, ItemDropTarget target)
        {
            items.ReturnHeldItem();
            var canvas = new GameObject("DragCanvas", typeof(RectTransform), typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var home = new GameObject("Home", typeof(RectTransform)).GetComponent<RectTransform>();
            home.SetParent(canvas.transform, false);
            home.sizeDelta = new Vector2(200f, 200f);
            var layer = new GameObject("DragLayer", typeof(RectTransform)).GetComponent<RectTransform>();
            layer.SetParent(canvas.transform, false);
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = layer.offsetMax = Vector2.zero;
            var obj = new GameObject("DragItem", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            obj.transform.SetParent(home, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(15f, 20f);
            Vector2 originalPosition = rect.anchoredPosition;
            var drag = obj.AddComponent<DraggableItem>();
            Set(drag, "items", items);
            Set(drag, "dragLayer", layer);
            Call(drag, "Awake");
            Call(drag, "OnEnable");
            var eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            var pointer = new PointerEventData(eventSystem)
            {
                position = new Vector2(100f, 100f),
                button = PointerEventData.InputButton.Left,
                pointerDrag = obj
            };
            try
            {
                Check(ExecuteEvents.GetEventHandler<IDragHandler>(obj) == obj, "Item registra os handlers de arrasto da Unity");
                drag.OnBeginDrag(pointer);
                Check(rect.parent == layer && items.HeldItem == ItemId.Scissors && !obj.GetComponent<CanvasGroup>().blocksRaycasts,
                    "Início do arrasto move o item e libera raycast do alvo");
                pointer.position = new Vector2(300f, 200f);
                drag.OnDrag(pointer);
                drag.OnEndDrag(pointer);
                Check(rect.parent == home && rect.anchoredPosition == originalPosition && !items.HeldItem.HasValue && items.IsReady(ItemId.Scissors),
                    "Fim de arrasto fora do alvo restaura posição sem recarga");
                drag.OnBeginDrag(pointer);
                target.OnDrop(pointer);
                drag.OnEndDrag(pointer);
                Check(rect.parent == home && !items.HeldItem.HasValue && items.GetCooldownRemaining(ItemId.Scissors) == 5f,
                    "OnDrop da Unity encaminha uso e restaura o item");
                Call(items, "AdvanceCooldowns", 5f);
                drag.OnBeginDrag(pointer);
                Call(game, "AdvanceClock", 150f);
                Check(rect.parent == home && !items.HeldItem.HasValue && obj.GetComponent<CanvasGroup>().blocksRaycasts,
                    "Fim de partida durante arrasto devolve o objeto e restaura raycast");
            }
            finally { Call(drag, "OnDisable"); }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
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
            for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
