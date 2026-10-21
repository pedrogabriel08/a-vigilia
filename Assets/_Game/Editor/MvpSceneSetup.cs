using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vigilia.Editor
{
    [InitializeOnLoad]
    public static class MvpSceneSetup
    {
        public const string ScenePath = "Assets/_Game/Scenes/Main_MVP.unity";
        private const string ReportPath = "Library/VigiliaMvpVerification.txt";
        private static Font font;

        static MvpSceneSetup() => EditorApplication.delayCall += PrepareOnce;

        private static void PrepareOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += PrepareOnce;
                return;
            }
            if (File.Exists(ScenePath))
                return;

            try
            {
                CreateScene();
                Verify();
            }
            catch (Exception exception)
            {
                File.WriteAllText(ReportPath, "FAIL\n" + exception);
                Debug.LogException(exception);
            }
        }

        [MenuItem("Game Jam/Abrir Base MVP")]
        public static void OpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (!File.Exists(ScenePath))
                CreateScene();
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        private static void CreateScene()
        {
            Directory.CreateDirectory("Assets/_Game/Scenes");
            AssetDatabase.Refresh();
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0f, 0f, -10f);
                var camera = cameraObject.GetComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = C("172029");

                var systems = new GameObject("Systems");
                var managerObject = new GameObject("GameManager");
                managerObject.transform.SetParent(systems.transform);
                var game = managerObject.AddComponent<GameManager>();

                var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
                    typeof(CanvasScaler), typeof(GraphicRaycaster));
                var canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                var root = canvasObject.transform;

                var room = Panel("Room_Placeholders", root, Vector2.zero, Vector2.one, C("202A33"));
                Panel("Floor", room, new Vector2(0, 0), new Vector2(1, .28f), C("42382F"));
                var window = Panel("Window", room, new Vector2(.08f, .43f), new Vector2(.32f, .81f), C("675845"));
                var pasture = Panel("Pasture", window, new Vector2(.04f, .04f), new Vector2(.96f, .96f), C("172733"));
                Panel("Grass", pasture, Vector2.zero, new Vector2(1, .32f), C("334538"));
                Label("WindowLabel", pasture, "JANELA / PASTO", new Vector2(.05f, .45f), new Vector2(.95f, .6f), 23);
                Panel("WindowCross", window, new Vector2(.48f, 0), new Vector2(.52f, 1), C("675845"));
                var door = Panel("Door", room, new Vector2(.76f, .27f), new Vector2(.92f, .8f), C("65533F"));
                Label("DoorLabel", door, "PORTA", new Vector2(0, .48f), new Vector2(1, .6f), 25);
                Panel("DoorHandle", door, new Vector2(.13f, .42f), new Vector2(.2f, .45f), C("C6AD78"));
                var wardrobe = Panel("Wardrobe", room, new Vector2(.39f, .52f), new Vector2(.6f, .8f), C("51483E"));
                Label("WardrobeLabel", wardrobe, "ARMÁRIO", new Vector2(0, .42f), new Vector2(1, .6f), 23);
                var chimney = Panel("Chimney", room, new Vector2(.64f, .59f), new Vector2(.7f, .88f), C("555A5D"));
                Label("ChimneyLabel", chimney, "CHAMINÉ", new Vector2(-.4f, .35f), new Vector2(1.4f, .6f), 18);

                var cradle = Panel("Cradle", room, new Vector2(.35f, .22f), new Vector2(.65f, .48f), C("8E7252"));
                Panel("Mattress", cradle, new Vector2(.04f, .12f), new Vector2(.96f, .92f), C("C6BCA8"));
                var babyObject = new GameObject("Baby", typeof(RectTransform), typeof(BabyController));
                babyObject.transform.SetParent(cradle, false);
                Stretch(babyObject.GetComponent<RectTransform>(), new Vector2(.13f, .2f), new Vector2(.87f, .85f));
                var baby = babyObject.GetComponent<BabyController>();
                var aura = Panel("ProtectionAura", babyObject.transform, new Vector2(-.12f, -.25f), new Vector2(1.12f, 1.22f), new Color(.65f, .83f, .5f, .5f));
                Panel("Blanket", babyObject.transform, new Vector2(.1f, .05f), new Vector2(.9f, .72f), C("E9E4D8"));
                Panel("LeftHand", babyObject.transform, new Vector2(.04f, .55f), new Vector2(.2f, .8f), C("D4A990"));
                Panel("RightHand", babyObject.transform, new Vector2(.8f, .55f), new Vector2(.96f, .8f), C("D4A990"));
                aura.gameObject.SetActive(false);
                Label("CradleLabel", room, "BERÇO — ARTE PROVISÓRIA", new Vector2(.31f, .17f), new Vector2(.69f, .215f), 20);
                Set(game, "baby", baby);

                var hud = Panel("HUD", root, new Vector2(0, .89f), Vector2.one, C("111B23"));
                Label("Title", hud, "A VIGÍLIA", new Vector2(.03f, .2f), new Vector2(.25f, .8f), 32, TextAnchor.MiddleLeft);
                var timer = Label("Timer", hud, "02:30", new Vector2(.42f, .05f), new Vector2(.58f, .95f), 58);
                var healthText = Label("HealthText", hud, "VIDA DO BEBÊ  100 / 100", new Vector2(.69f, .47f), new Vector2(.97f, .88f), 25);
                var healthTrack = Panel("HealthTrack", hud, new Vector2(.71f, .18f), new Vector2(.95f, .36f), C("35434C"));
                var healthFill = Panel("HealthFill", healthTrack, Vector2.zero, Vector2.one, C("7ABF8C")).GetComponent<Image>();
                var protectionText = Label("ProtectionText", root, "SEM PROTEÇÃO", new Vector2(.37f, .82f), new Vector2(.63f, .87f), 24);

                var debugObject = new GameObject("PrototypeDebugControls");
                debugObject.transform.SetParent(systems.transform);
                var debug = debugObject.AddComponent<PrototypeDebugControls>();
                var debugPanel = Panel("DebugPanel_DevelopmentOnly", root, Vector2.zero, new Vector2(1, .14f), C("111B23"));
                Label("DebugLabel", debugPanel, "ETAPA 1 · CONTROLES DE TESTE · AINDA SEM ATAQUES", new Vector2(.03f, .69f), new Vector2(.97f, .94f), 21);
                Button("Damage25", debugPanel, "DANO 25", new Vector2(.08f, .14f), new Vector2(.32f, .59f), debug.Damage25);
                var drainButton = Button("ContinuousDamage", debugPanel, "DANO CONTÍNUO 5/s", new Vector2(.38f, .14f), new Vector2(.62f, .59f), debug.ToggleDrain);
                Button("Protect15s", debugPanel, "PROTEGER 15s", new Vector2(.68f, .14f), new Vector2(.92f, .59f), debug.Protect15Seconds);
                Set(debug, "game", game);
                Set(debug, "debugPanel", debugPanel.gameObject);
                Set(debug, "drainButtonText", drainButton.GetComponentInChildren<Text>());

                var start = Panel("StartPanel", root, Vector2.zero, Vector2.one, new Color(.03f, .06f, .09f, .93f), true);
                Label("StartTitle", start, "A VIGÍLIA", new Vector2(.15f, .58f), new Vector2(.85f, .76f), 88);
                Label("StartDescription", start, "Proteja seu filho até o amanhecer.\n\nProtótipo da etapa 1: vida, tempo e resultado.\nOs ataques serão adicionados na próxima etapa.", new Vector2(.17f, .34f), new Vector2(.83f, .57f), 29);
                Button("StartButton", start, "INICIAR VIGÍLIA", new Vector2(.36f, .2f), new Vector2(.64f, .29f), game.StartGame);
                var result = Panel("ResultPanel", root, Vector2.zero, Vector2.one, new Color(.03f, .06f, .09f, .93f), true);
                var resultTitle = Label("ResultTitle", result, "AMANHECEU", new Vector2(.1f, .56f), new Vector2(.9f, .76f), 76);
                var resultDescription = Label("ResultDescription", result, "", new Vector2(.15f, .38f), new Vector2(.85f, .55f), 31);
                Button("RestartButton", result, "TENTAR NOVAMENTE", new Vector2(.34f, .21f), new Vector2(.66f, .31f), game.StartGame);
                result.gameObject.SetActive(false);

                var ui = canvasObject.AddComponent<UIManager>();
                Set(ui, "game", game);
                Set(ui, "timerText", timer);
                Set(ui, "healthText", healthText);
                Set(ui, "healthFill", healthFill);
                Set(ui, "protectionText", protectionText);
                Set(ui, "protectionAura", aura.gameObject);
                Set(ui, "startPanel", start.gameObject);
                Set(ui, "resultPanel", result.gameObject);
                Set(ui, "resultTitle", resultTitle);
                Set(ui, "resultDescription", resultDescription);
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new InvalidOperationException("Não foi possível salvar a cena MVP.");
            }
            finally
            {
                if (previous.IsValid())
                    SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Base MVP criada. Abra Game Jam > Abrir Base MVP e pressione Play.");
        }

        private static Transform Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color, bool blockRaycasts = false)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            Stretch(obj.GetComponent<RectTransform>(), min, max);
            var image = obj.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = blockRaycasts;
            return obj.transform;
        }

        private static Text Label(string name, Transform parent, string content, Vector2 min, Vector2 max, int size, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            Stretch(obj.GetComponent<RectTransform>(), min, max);
            var text = obj.GetComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = C("EEE8D8");
            text.raycastTarget = false;
            return text;
        }

        private static Button Button(string name, Transform parent, string content, Vector2 min, Vector2 max, UnityAction callback)
        {
            var transform = Panel(name, parent, min, max, C("5B6A59"), true);
            var button = transform.gameObject.AddComponent<Button>();
            button.targetGraphic = transform.GetComponent<Image>();
            Label("Label", transform, content, new Vector2(.03f, .03f), new Vector2(.97f, .97f), 26);
            UnityEventTools.AddPersistentListener(button.onClick, callback);
            return button;
        }

        private static void Stretch(RectTransform transform, Vector2 min, Vector2 max)
        {
            transform.anchorMin = min;
            transform.anchorMax = max;
            transform.offsetMin = Vector2.zero;
            transform.offsetMax = Vector2.zero;
        }

        private static Color C(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var result);
            return result;
        }

        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Game Jam/Verificar Base MVP")]
        public static void Verify()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var previous = SceneManager.GetActiveScene();
            var testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(testScene);
            GameManager game = null;
            try
            {
                var baby = new GameObject("VerificationBaby").AddComponent<BabyController>();
                game = new GameObject("VerificationGame").AddComponent<GameManager>();
                Set(game, "baby", baby);
                Call(baby, "Awake");
                Call(game, "OnEnable");
                Call(game, "Start");
                Check(game.State == GameState.Waiting && game.RemainingTime == 150f, "Estado inicial e timer");
                Call(game, "AdvanceClock", 30f);
                Check(game.RemainingTime == 150f && !baby.TakeDamage(20f), "Waiting não perde tempo nem vida");
                game.StartGame();
                Check(game.IsPlaying && baby.Health == 100f, "Início com vida completa");
                baby.TakeDamage(25f);
                Check(baby.Health == 75f, "Dano aplicado uma vez");
                baby.Protect(15f);
                Check(!baby.TakeDamage(50f) && baby.Health == 75f, "Proteção bloqueia dano");
                Call(baby, "AdvanceProtection", 14f);
                Check(baby.IsProtected, "Proteção permanece antes do prazo");
                Call(baby, "AdvanceProtection", 1f);
                Check(!baby.IsProtected && baby.TakeDamage(5f) && baby.Health == 70f, "Proteção expira aos 15 segundos");
                Call(game, "AdvanceClock", 150f);
                Check(game.State == GameState.Victory && game.RemainingTime == 0f, "Vitória em zero");
                Check(!baby.TakeDamage(1000f) && game.State == GameState.Victory, "Dano após vitória bloqueado");
                game.StartGame();
                Check(game.IsPlaying && game.RemainingTime == 150f && baby.Health == 100f && !baby.IsProtected, "Reinício limpa a partida");
                for (int i = 0; i < 60; i++)
                    baby.TakeDamage(5f / 60f);
                Check(Mathf.Abs(baby.Health - 95f) < .01f, "Dano gradual de 5 pontos por segundo");
                int deaths = 0;
                baby.Died += () => deaths++;
                baby.TakeDamage(1000f);
                baby.TakeDamage(1000f);
                Check(game.State == GameState.Defeat && baby.Health == 0f && deaths == 1, "Derrota e morte emitidas uma vez");
                Call(game, "AdvanceClock", 500f);
                Check(game.State == GameState.Defeat, "Timer não transforma derrota em vitória");
                game.StartGame();
                Check(baby.Health == 100f && game.IsPlaying, "Reinício após derrota");
                File.WriteAllText(ReportPath, "PASS\n14 verificações da base aprovadas.\n" + DateTime.Now.ToString("O"));
                Debug.Log("Base MVP: 14 verificações aprovadas (tempo, vida, proteção, vitória, derrota e reinício).");
            }
            catch (Exception exception)
            {
                File.WriteAllText(ReportPath, "FAIL\n" + exception);
                Debug.LogException(exception);
                throw;
            }
            finally
            {
                if (game != null)
                    Call(game, "OnDisable");
                if (previous.IsValid())
                    SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(testScene, true);
            }
        }

        private static void Call(object target, string method, params object[] arguments)
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, arguments);
        }

        private static void Check(bool condition, string description)
        {
            if (!condition)
                throw new InvalidOperationException("Verificação falhou: " + description);
        }
    }
}
