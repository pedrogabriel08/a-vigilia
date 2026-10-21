using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Vigilia.Editor.JamSceneTools;

namespace Vigilia.Editor
{
    public static class SurvivalVerification
    {
        [MenuItem("Game Jam/Verificar sobrevivência completa")]
        public static void Verify()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);int count=0;GameManager game=null;QTEManager qte=null;ItemManager items=null;
            void Check(bool ok,string message){if(!ok)throw new Exception(message);count++;}
            void Call(object obj,string method,params object[] args)=>obj.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(obj,args);
            void Float(UnityEngine.Object obj,string name,float value){var so=new SerializedObject(obj);so.FindProperty(name).floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
            try
            {
                var baby=new GameObject("Baby").AddComponent<BabyController>();Call(baby,"Awake");
                game=new GameObject("Game").AddComponent<GameManager>();Set(game,"baby",baby);Call(game,"OnEnable");Call(game,"Start");
                qte=new GameObject("QTE").AddComponent<QTEManager>();Set(qte,"game",game);Call(qte,"OnEnable");
                items=new GameObject("Items").AddComponent<ItemManager>();Set(items,"game",game);Set(items,"qte",qte);
                PrayerFireSetup.ConfigureItem(items,ItemId.Rosary,30f);PrayerFireSetup.ConfigureItem(items,ItemId.Broom,7f);Call(items,"OnEnable");
                game.StartGame();
                var fire=new GameObject("Fire").AddComponent<FireAttack>();Set(fire,"qte",qte);
                var smoke=new GameObject("Smoke");var flame=new GameObject("Flame");Set(fire,"smokeSignal",smoke);Set(fire,"flameSignal",flame);
                smoke.SetActive(false);flame.SetActive(false);
                fire.BeginPreAttack(game);Check(smoke.activeSelf&&flame.activeSelf,"Fogo apresenta sinais");
                fire.RepelDuringPreAttack();Check(fire.Phase==AttackPhase.Idle&&!smoke.activeSelf,"São Miguel encerra manifestação");
                baby.Protect(15);fire.BeginPreAttack(game);fire.BeginAttack();Check(!qte.IsActive&&fire.Phase==AttackPhase.Idle,"Ave-Maria evita QTE de fogo");
                baby.ClearProtection();fire.BeginPreAttack(game);fire.BeginAttack();Check(qte.IsActive&&fire.Phase==AttackPhase.Active,"Fogo inicia QTE");
                Check(!items.TryPickUp(ItemId.Rosary),"Não buscar oração durante QTE");
                Call(qte,"Advance",2f);Check(baby.Health==90f,"Fogo usa dano central de 5/s");
                for(int i=0;i<12;i++)qte.RegisterPress();Check(!qte.IsActive&&fire.Phase==AttackPhase.Idle&&!flame.activeSelf,"Sucesso limpa fogo e QTE");
                var rosaryTarget=new GameObject("BabyTarget").AddComponent<ItemDropTarget>();Int(rosaryTarget,"acceptedItem",(int)ItemId.Rosary);
                var prayers=new GameObject("Prayers").AddComponent<RosaryPrayers>();Set(prayers,"baby",baby);rosaryTarget.OnUsed.AddListener(prayers.PrayAveMaria);
                items.TryPickUp(ItemId.Rosary);items.TryUseOn(rosaryTarget);
                Check(baby.IsProtected&&baby.ProtectionRemaining==15f&&items.GetCooldownRemaining(ItemId.Rosary)==30f,"Ave-Maria e recarga compartilhada");
                Check(!items.TryPickUp(ItemId.Rosary),"São Miguel bloqueado pela mesma recarga do terço");
                Call(baby,"AdvanceProtection",15f);Check(!baby.IsProtected,"Escudo expira em 15s");
                var door=new GameObject("Door").AddComponent<DoorAttack>();Set(door,"qte",qte);Float(door,"witchChance",1f);
                door.BeginPreAttack(game);door.RevealWithBroom();door.BeginResistance();
                Check(qte.IsActive&&qte.Type==QTEType.Hold&&qte.Required==2.5f,"Vassoura enfraquece resistência da bruxa");
                qte.SetHolding(true);Call(qte,"Advance",2.5f);
                Check(door.Phase==AttackPhase.Idle&&!qte.IsActive&&Mathf.Abs(baby.Health-77.5f)<.01f,"Resistência repele bruxa e aplica dano gradual");
                door.BeginPreAttack(game);door.BeginResistance();Check(qte.Required==4f,"Bruxa não enfraquecida exige 4s");qte.Cancel();
                Float(door,"witchChance",0f);door.BeginPreAttack(game);door.RevealWithBroom();door.BeginResistance();
                float health=baby.Health;qte.SetHolding(true);Call(qte,"Advance",4f);
                Check(door.Phase==AttackPhase.Idle&&baby.Health==health,"Recusar mãe é inofensivo");
                door.BeginPreAttack(game);door.BeginAttack();Check(door.Phase==AttackPhase.Active&&!qte.IsActive&&baby.Health==health,"Mãe pode visitar sem QTE");door.Cancel();
                Float(door,"witchChance",1f);baby.Protect(15);door.BeginPreAttack(game);door.BeginAttack();
                Check(game.IsPlaying&&door.Phase==AttackPhase.Idle&&!qte.IsActive,"Proteção também bloqueia porta");baby.ClearProtection();
                door.BeginPreAttack(game);door.BeginResistance();Call(qte,"Advance",7f);
                Check(game.State==GameState.Defeat&&!qte.IsActive,"Falha de resistência à bruxa causa derrota");
                game.StartGame();door.BeginPreAttack(game);door.BeginAttack();Check(game.State==GameState.Defeat,"Ignorar bruxa permite invasão");
                game.StartGame();items.TryPickUp(ItemId.Scissors);qte.StartMashQTE(12,8,null);
                Check(!items.HeldItem.HasValue&&!items.CanUseItems,"Início de QTE devolve item carregado");qte.Cancel();
                Check(items.CanUseItems&&items.TryPickUp(ItemId.Broom),"Itens voltam após QTE");items.ReturnHeldItem();
                File.WriteAllText("Library/VigiliaSurvivalVerification.txt",$"PASS\n{count} verificações aprovadas.");Debug.Log($"Sobrevivência: {count} verificações aprovadas.");
            }
            catch(Exception e){File.WriteAllText("Library/VigiliaSurvivalVerification.txt","FAIL\n"+e);throw;}
            finally
            {
                if(items!=null)Call(items,"OnDisable");if(qte!=null)Call(qte,"OnDisable");if(game!=null)Call(game,"OnDisable");
                if(previous.IsValid())SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true);
            }
        }
        [MenuItem("Game Jam/Preparar e verificar versão atual")]
        public static void PrepareAndVerify()
        {
            QteStageSetup.Install();PrayerFireSetup.Install();DoorStageSetup.Install();
            MvpSceneSetup.Verify();BullStageVerification.Verify();ScissorsStageVerification.Verify();QteStageVerification.Verify();Verify();
        }
    }
}
