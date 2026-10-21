using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Vigilia.Editor.JamSceneTools;

namespace Vigilia.Editor
{
    [InitializeOnLoad]
    public static class DoorStageSetup
    {
        private const string Marker="Library/VigiliaDoorStageSetup.txt";
        static DoorStageSetup()=>EditorApplication.delayCall+=Prepare;
        private static void Prepare()
        {
            if(File.Exists(Marker))return;
            if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=Prepare;return;}
            if(EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists("Library/VigiliaPrayerFireSetup.txt"))return;
            try{Install();}catch(Exception e){Debug.LogException(e);}
        }
        [MenuItem("Game Jam/Preparar porta e vassoura")]
        public static void Install()
        {
            Edit((systems,canvas,game)=>
            {
                var qte=systems.GetComponentInChildren<QTEManager>(true);
                var items=systems.GetComponentInChildren<ItemManager>(true);
                var audio=Component<PrototypeAudio>(systems,"AudioFeedback");Set(audio,"game",game);
                var door=Component<DoorAttack>(systems,"DoorAttack");Set(door,"qte",qte);Set(door,"audioFeedback",audio);
                var doorObject=canvas.Find("Room_Placeholders/Door");
                var interaction=doorObject.GetComponent<DoorInteraction>()??doorObject.gameObject.AddComponent<DoorInteraction>();Set(interaction,"door",door);
                doorObject.GetComponent<Image>().raycastTarget=true;
                if(doorObject.Find("VisitorText")==null)
                {
                    var text=Label("VisitorText",doorObject,"",new Vector2(-.2f,.05f),new Vector2(1.2f,.35f),24);
                    var mother=Group("MotherVisit",doorObject,new Vector2(.2f,.35f),new Vector2(.8f,.95f));
                    Panel("Body",mother,new Vector2(.1f,0),new Vector2(.9f,.65f),new Color(.35f,.4f,.55f));
                    Panel("Face",mother,new Vector2(.3f,.62f),new Vector2(.7f,.94f),new Color(.78f,.62f,.5f));
                    Set(door,"visitorText",text);Set(door,"motherVisual",mother.gameObject);mother.gameObject.SetActive(false);
                }
                PrayerFireSetup.Target(doorObject,"BroomDropArea",ItemId.Broom,door.RevealWithBroom);
                var contents=canvas.Find("Room_Placeholders/Wardrobe/Contents");
                Stretch((RectTransform)contents.Find("Scissors"),new Vector2(.02f,.35f),new Vector2(.31f,.95f));
                Stretch((RectTransform)contents.Find("Rosary"),new Vector2(.35f,.35f),new Vector2(.64f,.95f));
                if(contents.Find("Broom")==null)
                {
                    var broom=Panel("Broom",contents,new Vector2(.68f,.35f),new Vector2(.98f,.95f),new Color(.24f,.29f,.24f),true);
                    Panel("Handle",broom,new Vector2(.47f,.25f),new Vector2(.53f,.95f),new Color(.65f,.45f,.25f));
                    Panel("Bristles",broom,new Vector2(.25f,.05f),new Vector2(.75f,.35f),new Color(.72f,.6f,.34f));
                    var drag=broom.gameObject.AddComponent<DraggableItem>();Set(drag,"items",items);Set(drag,"dragLayer",canvas.Find("DragLayer"));Int(drag,"item",(int)ItemId.Broom);
                    PrayerFireSetup.AddView(contents,"BroomView",items,ItemId.Broom,new Vector2(.67f,0),new Vector2(1f,.32f));
                    Stretch((RectTransform)contents.Find("ScissorsStatus"),new Vector2(0,.03f),new Vector2(.32f,.23f));
                    contents.Find("ScissorsStatus").GetComponent<Text>().fontSize=14;
                    Stretch((RectTransform)contents.Find("CooldownTrack"),new Vector2(.02f,.25f),new Vector2(.31f,.29f));
                    Stretch((RectTransform)contents.Find("RosaryView"),new Vector2(.33f,0),new Vector2(.66f,.32f));
                    contents.Find("RosaryView/Status").GetComponent<Text>().fontSize=14;
                    contents.Find("BroomView/Status").GetComponent<Text>().fontSize=14;
                    var hud=PrayerFireSetup.AddView(canvas,"BroomCooldownHUD",items,ItemId.Broom,new Vector2(.77f,.16f),new Vector2(.93f,.23f));
                    hud.SetSiblingIndex(canvas.Find("StartPanel").GetSiblingIndex());
                }
                PrayerFireSetup.ConfigureItem(items,ItemId.Broom,7f);
                PrayerFireSetup.AddAttack(systems.GetComponentInChildren<AttackManager>(true),door);
                if(canvas.Find("ScarePanel")==null)
                {
                    var panel=Panel("ScarePanel",canvas,Vector2.zero,Vector2.one,new Color(.04f,.015f,.02f),true);
                    Panel("Face",panel,new Vector2(.24f,.1f),new Vector2(.76f,.95f),new Color(.24f,.2f,.27f));
                    Panel("LeftEye",panel,new Vector2(.32f,.6f),new Vector2(.43f,.7f),new Color(.9f,.3f,.3f));
                    Panel("RightEye",panel,new Vector2(.57f,.6f),new Vector2(.68f,.7f),new Color(.9f,.3f,.3f));
                    Panel("Mouth",panel,new Vector2(.43f,.22f),new Vector2(.57f,.47f),Color.black);
                    var scare=Component<ScareFeedback>(systems,"ScareFeedback");Set(scare,"game",game);Set(scare,"panel",panel.gameObject);panel.gameObject.SetActive(false);
                }
                canvas.Find("StartPanel/StartDescription").GetComponent<Text>().text=
                    "Tesoura → janela · Terço → berço ou chaminé\nVassoura → porta: escute a reação. Clique na porta para resistir.\n\nCliques ou Espaço repelem ataques. Segure o botão na porta.";
                var qtePanel=canvas.Find("QTEPanel");
                if(qtePanel!=null)qtePanel.SetSiblingIndex(canvas.Find("StartPanel").GetSiblingIndex()-1);
            });
            File.WriteAllText(Marker,"Porta mãe/bruxa, vassoura 7s, resistência e consequência de invasão instaladas.");
        }
    }
}
