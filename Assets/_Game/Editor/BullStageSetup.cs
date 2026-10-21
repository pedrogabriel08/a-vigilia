using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vigilia.Editor
{
    [InitializeOnLoad]
    public static class BullStageSetup
    {
        private const string Marker = "Library/VigiliaBullStageSetup.txt";

        static BullStageSetup() => EditorApplication.delayCall += PrepareOnce;

        private static void PrepareOnce()
        {
            if (File.Exists(Marker))
                return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += PrepareOnce;
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(MvpSceneSetup.ScenePath))
                return;

            var current = SceneManager.GetActiveScene();
            if (current.path == MvpSceneSetup.ScenePath && current.isDirty)
            {
                Debug.Log("Boi: salve suas alterações e use Game Jam > Preparar eventos do boi.");
                return;
            }
            try
            {
                Install();
                BullStageVerification.Verify();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        [MenuItem("Game Jam/Preparar eventos do boi")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var previous = SceneManager.GetActiveScene();
            var scene = SceneManager.GetSceneByPath(MvpSceneSetup.ScenePath);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (!openedHere && scene.isDirty)
                throw new InvalidOperationException("Salve a cena antes de preparar os eventos do boi.");
            if (openedHere)
                scene = EditorSceneManager.OpenScene(MvpSceneSetup.ScenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);

            try
            {
                var systems = Root(scene, "Systems");
                var canvas = Root(scene, "Canvas");
                var game = systems.GetComponentInChildren<GameManager>(true);
                if (game == null)
                    throw new InvalidOperationException("GameManager não encontrado na cena MVP.");

                // Não recria a sala nem reposiciona objetos já existentes.
                var manager = systems.GetComponentInChildren<AttackManager>(true);
                if (manager == null)
                {
                    var pasture = canvas.Find("Room_Placeholders/Window/Pasture");
                    if (pasture == null)
                        throw new InvalidOperationException("Pasto não encontrado. Conecte os novos componentes manualmente.");
                    var bullsRoot = new GameObject("Bulls", typeof(RectTransform)).GetComponent<RectTransform>();
                    bullsRoot.SetParent(pasture, false);
                    Stretch(bullsRoot, Vector2.zero, Vector2.one);
                    var bulls = new BullVisual[4];
                    bulls[0] = Cow("Bull_01", bullsRoot, new Vector2(.04f, .11f), new Vector2(.45f, .44f));
                    bulls[1] = Cow("Bull_02", bullsRoot, new Vector2(.55f, .11f), new Vector2(.96f, .44f));
                    bulls[2] = Cow("Bull_03", bullsRoot, new Vector2(.04f, .53f), new Vector2(.45f, .86f));
                    bulls[3] = Cow("Bull_04", bullsRoot, new Vector2(.55f, .53f), new Vector2(.96f, .86f));
                    var oldLabel = pasture.Find("WindowLabel");
                    if (oldLabel != null)
                        oldLabel.gameObject.SetActive(false);

                    var attackObject = new GameObject("BullAttack", typeof(BullAttack));
                    attackObject.transform.SetParent(systems, false);
                    var bullAttack = attackObject.GetComponent<BullAttack>();
                    SetArray(bullAttack, "bulls", bulls);

                    var managerObject = new GameObject("AttackManager", typeof(AttackManager));
                    managerObject.transform.SetParent(systems, false);
                    manager = managerObject.GetComponent<AttackManager>();
                    Set(manager, "game", game);
                    SetArray(manager, "attacks", new AttackEvent[] { bullAttack });
                }

                var debugLabel = canvas.Find("DebugPanel_DevelopmentOnly/DebugLabel");
                if (debugLabel != null)
                {
                    var view = debugLabel.GetComponent<AttackDebugView>();
                    if (view == null)
                        view = debugLabel.gameObject.AddComponent<AttackDebugView>();
                    var label = debugLabel.GetComponent<Text>();
                    label.text = "TESTE · SEM QTE/DANO · AGUARDANDO PARTIDA";
                    Set(view, "attacks", manager);
                    Set(view, "statusText", label);
                }

                var description = canvas.Find("StartPanel/StartDescription")?.GetComponent<Text>();
                if (description != null)
                    description.text = "Observe os bois pela janela.\nUm deles pode revelar algo estranho.\n\nPrévia de observação: ainda sem tesoura e QTE.";
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Falha ao salvar os eventos do boi.");
                File.WriteAllText(Marker, "Instalado: eventos sequenciais e pré-ataque do boi.\n" + DateTime.Now.ToString("O"));
                Debug.Log("Boi preparado: intervalos de 8–14s, pré-ataque de 5s. Ainda sem QTE/dano de ataque.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static BullVisual Cow(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(BullVisual));
            obj.transform.SetParent(parent, false);
            Stretch(obj.GetComponent<RectTransform>(), min, max);
            var normal = Appearance("NormalAppearance", obj.transform, false);
            var strange = Appearance("StrangeAppearance", obj.transform, true);
            var visual = obj.GetComponent<BullVisual>();
            Set(visual, "normalAppearance", normal);
            Set(visual, "strangeAppearance", strange);
            visual.SetStrange(false);
            return visual;
        }

        private static GameObject Appearance(string name, Transform parent, bool strange)
        {
            var group = new GameObject(name, typeof(RectTransform));
            group.transform.SetParent(parent, false);
            Stretch(group.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            Color body = C(strange ? "69647D" : "BBB4A0");
            Color head = C(strange ? "383343" : "A49D8B");
            Color horn = C(strange ? "B9B5CB" : "D2C9AC");
            Color eye = C(strange ? "E67865" : "242B2C");
            Block("LeftLeg", group.transform, new Vector2(.23f, .06f), new Vector2(.31f, .35f), head);
            Block("RightLeg", group.transform, new Vector2(.57f, .06f), new Vector2(.65f, .35f), head);
            Block("Tail", group.transform, new Vector2(.09f, .28f), new Vector2(.17f, .55f), head);
            Block("Body", group.transform, new Vector2(.15f, .28f), new Vector2(.74f, .66f), body);
            Block("HornLeft", group.transform, new Vector2(.65f, .72f), new Vector2(.7f, strange ? 1f : .92f), horn);
            Block("HornRight", group.transform, new Vector2(.87f, .72f), new Vector2(.92f, strange ? 1f : .92f), horn);
            Block("Head", group.transform, new Vector2(.62f, .43f), new Vector2(.95f, .82f), head);
            Block("Muzzle", group.transform, new Vector2(.65f, .43f), new Vector2(.94f, .54f), body);
            Block("EyeLeft", group.transform, new Vector2(.68f, .66f), new Vector2(.74f, .74f), eye);
            Block("EyeRight", group.transform, new Vector2(.84f, .66f), new Vector2(.9f, .74f), eye);
            return group;
        }

        private static void Block(string name, Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            Stretch(obj.GetComponent<RectTransform>(), min, max);
            var image = obj.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static Transform Root(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name)
                    return root.transform;
            throw new InvalidOperationException("Objeto não encontrado: " + name);
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
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }

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
