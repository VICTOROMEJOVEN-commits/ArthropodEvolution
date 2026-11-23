using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class DeliveryBuilder {
 static DeliveryBuilder(){EditorApplication.update+=CheckDeliveryRequest;}
 static void CheckDeliveryRequest(){
  const string path="Temp/ArthropodDeliveryRequest.txt";
  if(EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(path))return;
  string cmd=File.ReadAllText(path).Trim();
  if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
  var target=cmd=="WEBGL"?BuildTarget.WebGL:BuildTarget.StandaloneWindows64;
  if(EditorUserBuildSettings.activeBuildTarget!=target){
   EditorUserBuildSettings.SwitchActiveBuildTarget(cmd=="WEBGL"?BuildTargetGroup.WebGL:BuildTargetGroup.Standalone,target);return;
  }
  File.Delete(path);
  try{if(cmd=="WEBGL")BuildWebGL();else BuildWindows();}
  catch(Exception ex){Debug.LogException(ex);}
 }
 [Serializable] class Catalog { public Atlas[] atlases; }
 [Serializable] class Atlas { public string name; public Cell[] cells; }
 [Serializable] class Cell { public int index,x,y,w,h; }
 static string Root {
  get {
   var args=Environment.GetCommandLineArgs();
   int i=Array.IndexOf(args,"-releaseRoot");
   if(i>=0&&i+1<args.Length)return Path.GetFullPath(args[i+1]);
   return Path.GetFullPath(File.ReadAllText("Temp/ArthropodReleaseRoot.txt").Trim());
  }
 }
 static void Prepare() {
  DemoBuilder.EnsureScene();
  PlayerSettings.companyName="Botavitos";PlayerSettings.productName="Arthropod Evolution";
  PlayerSettings.bundleVersion="1.2";
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
  PlayerSettings.WebGL.decompressionFallback=false;
  PlayerSettings.WebGL.threadsSupport=false;
  PlayerSettings.WebGL.initialMemorySize=256;
  PlayerSettings.WebGL.maximumMemorySize=2048;
  PlayerSettings.WebGL.template="APPLICATION:Default";
  ValidateEnemyFrames();
  AssetDatabase.SaveAssets();
 }
 [MenuItem("Arthropod/Entrega/Validar recortes de enemigos")]
 public static void ValidateEnemyFrames() {
  var catalog=JsonUtility.FromJson<Catalog>(File.ReadAllText("Assets/Resources/Art/atlas_layout.json"));
  var cells=catalog.atlases.First(a=>a.name=="enemy_evolved").cells;
  var texture=Resources.Load<Texture2D>("Art/enemy_evolved");
  if(cells.Length!=16)throw new Exception("Expected sixteen enemy frames");
  foreach(var cell in cells) {
   if(cell.x<0||cell.y<0||cell.w<=0||cell.h<=0||cell.x+cell.w>texture.width||cell.y+cell.h>texture.height)
    throw new Exception("Invalid frame "+cell.index);
   if(cell.index>=8&&cell.index<12&&cell.h<230)throw new Exception("Blue enemy antennae clipped");
   if(cell.index>=12&&cell.h<300)throw new Exception("Boss legs clipped");
  }
  Directory.CreateDirectory("Validation");
  File.WriteAllText("Validation/sprite-crops.txt","PASS 16 cuadros con limites individuales\nPASS 4 cuadros de escarabajo completos\nPASS 4 cuadros de jefe completos\n"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
 }
 static void Build(BuildTarget target,string sub,string filename) {
  Prepare();var destination=Path.Combine(Root,sub);Directory.CreateDirectory(destination);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
   scenes=new[]{"Assets/Scenes/Jardin.unity"},
   locationPathName=string.IsNullOrEmpty(filename)?destination:Path.Combine(destination,filename),
   target=target,options=BuildOptions.None
  });
  if(target==BuildTarget.WebGL&&report.summary.result==BuildResult.Succeeded)CustomizeWebIndex(destination);
  Directory.CreateDirectory(Path.Combine(Root,"Validacion"));
  File.WriteAllText(Path.Combine(Root,"Validacion",sub+"-build.txt"),
   report.summary.result+"\n"+report.summary.totalErrors+" errors\n"+report.summary.totalWarnings+" warnings\n"+report.summary.totalTime+"\n"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+sub);
 }
 static void CustomizeWebIndex(string destination){
  string path=Path.Combine(destination,"index.html");string html=File.ReadAllText(path);
  html=html.Replace("lang=\"en-us\"","lang=\"es\"").Replace("<title>Unity Web Player | Arthropod Evolution</title>","<title>Arthropod Evolution - Botavitos</title>");
  html=html.Replace("// config.autoSyncPersistentDataPath = true;","config.autoSyncPersistentDataPath = true;");
  html=html.Replace("width=960 height=600","width=960 height=540");
  html=html.Replace("</head>","<style>html,body{margin:0;width:100%;height:100%;overflow:hidden;background:#071916}#unity-container.unity-desktop,#unity-container.unity-mobile{position:absolute;left:0;top:0;transform:none;width:100vw;height:100vh;display:flex;align-items:center;justify-content:center}#unity-canvas{width:min(100vw,177.7778vh)!important;height:min(100vh,56.25vw)!important;outline:none}#unity-footer{display:none}#unity-warning{z-index:9}</style></head>");
  File.WriteAllText(path,html);
 }
 public static void BuildWindows(){Build(BuildTarget.StandaloneWindows64,"Windows","ArthropodEvolution.exe");}
 public static void BuildWebGL(){Build(BuildTarget.WebGL,"WebGL",null);}
}
