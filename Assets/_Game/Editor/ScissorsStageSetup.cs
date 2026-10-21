using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vigilia.Editor
{
    [InitializeOnLoad]
    public static class ScissorsStageSetup
    {
        private const string Marker = "Library/VigiliaScissorsStageSetup.txt";
        static ScissorsStageSetup() => EditorApplication.delayCall += PrepareOnce;

        private static void PrepareOnce()
        {
            if (File.Exists(Marker))
                return;
            if (EditorApplication.isUpdating || EditorApplication.isCompiling)
            {
                EditorApplication.delayCall += PrepareOnce;
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(MvpSceneSetup.ScenePath))
                return;
            var scene = SceneManager.GetSceneByPath(MvpSceneSetup.ScenePath);
            if (scene.IsValid() && scene.isLoaded && scene.isDirty)
            {
                Debug.Log("Tesoura: salve a cena e use Game Jam > Preparar tesoura e armário.");
                return;
            }
            try
            {
                Install();
                ScissorsStageVerification.Verify();
            }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        [MenuItem("Game Jam/Preparar tesoura e armário")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            var previous = SceneManager.GetActiveScene();
            var scene = SceneManager.GetSceneByPath(MvpSceneSetup.ScenePath);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (!openedHere && scene.isDirty)
                throw new InvalidOperationException("Salve a cena antes de preparar a tesoura.");
            if (openedHere)
                scene = EditorSceneManager.OpenScene(MvpSceneSetup.ScenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var systems = Root(scene, "Systems");
                var canvas = Root(scene, "Canvas");
                var game = systems.GetComponentInChildren<GameManager>(true);
                var bull = systems.GetComponentInChildren<BullAttack>(true);
                if (game == null || bull == null)
                    throw new InvalidOperationException("Prepare a base e os eventos do boi antes da tesoura.");
                var wardrobe = canvas.Find("Room_Placeholders/Wardrobe");
                var window = canvas.Find("Room_Placeholders/Window");
                if (wardrobe == null || window == null)
                    throw new InvalidOperationException("Armário/janela não encontrados na cena MVP.");

                var items = systems.GetComponentInChildren<ItemManager>(true);
                if (items == null)
                {
                    var obj = new GameObject("ItemManager", typeof(ItemManager));
                    obj.transform.SetParent(systems, false);
                    items = obj.GetComponent<ItemManager>();
                }
                Set(items, "game", game);

                var dragLayer = canvas.Find("DragLayer") as RectTransform;
                if (dragLayer == null)
                {
                    dragLayer = Group("DragLayer", canvas, Vector2.zero, Vector2.one);
                    BelowModals(dragLayer, canvas);
                }
                var contents = wardrobe.Find("Contents") as RectTransform;
                if (contents == null)
                {
                    contents = Panel("Contents", wardrobe, new Vector2(.04f, .04f), new Vector2(.96f, .82f), C("262E32"));
                    var item = Panel("Scissors", contents, new Vector2(.2f, .31f), new Vector2(.8f, .95f), C("4B554F"));
                    item.GetComponent<Image>().raycastTarget = true;
                    item.gameObject.AddComponent<CanvasGroup>();
                    var art = Group("Art", item, new Vector2(.1f, .05f), new Vector2(.9f, .95f));
                    Panel("BladeVertical", art, new Vector2(.47f, .25f), new Vector2(.54f, .94f), C("D8DCD6"));
                    Panel("BladeHorizontal", art, new Vector2(.12f, .54f), new Vector2(.89f, .63f), C("C7CEC7"));
                    Panel("HandleLeft", art, new Vector2(.25f, .07f), new Vector2(.46f, .32f), C("C19B67"));
                    Panel("HandleLeftHole", art, new Vector2(.3f, .12f), new Vector2(.41f, .27f), C("4B554F"));
                    Panel("HandleRight", art, new Vector2(.57f, .07f), new Vector2(.78f, .32f), C("C19B67"));
                    Panel("HandleRightHole", art, new Vector2(.62f, .12f), new Vector2(.73f, .27f), C("4B554F"));
                    var draggable = item.gameObject.AddComponent<DraggableItem>();
                    Set(draggable, "items", items);
                    Set(draggable, "dragLayer", dragLayer);
                    CooldownView(contents, items);
                }

                var header = wardrobe.Find("WardrobeLabel")?.GetComponent<Text>();
                var wardrobeController = wardrobe.GetComponent<WardrobeController>();
                if (wardrobeController == null)
                {
                    wardrobeController = wardrobe.gameObject.AddComponent<WardrobeController>();
                    if (header != null)
                    {
                        Stretch(header.rectTransform, new Vector2(.02f, .84f), new Vector2(.98f, .99f));
                        header.fontSize = 20;
                    }
                }
                Set(wardrobeController, "game", game);
                Set(wardrobeController, "items", items);
                Set(wardrobeController, "contents", contents.gameObject);
                Set(wardrobeController, "label", header);
                wardrobe.GetComponent<Image>().raycastTarget = true;
                if (header != null)
                    header.text = "ARMÁRIO · ABRIR";
                contents.gameObject.SetActive(false);

                var targetTransform = window.Find("ScissorsDropArea");
                if (targetTransform == null)
                {
                    targetTransform = Panel("ScissorsDropArea", window, Vector2.zero, Vector2.one, Color.clear);
                    targetTransform.GetComponent<Image>().raycastTarget = true;
                    var target = targetTransform.gameObject.AddComponent<ItemDropTarget>();
                    UnityEventTools.AddPersistentListener(target.OnUsed, bull.RepelDuringPreAttack);
                }

                if (canvas.Find("ScissorsCooldownHUD") == null)
                {
                    var hud = Group("ScissorsCooldownHUD", canvas, new Vector2(.08f, .32f), new Vector2(.32f, .41f));
                    CooldownView(hud, items);
                    Stretch((RectTransform)hud.Find("ScissorsStatus"), new Vector2(.02f, .35f), new Vector2(.98f, .95f));
                    Stretch((RectTransform)hud.Find("CooldownTrack"), new Vector2(.1f, .15f), new Vector2(.9f, .25f));
                    BelowModals(hud, canvas);
                }
                var description = canvas.Find("StartPanel/StartDescription")?.GetComponent<Text>();
                if (description != null)
                    description.text = "Observe os bois pela janela.\nAbra o armário e arraste a tesoura até a janela.\n\nRepila a manifestação antes que ela avance.\nO QTE será adicionado na próxima etapa.";

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Falha ao salvar a tesoura e o armário.");
                File.WriteAllText(Marker, "Instalado: armário, arrasto, tesoura e cooldown de 5s.\n" + DateTime.Now.ToString("O"));
                Debug.Log("Tesoura pronta. Abra o armário e arraste até a janela durante o pré-ataque.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void CooldownView(Transform parent, ItemManager items)
        {
            var label = Text("ScissorsStatus", parent, "TESOURA · PRONTA", new Vector2(.02f, .03f), new Vector2(.98f, .23f));
            var track = Panel("CooldownTrack", parent, new Vector2(.1f, .25f), new Vector2(.9f, .29f), C("35434C"));
            var fill = Panel("CooldownFill", track, Vector2.zero, Vector2.one, C("7ABF8C")).GetComponent<Image>();
            var view = parent.gameObject.AddComponent<ItemCooldownView>();
            Set(view, "items", items);
            Set(view, "label", label);
            Set(view, "progress", fill);
        }

        private static void BelowModals(Transform obj, Transform canvas)
        {
            var start = canvas.Find("StartPanel");
            if (start != null)
                obj.SetSiblingIndex(start.GetSiblingIndex());
        }
        private static RectTransform Group(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            Stretch(rect, min, max);
            return rect;
        }
        private static RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var rect = Group(name, parent, min, max);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }
        private static Text Text(string name, Transform parent, string value, Vector2 min, Vector2 max)
        {
            var rect = Group(name, parent, min, max);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 21;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = C("EEE8D8");
            text.text = value;
            text.raycastTarget = false;
            return text;
        }
        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static Color C(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
        private static Transform Root(Scene scene, string name)
        {
            foreach (var obj in scene.GetRootGameObjects())
                if (obj.name == name)
                    return obj.transform;
            throw new InvalidOperationException("Objeto não encontrado: " + name);
        }
        private static void Set(UnityEngine.Object target, string name, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(name).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
