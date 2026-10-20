using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using CnhTreino;

public static class ProjectBuilder
{
    [MenuItem("CNH LAB/Preparar cena e configurações")]
    public static void Prepare()
    {
        PlayerSettings.companyName="CNH Lab";PlayerSettings.productName="CNH Lab - Treino Categoria B";
        PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
        PlayerSettings.colorSpace=ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;settings.ApplyModifiedProperties();}
        var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var list=tags.FindProperty("tags");bool found=false;for(int i=0;i<list.arraySize;i++)if(list.GetArrayElementAtIndex(i).stringValue=="Obstacle")found=true;
        if(!found){list.InsertArrayElementAtIndex(list.arraySize);list.GetArrayElementAtIndex(list.arraySize-1).stringValue="Obstacle";tags.ApplyModifiedProperties();}
        Directory.CreateDirectory("Assets/Scenes");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("CNH Lab • Bootstrap").AddComponent<TrainingWorld>();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Treino.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Treino.unity",true)};
        // Runtime-generated materials must retain the Standard shader in player builds.
        var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        var shaders=graphics.FindProperty("m_AlwaysIncludedShaders");bool has=false;
        for(int i=0;i<shaders.arraySize;i++)if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==Shader.Find("Standard"))has=true;
        if(!has){shaders.InsertArrayElementAtIndex(shaders.arraySize);shaders.GetArrayElementAtIndex(shaders.arraySize-1).objectReferenceValue=Shader.Find("Standard");graphics.ApplyModifiedProperties();}
        AssetDatabase.SaveAssets();
    }
    static void Check(bool condition,string name){if(!condition)throw new Exception("FAIL: "+name);Debug.Log("PASS: "+name);}
    [MenuItem("CNH LAB/Testar transmissão")]
    public static void TestTransmission()
    {
        var m=new ManualPowertrain();m.clutch=0;Check(!m.Shift(1,0),"troca sem embreagem recusada");
        m.clutch=1;Check(m.Shift(1,0),"primeira com embreagem");m.clutch=0;Check(!m.Start(0),"partida engrenada bloqueada");
        m.clutch=1;Check(m.Start(0),"partida com embreagem");Check(!m.Shift(-1,3),"ré em movimento recusada");
        Check(m.Shift(-1,0),"ré parado aceita");Check(!m.Shift(1,-3),"frente durante ré recusada");
        Check(m.Shift(1,0),"primeira");m.clutch=0;
        for(int i=0;i<200;i++)m.Step(.02f,0,0,0);Check(!m.running&&m.stalls==1,"motor apaga sob carga parado");
        m.clutch=1;m.gear=0;m.Start(0);for(int i=0;i<50;i++)m.Step(.02f,1,0,0);Check(m.rpm>4000,"motor sobe giro em neutro");
        Check(m.Step(.02f,1,0,0)==0,"neutro sem tração");
        Check(!m.Shift(6,0),"marcha inválida recusada");
        File.WriteAllText("Build/transmission-tests.txt","PASS: 12 verificações de transmissão manual.\n");
    }
    [MenuItem("CNH LAB/Gerar jogo Windows")]
    public static void Build()
    {
        Prepare();TestTransmission();
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Treino.unity"},locationPathName="Build/Windows/CNH Lab.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build falhou: "+result.summary.result);
        File.WriteAllText("Build/build-result.txt","Build Windows concluído. "+result.summary.totalSize+" bytes.");
    }
}
