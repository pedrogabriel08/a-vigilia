using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Vigilia.Editor.JamSceneTools;

namespace Vigilia.Editor
{
    [InitializeOnLoad]
    public static class QteStageSetup
    {
        private const string Marker = "Library/VigiliaQteStageSetup.txt";
        static QteStageSetup() => EditorApplication.delayCall += Prepare;
        private static void Prepare()
        {
            if (File.Exists(Marker)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Prepare; return; }
            if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(MvpSceneSetup.ScenePath)) return;
            try { Install(); QteStageVerification.Verify(); } catch (Exception e) { Debug.LogException(e); }
        }
        [MenuItem("Game Jam/Preparar QTE")]
        public static void Install()
        {
            Edit((systems, canvas, game) =>
            {
                var qte = Component<QTEManager>(systems, "QTEManager"); Set(qte, "game", game);
                Set(systems.GetComponentInChildren<BullAttack>(true), "qte", qte);
                Set(systems.GetComponentInChildren<ItemManager>(true), "qte", qte);
                Set(canvas.GetComponentInChildren<WardrobeController>(true), "qte", qte);
                if (canvas.Find("QTEPanel") == null)
                {
                    var panel = Panel("QTEPanel", canvas, Vector2.zero, Vector2.one, new Color(.05f,.02f,.06f,.94f), true);
                    var start = canvas.Find("StartPanel"); if (start != null) panel.SetSiblingIndex(start.GetSiblingIndex());
                    var title = Label("Title", panel, "AFASTE A BRUXA", new Vector2(.1f,.7f), new Vector2(.9f,.86f), 70);
                    var detail = Label("Detail", panel, "", new Vector2(.12f,.52f), new Vector2(.88f,.66f), 32);
                    var timer = Label("Timer", panel, "8.0s", new Vector2(.4f,.4f), new Vector2(.6f,.52f), 50);
                    var track = Panel("ProgressTrack", panel, new Vector2(.2f,.34f), new Vector2(.8f,.38f), new Color(.2f,.2f,.25f));
                    var progress = Panel("Progress", track, Vector2.zero, Vector2.one, new Color(.65f,.75f,.49f)).GetComponent<Image>();
                    var button = Button("React", panel, "REPELIR", new Vector2(.23f,.14f), new Vector2(.77f,.29f), qte.RegisterPress);
                    var input = button.gameObject.AddComponent<QTEButtonInput>(); Set(input,"qte",qte);
                    var view = Component<QTEView>(systems, "QTEView"); Set(view,"qte",qte); Set(view,"panel",panel.gameObject);
                    Set(view,"title",title); Set(view,"detail",detail); Set(view,"timer",timer);
                    Set(view,"progress",progress); Set(view,"buttonLabel",button.GetComponentInChildren<Text>());
                    panel.gameObject.SetActive(false);
                }
                var qtePanel=canvas.Find("QTEPanel");
                var hp=qtePanel.Find("BabyHealth")?.GetComponent<Text>();
                if(hp==null)hp=Label("BabyHealth",qtePanel,"VIDA DO BEBÊ  100 / 100",new Vector2(.1f,.88f),new Vector2(.9f,.97f),34);
                var qteView=systems.GetComponentInChildren<QTEView>(true);Set(qteView,"baby",game.Baby);Set(qteView,"babyHealth",hp);
                canvas.Find("StartPanel/StartDescription").GetComponent<Text>().text =
                    "Observe os bois e use a tesoura na janela.\nSe a bruxa avançar, clique ou pressione Espaço para resistir.\n\nDurante o desafio, o bebê perde vida gradualmente.";
            });
            File.WriteAllText(Marker, "QTE instalado: 12 pressões / 8s / dano 5 por segundo.");
        }
    }
}
