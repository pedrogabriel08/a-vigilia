using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vigilia.Editor
{
    internal static class JamSceneTools
    {
        internal static void Edit(Action<Transform, Transform, GameManager> edit)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Saia do Play para preparar a cena.");
            var previous = SceneManager.GetActiveScene();
            var scene = SceneManager.GetSceneByPath(MvpSceneSetup.ScenePath);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (!opened && scene.isDirty) throw new InvalidOperationException("Salve a cena antes de preparar esta etapa.");
            if (opened) scene = EditorSceneManager.OpenScene(MvpSceneSetup.ScenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                Transform systems = null, canvas = null;
                foreach (var obj in scene.GetRootGameObjects())
                {
                    if (obj.name == "Systems") systems = obj.transform;
                    if (obj.name == "Canvas") canvas = obj.transform;
                }
                if (systems == null || canvas == null) throw new InvalidOperationException("Cena MVP incompleta.");
                edit(systems, canvas, systems.GetComponentInChildren<GameManager>(true));
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Falha ao salvar a cena.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }
        internal static T Component<T>(Transform parent, string name) where T : Component
        {
            var child = parent.Find(name);
            if (child == null) { child = new GameObject(name).transform; child.SetParent(parent, false); }
            return child.GetComponent<T>() ?? child.gameObject.AddComponent<T>();
        }
        internal static RectTransform Group(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); Stretch(rect, min, max); return rect;
        }
        internal static RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color, bool raycast = false)
        {
            var rect = Group(name, parent, min, max); var image = rect.gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = raycast; return rect;
        }
        internal static Text Label(string name, Transform parent, string text, Vector2 min, Vector2 max, int size = 28)
        {
            var rect = Group(name, parent, min, max); var label = rect.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; label.text = text; label.raycastTarget = false; return label;
        }
        internal static Button Button(string name, Transform parent, string text, Vector2 min, Vector2 max, UnityAction action)
        {
            var rect = Panel(name, parent, min, max, new Color(.33f,.39f,.32f), true);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            Label("Label", rect, text, Vector2.zero, Vector2.one);
            UnityEventTools.AddPersistentListener(button.onClick, action); return button;
        }
        internal static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        internal static void Set(UnityEngine.Object obj, string field, UnityEngine.Object value)
        { var so = new SerializedObject(obj); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        internal static void Int(UnityEngine.Object obj, string field, int value)
        { var so = new SerializedObject(obj); so.FindProperty(field).intValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
