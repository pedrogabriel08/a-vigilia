using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Vigilia.Editor
{
    [InitializeOnLoad]
    public static class WebBuild
    {
        private const string Pending="VigiliaWebBuildPending";
        static WebBuild()=>EditorApplication.update+=ContinueBuild;
        [MenuItem("Game Jam/Gerar build Web")]
        public static void Begin()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            SessionState.SetBool(Pending,true);
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.WebGL)
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.WebGL,BuildTarget.WebGL);
        }
        private static void ContinueBuild()
        {
            if(!SessionState.GetBool(Pending,false)||EditorApplication.isCompiling||EditorApplication.isUpdating||
                EditorApplication.isPlayingOrWillChangePlaymode||EditorUserBuildSettings.activeBuildTarget!=BuildTarget.WebGL)return;
            SessionState.SetBool(Pending,false);
            try
            {
                PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.threadsSupport=false;
                PlayerSettings.companyName="Equipe Game Jam";PlayerSettings.productName="A Vigília";
                Directory.CreateDirectory("Builds/Web");
                File.WriteAllText("Library/VigiliaWebBuild.txt","RUNNING\n"+DateTime.Now.ToString("O"));
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{MvpSceneSetup.ScenePath},locationPathName="Builds/Web",target=BuildTarget.WebGL,options=BuildOptions.None});
                File.WriteAllText("Library/VigiliaWebBuild.txt",report.summary.result+"\nErrors="+report.summary.totalErrors+"\nBytes="+report.summary.totalSize);
                Debug.Log("Build Web: "+report.summary.result);
            }
            catch(Exception e){File.WriteAllText("Library/VigiliaWebBuild.txt","FAIL\n"+e);Debug.LogException(e);}
        }
    }
}
