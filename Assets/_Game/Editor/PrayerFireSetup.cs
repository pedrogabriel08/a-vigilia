using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Vigilia.Editor.JamSceneTools;

namespace Vigilia.Editor
{
    [InitializeOnLoad]
    public static class PrayerFireSetup
    {
        private const string Marker="Library/VigiliaPrayerFireSetup.txt";
        static PrayerFireSetup()=>EditorApplication.delayCall+=Prepare;
        private static void Prepare()
        {
            if(File.Exists(Marker))return;
            if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=Prepare;return;}
            if(EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists("Library/VigiliaQteStageSetup.txt"))return;
            try{Install();}catch(Exception e){Debug.LogException(e);}
        }
        [MenuItem("Game Jam/Preparar terço e fogo")]
        public static void Install()
        {
            Edit((systems,canvas,game)=>
            {
                var qte=systems.GetComponentInChildren<QTEManager>(true);
                var items=systems.GetComponentInChildren<ItemManager>(true);
                var prayers=Component<RosaryPrayers>(systems,"RosaryPrayers"); Set(prayers,"baby",game.Baby);
                var fire=Component<FireAttack>(systems,"FireAttack"); Set(fire,"qte",qte);
                var chimney=canvas.Find("Room_Placeholders/Chimney");
                if(chimney.Find("SmokeSignal")==null)
                {
                    var smoke=Group("SmokeSignal",chimney,new Vector2(-.2f,.55f),new Vector2(1.2f,1.25f));
                    Panel("WispA",smoke,new Vector2(.1f,.1f),new Vector2(.65f,.8f),new Color(.55f,.58f,.61f,.6f));
                    Panel("WispB",smoke,new Vector2(.5f,.4f),new Vector2(.95f,1f),new Color(.7f,.65f,.72f,.55f));
                    var flame=Panel("FlameSignal",chimney,new Vector2(.15f,.02f),new Vector2(.85f,.4f),new Color(.9f,.35f,.1f));
                    Panel("Core",flame,new Vector2(.25f,0f),new Vector2(.75f,.7f),new Color(1f,.75f,.3f));
                    Set(fire,"smokeSignal",smoke.gameObject);Set(fire,"flameSignal",flame.gameObject);
                    smoke.gameObject.SetActive(false);flame.gameObject.SetActive(false);
                }
                Target(chimney,"RosaryDropArea",ItemId.Rosary,fire.RepelDuringPreAttack);
                Target(canvas.Find("Room_Placeholders/Cradle"),"RosaryDropArea",ItemId.Rosary,prayers.PrayAveMaria);
                var contents=canvas.Find("Room_Placeholders/Wardrobe/Contents");
                var scissors=contents.Find("Scissors") as RectTransform;
                Stretch(scissors,new Vector2(.03f,.34f),new Vector2(.46f,.94f));
                if(contents.Find("Rosary")==null)
                {
                    var rosary=Panel("Rosary",contents,new Vector2(.54f,.34f),new Vector2(.97f,.94f),new Color(.22f,.28f,.3f),true);
                    var drag=rosary.gameObject.AddComponent<DraggableItem>();Set(drag,"items",items);Set(drag,"dragLayer",canvas.Find("DragLayer"));Int(drag,"item",(int)ItemId.Rosary);
                    Label("Art",rosary,"✝",new Vector2(.05f,.1f),new Vector2(.95f,.95f),70);
                    AddView(contents,"RosaryView",items,ItemId.Rosary,new Vector2(.52f,0f),new Vector2(.98f,.32f));
                    // Reaproveita a apresentação anterior da tesoura no lado esquerdo.
                    Stretch(contents.Find("ScissorsStatus").GetComponent<RectTransform>(),new Vector2(.01f,.03f),new Vector2(.48f,.23f));
                    Stretch(contents.Find("CooldownTrack").GetComponent<RectTransform>(),new Vector2(.03f,.25f),new Vector2(.46f,.29f));
                    var hud=AddView(canvas,"RosaryCooldownHUD",items,ItemId.Rosary,new Vector2(.08f,.24f),new Vector2(.32f,.31f));
                    hud.SetSiblingIndex(canvas.Find("StartPanel").GetSiblingIndex());
                }
                ConfigureItem(items,ItemId.Rosary,30f);
                AddAttack(systems.GetComponentInChildren<AttackManager>(true),fire);
                canvas.Find("StartPanel/StartDescription").GetComponent<Text>().text=
                    "Tesoura na janela · Terço no berço: Ave-Maria\nTerço na chaminé: São Miguel · Recarga do terço: 30s\n\nObserve os sinais. Resista aos ataques com cliques ou Espaço.";
            });
            File.WriteAllText(Marker,"Terço compartilhado 30s, proteção 15s e ataque de fogo instalados.");
        }
        internal static void Target(Transform parent,string name,ItemId item,UnityEngine.Events.UnityAction callback)
        {
            if(parent.Find(name)!=null)return;
            var rect=Panel(name,parent,Vector2.zero,Vector2.one,Color.clear,true);
            var target=rect.gameObject.AddComponent<ItemDropTarget>();Int(target,"acceptedItem",(int)item);
            UnityEventTools.AddPersistentListener(target.OnUsed,callback);
        }
        internal static RectTransform AddView(Transform parent,string name,ItemManager items,ItemId item,Vector2 min,Vector2 max)
        {
            var root=Group(name,parent,min,max);
            var label=Label("Status",root,"PRONTO",new Vector2(0,.35f),Vector2.one,20);
            var track=Panel("Track",root,new Vector2(.05f,.1f),new Vector2(.95f,.22f),new Color(.2f,.25f,.28f));
            var fill=Panel("Fill",track,Vector2.zero,Vector2.one,new Color(.5f,.75f,.55f)).GetComponent<Image>();
            var view=root.gameObject.AddComponent<ItemCooldownView>();Set(view,"items",items);Set(view,"label",label);Set(view,"progress",fill);Int(view,"item",(int)item);return root;
        }
        internal static void ConfigureItem(ItemManager items,ItemId id,float cooldown)
        {
            var so=new SerializedObject(items);var array=so.FindProperty("items");int index=-1;
            for(int i=0;i<array.arraySize;i++)if(array.GetArrayElementAtIndex(i).FindPropertyRelative("item").intValue==(int)id)index=i;
            if(index<0){index=array.arraySize;array.arraySize++;}
            var entry=array.GetArrayElementAtIndex(index);entry.FindPropertyRelative("item").intValue=(int)id;
            entry.FindPropertyRelative("cooldownSeconds").floatValue=cooldown;so.ApplyModifiedPropertiesWithoutUndo();
        }
        internal static void AddAttack(AttackManager manager,AttackEvent attack)
        {
            var so=new SerializedObject(manager);var array=so.FindProperty("attacks");
            for(int i=0;i<array.arraySize;i++)if(array.GetArrayElementAtIndex(i).objectReferenceValue==attack)return;
            int index=array.arraySize;array.arraySize++;array.GetArrayElementAtIndex(index).objectReferenceValue=attack;so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
