using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class DemoBuilder {
 static DemoBuilder(){EditorApplication.delayCall+=EnsureScene;EditorApplication.update+=CheckRequests;}
 static void CheckRequests(){
  const string request="Temp/ArthropodRequest.txt";if(EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(request))return;
  string cmd;try{cmd=File.ReadAllText(request).Trim();File.Delete(request);}catch(IOException){return;}
  if(cmd=="REFRESH")AssetDatabase.Refresh();
  if(cmd=="ART"){foreach(string path in Directory.GetFiles("Assets/Resources/Art","*.png")){var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Point;importer.SaveAndReimport();}}
  if(cmd=="PLAY")EditorApplication.isPlaying=true;
  if(cmd=="STOP")EditorApplication.isPlaying=false;
  if(cmd=="BUILD"){if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;EditorApplication.delayCall+=Build;}else Build();}
  if(cmd=="VALIDATE")Validate();
  if(cmd=="SMOKE"){if(!EditorApplication.isPlaying){File.WriteAllText("Temp/ArthropodSmokePending.txt","1");EditorApplication.isPlaying=true;}else UnityEngine.Object.FindFirstObjectByType<Arthropod.DemoGame>()?.RunSmokeTest();}
  if(cmd=="PREVIEW" && EditorApplication.isPlaying)UnityEngine.Object.FindFirstObjectByType<Arthropod.DemoGame>()?.StartPreviews();
 }
 [MenuItem("Arthropod/Preparar y abrir Jardín")]
 public static void EnsureScene(){
  if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)return;
  const string path="Assets/Scenes/Jardin.unity";
  if(!File.Exists(path)){
   Directory.CreateDirectory("Assets/Scenes");
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var root=new GameObject("ARTHROPOD EVOLUTION - Demo");root.AddComponent<Arthropod.DemoGame>();
   EditorSceneManager.SaveScene(scene,path);
  }
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(path,true)};
  PlayerSettings.companyName="Bovatitos";PlayerSettings.productName="Arthropod Evolution";
  PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
  PlayerSettings.runInBackground=false;PlayerSettings.resizableWindow=true;
  PlayerSettings.colorSpace=ColorSpace.Gamma;
  if(EditorSceneManager.GetActiveScene().path!=path)EditorSceneManager.OpenScene(path);
 }
 [MenuItem("Arthropod/Compilar demo Windows")]
 public static void Build(){EnsureScene();Directory.CreateDirectory("Build/Windows");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Jardin.unity"},locationPathName="Build/Windows/ArthropodEvolution.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});File.WriteAllText("Build/build-result.txt",report.summary.result+"\n"+report.summary.totalErrors+" errors");}
 [MenuItem("Arthropod/Validar datos")]
 public static void Validate(){
  for(int i=0;i<5;i++)for(int r=0;r<5;r++){if(Arthropod.EvolutionData.Damage[i][r]<=0 || Arthropod.EvolutionData.Interval[i][r]<=0)throw new System.Exception("Invalid combat data");if(r>0 && Arthropod.EvolutionData.Damage[i][r]<=Arthropod.EvolutionData.Damage[i][r-1])throw new System.Exception("Damage must increase");}
  Debug.Log("Arthropod: 12 mutaciones, 5 rangos; datos válidos.");
 }
}
