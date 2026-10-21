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
    public static class QteStageVerification
    {
        [MenuItem("Game Jam/Verificar QTE")]
        public static void Verify()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene); int checks = 0;
            GameManager game = null; QTEManager qte = null;
            void Check(bool ok, string message) { if (!ok) throw new Exception(message); checks++; }
            void Call(object obj, string method, params object[] args) => obj.GetType().GetMethod(method, BindingFlags.Instance|BindingFlags.NonPublic).Invoke(obj,args);
            try
            {
                var baby = new GameObject("Baby").AddComponent<BabyController>(); Call(baby,"Awake");
                game = new GameObject("Game").AddComponent<GameManager>(); Set(game,"baby",baby); Call(game,"OnEnable"); Call(game,"Start");
                qte = new GameObject("QTE").AddComponent<QTEManager>(); Set(qte,"game",game); Call(qte,"OnEnable");
                QTEResult? result = null;
                Check(!qte.StartMashQTE(12,8,r=>result=r),"Não iniciar fora da partida");
                game.StartGame();
                Check(qte.StartMashQTE(12,8,r=>result=r,s=>baby.TakeDamage(5*s)),"Iniciar mash");
                Check(!qte.StartMashQTE(2,1,null),"Sem sobreposição");
                Call(qte,"Advance",2f); Check(Mathf.Abs(baby.Health-90f)<.01f,"Dano gradual de 5/s");
                for(int i=0;i<11;i++)qte.RegisterPress(); Check(qte.IsActive,"Não vencer antes de 12");
                qte.RegisterPress(); Check(!qte.IsActive && result==QTEResult.Success,"Sucesso ao pressionar 12 vezes");
                Call(qte,"Advance",100f); Check(baby.Health==90f,"Sem dano após sucesso");
                qte.StartMashQTE(12,8,r=>result=r,s=>baby.TakeDamage(5*s)); Call(qte,"Advance",100f);
                Check(result==QTEResult.Failure && baby.Health==50f,"Timeout limita dano a 8s, sem dano extra");
                qte.StartMashQTE(12,8,r=>result=r); qte.Cancel(); Check(result==QTEResult.Cancelled && !qte.IsActive,"Cancelamento limpo");
                qte.StartHoldQTE(3,7,r=>result=r); Call(qte,"Advance",2f); Check(qte.Progress==0f,"Hold exige segurar");
                qte.SetHolding(true); Call(qte,"Advance",3f); Check(result==QTEResult.Success,"Hold resolve após resistência");
                qte.StartMashQTE(12,8,r=>result=r); Call(game,"AdvanceClock",150f);
                Check(game.State==GameState.Victory && !qte.IsActive && result==QTEResult.Cancelled,"Vitória cancela QTE");
                game.StartGame(); baby.TakeDamage(98f); qte.StartMashQTE(12,8,r=>result=r,s=>baby.TakeDamage(s*5)); Call(qte,"Advance",1f);
                Check(game.State==GameState.Defeat && !qte.IsActive && result==QTEResult.Cancelled,"Morte durante tick não gera callback duplo");
                game.StartGame(); Check(qte.StartMashQTE(12,8,null),"Reinício permite novo desafio"); qte.Cancel();
                File.WriteAllText("Library/VigiliaQteStageVerification.txt",$"PASS\n{checks} verificações aprovadas.");
                Debug.Log($"QTE: {checks} verificações aprovadas.");
            }
            catch(Exception e) { File.WriteAllText("Library/VigiliaQteStageVerification.txt","FAIL\n"+e); throw; }
            finally
            {
                if(qte!=null)Call(qte,"OnDisable"); if(game!=null)Call(game,"OnDisable");
                if(previous.IsValid())SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene,true);
            }
        }
    }
}
