using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Random=UnityEngine.Random;

namespace Arthropod {
// A self-contained, editable vertical slice. Coordinates are in world units.
public sealed class DemoGame:MonoBehaviour {
 enum Mode {Menu,Intro,Playing,Upgrade,Pause,Options,Result,Credits}
 sealed class Enemy {public GameObject go;public SpriteRenderer sr;public Vector2 pos,dir,target;public float hp,max,speed,damage,timer,cooldown,flash;public int type,phase,attack,frame;public bool dead;}
 sealed class Drop {public SpriteRenderer sr;public Vector2 pos;public int value;}
 sealed class Fx {public SpriteRenderer sr;public Vector2 vel;public float time,total;public Color color;}
 sealed class FloatText {public Vector2 pos;public string text;public Color color;public float time;}
 sealed class Marker {public LineRenderer line;public float time;}
 [Serializable] sealed class Settings {public float music=.55f,effects=.65f,shake=.5f;public bool numbers=true,reducedFlash=false,fullScreen=true;public int best,cap=60;}
 Mode mode=Mode.Menu,returnMode=Mode.Menu;
 Settings settings=new Settings();
 Camera cam; Transform world,actors,decorations; SpriteRenderer player,shadow; SpriteRenderer[] parts=new SpriteRenderer[12];
 readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
 readonly List<Rect> solids=new List<Rect>(),puddles=new List<Rect>();
 readonly List<Enemy> enemies=new List<Enemy>();readonly List<Drop> drops=new List<Drop>();
 readonly List<Fx> effects=new List<Fx>();readonly List<FloatText> texts=new List<FloatText>();readonly List<Marker> markers=new List<Marker>();
 readonly Queue<Enemy> enemyPool=new Queue<Enemy>();readonly Queue<SpriteRenderer> dropPool=new Queue<SpriteRenderer>(),fxPool=new Queue<SpriteRenderer>();
 readonly int[] ranks=new int[12];readonly float[] cooldowns=new float[5];
 Vector2 pos=new Vector2(0,-10),facing=Vector2.up;float hp=100,maxHp=100,clock,withoutDamage,invulnerable,shellTimer,spawnCredit,bannerTime,shakeTime,totalDamage,upgradeLock;
 int level=1,xp,kills,pending,selected;int[] offers=Array.Empty<int>();bool bossSpawned,won,elite1,elite2;Enemy boss;
 string banner="";float introTime;AudioSource music,sfx;AudioClip menuTrack,gardenTrack,bossTrack;float sfxLimiter;Material lineMaterial;
 Font font;GUIStyle label,small,title,button,body;Texture2D white;bool guiReady;Vector2 creditsScroll;
 static readonly Color Bg=EvolutionData.C("101f20"),Panel=EvolutionData.C("172d2d"),Mint=EvolutionData.C("b8ddab"),Gold=EvolutionData.C("ebc787"),Muted=EvolutionData.C("8fa6a0"),Red=EvolutionData.C("e99b8b");
 string savePath;

 void Awake(){
  Application.targetFrameRate=60;QualitySettings.vSyncCount=1;Random.InitState(671);
  savePath=Path.Combine(Application.persistentDataPath,"arthropod-demo-v1.json");Load();Application.targetFrameRate=settings.cap;QualitySettings.vSyncCount=0;Screen.fullScreen=settings.fullScreen;
  var old=Camera.main;if(old)Destroy(old.gameObject);
  cam=new GameObject("Cámara cenital").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=8.5f;cam.backgroundColor=EvolutionData.C("293e2c");cam.clearFlags=CameraClearFlags.SolidColor;cam.transform.position=new Vector3(0,-10,-10);cam.nearClipPlane=.1f;cam.farClipPlane=100;
  world=new GameObject("Jardín de los Descartes").transform;actors=new GameObject("Criaturas y mutaciones").transform;decorations=new GameObject("Escenografía").transform;
  cam.gameObject.AddComponent<AudioListener>();
  lineMaterial=new Material(Shader.Find("Sprites/Default"));
  BuildGarden();CreatePlayer();
  music=gameObject.AddComponent<AudioSource>();music.loop=true;music.volume=settings.music*.42f;
  sfx=gameObject.AddComponent<AudioSource>();sfx.volume=settings.effects;
  menuTrack=Resources.Load<AudioClip>("Audio/menu");gardenTrack=Resources.Load<AudioClip>("Audio/garden");bossTrack=Resources.Load<AudioClip>("Audio/boss");PlayMusic(menuTrack);
  Time.timeScale=1;
  #if UNITY_EDITOR
  if(File.Exists("Temp/ArthropodSmokePending.txt")){File.Delete("Temp/ArthropodSmokePending.txt");Invoke(nameof(RunSmokeTest),1);}
  #endif
 }
 Sprite Art(string key){if(sprites.TryGetValue(key,out var s))return s;var t=Resources.Load<Texture2D>("Art/"+key);if(!t){t=Texture2D.whiteTexture;}t.filterMode=FilterMode.Point;s=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),32);sprites[key]=s;return s;}
 SpriteRenderer Render(string name,string art,Vector2 p,int order,Transform parent=null){var g=new GameObject(name);g.transform.SetParent(parent?parent:world);g.transform.position=new Vector3(p.x,p.y,0);var s=g.AddComponent<SpriteRenderer>();s.sprite=Art(art);s.sortingOrder=order;return s;}
 void BuildGarden(){
  // Combine 3072 grass tiles into one mesh to keep draw calls low.
  var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
  var atlas=new Texture2D(192,32,TextureFormat.RGBA32,false);atlas.filterMode=FilterMode.Point;atlas.wrapMode=TextureWrapMode.Repeat;
  for(int i=0;i<6;i++){var tex=Art("grass"+i).texture;atlas.SetPixels(i*32,0,32,32,tex.GetPixels());}atlas.Apply();
  for(int x=-32;x<32;x++)for(int y=-24;y<24;y++){
   int n=vertices.Count;vertices.AddRange(new[]{new Vector3(x,y,2),new Vector3(x+1,y,2),new Vector3(x+1,y+1,2),new Vector3(x,y+1,2)});
   int tile=Random.Range(0,6);float a=(tile*32+.1f)/192,b=((tile+1)*32-.1f)/192;
   uv.AddRange(new[]{new Vector2(a,.003f),new Vector2(b,.003f),new Vector2(b,.997f),new Vector2(a,.997f)});triangles.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});
  }
  var ground=new GameObject("Césped - malla combinada");ground.transform.SetParent(world);var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();ground.AddComponent<MeshFilter>().mesh=mesh;var mat=new Material(Shader.Find("Sprites/Default"));mat.mainTexture=atlas;ground.AddComponent<MeshRenderer>().sharedMaterial=mat;ground.GetComponent<MeshRenderer>().sortingOrder=-200;
  Rect[] pots={new Rect(-22,-14,4,4),new Rect(14,-12,4,4),new Rect(-20,10,4,4),new Rect(14,10,4,4)};
  foreach(var r in pots){solids.Add(r);var p=Render("Maceta sólida","pot",r.center,Order(r.center.y),decorations);p.transform.localScale=Vector3.one*2.3f;}
  foreach(var r in new[]{new Rect(-10,12,3,3),new Rect(6,-17,3,3)}){solids.Add(r);var rock=Render("Piedra sólida","rock",r.center,Order(r.center.y),decorations);rock.transform.localScale=Vector3.one*3.4f;}
  puddles.Add(new Rect(-16,-4,4,5));puddles.Add(new Rect(11,1,4,4));
  foreach(var r in puddles){var w=Render("Charco - ralentiza 25%","water",r.center,-180,decorations);w.transform.localScale=new Vector3(r.width/2,r.height/2,1);}
  for(int i=0;i<240;i++){var p=new Vector2(Random.Range(-31f,31f),Random.Range(-23f,23f));if(!Blocked(p,.5f) && (p-pos).sqrMagnitude>9){var s=Render("Flora",""+(i%4==0?"flower":"leaf"),p,-170,decorations);s.color=new Color(1,1,1,i%4==0?.85f:.45f);s.transform.rotation=Quaternion.Euler(0,0,Random.Range(-30,30));s.transform.localScale=Vector3.one*Random.Range(.5f,1.2f);}}
  Render("Desagüe de Zer0","drain",new Vector2(0,-12.5f),-150,decorations).transform.localScale=Vector3.one*1.4f;
  for(int x=-32;x<=32;x+=2){Fence(new Vector2(x,-24));Fence(new Vector2(x,24));}for(int y=-22;y<24;y+=2){Fence(new Vector2(-32,y));Fence(new Vector2(32,y));}
 }
 void Fence(Vector2 p){var s=Render("Límite del jardín","rock",p,-20,decorations);s.transform.localScale=new Vector3(2.3f,1.3f,1);s.color=EvolutionData.C("788a6a");}
 static int Order(float y)=>100-Mathf.RoundToInt(y*5);
 void CreatePlayer(){shadow=Render("Sombra Zer0","shadow",pos,-15,actors);player=Render("Zer0","zero0",pos,100,actors);player.transform.localScale=Vector3.one*1.35f;for(int i=0;i<12;i++){parts[i]=Render(EvolutionData.Names[i],EvolutionData.Icons[i],pos,110,actors);parts[i].enabled=false;}}
 bool Blocked(Vector2 p,float pad=.3f){if(p.x<-31+pad||p.x>31-pad||p.y<-23+pad||p.y>23-pad)return true;foreach(var r in solids)if(p.x>r.xMin-pad&&p.x<r.xMax+pad&&p.y>r.yMin-pad&&p.y<r.yMax+pad)return true;return false;}
 Vector2 Move(Vector2 p,Vector2 delta,float pad=.3f){Vector2 q=p;if(!Blocked(q+new Vector2(delta.x,0),pad))q.x+=delta.x;if(!Blocked(q+new Vector2(0,delta.y),pad))q.y+=delta.y;return q;}
 float GroundSpeed(Vector2 p){foreach(var r in puddles)if(r.Contains(p))return .75f;return 1;}
 bool Sight(Vector2 a,Vector2 b){float d=Vector2.Distance(a,b);for(float t=.2f;t<d;t+=.35f)if(Blocked(Vector2.Lerp(a,b,t/d),0))return false;return true;}
 void StartRun(){
  foreach(var e in enemies){e.go.SetActive(false);enemyPool.Enqueue(e);}enemies.Clear();foreach(var d in drops){d.sr.gameObject.SetActive(false);dropPool.Enqueue(d.sr);}drops.Clear();foreach(var f in effects)Destroy(f.sr.gameObject);effects.Clear();foreach(var m in markers)Destroy(m.line.gameObject);markers.Clear();texts.Clear();
  Array.Clear(ranks,0,12);Array.Clear(cooldowns,0,5);ranks[0]=1;hp=maxHp=100;clock=0;xp=0;level=1;kills=0;pending=0;totalDamage=0;spawnCredit=0;invulnerable=1;withoutDamage=0;shellTimer=0;boss=null;bossSpawned=elite1=elite2=won=false;pos=new Vector2(0,-10);facing=Vector2.up;cam.transform.position=new Vector3(0,-10,-10);player.enabled=true;mode=Mode.Intro;introTime=0;PlayMusic(gardenTrack);Banner("EL JARDÍN DE LOS DESCARTES",4);UpdatePlayerArt();
 }
 void Update(){
  if(Input.GetKeyDown(KeyCode.Escape)){if(mode==Mode.Playing)mode=Mode.Pause;else if(mode==Mode.Pause)mode=Mode.Playing;else if(mode==Mode.Options)mode=returnMode;else if(mode==Mode.Intro)mode=Mode.Playing;}
  if(mode==Mode.Intro){introTime=Mathf.Min(12,introTime+Time.unscaledDeltaTime);if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space)){if(introTime>=12)mode=Mode.Playing;else introTime=Mathf.Min(12,(Mathf.Floor(introTime/4)+1)*4);}UpdatePlayerArt();return;}
  if(mode==Mode.Upgrade){if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D))selected=(selected+1)%offers.Length;if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))selected=(selected+offers.Length-1)%offers.Length;if(Time.unscaledTime>upgradeLock){if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space))Choose(selected);for(int i=0;i<offers.Length;i++)if(Input.GetKeyDown(KeyCode.Alpha1+i))Choose(i);}return;}
  if(mode!=Mode.Playing)return;
  float dt=Mathf.Min(Time.deltaTime,.05f);clock+=dt;bannerTime-=dt;sfxLimiter-=dt;invulnerable-=dt;withoutDamage+=dt;shellTimer=Mathf.Max(0,shellTimer-dt);shakeTime-=dt;
  Vector2 input=new Vector2((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0));input=Vector2.ClampMagnitude(input,1);if(input.sqrMagnitude>.01f)facing=input.normalized;
  float speedBonus=Mathf.Min(.5f,(EvolutionData.Value(5,ranks[5])+EvolutionData.Value(6,ranks[6])+EvolutionData.Value(7,ranks[7]))/100);
  pos=Move(pos,input*3*(1+speedBonus)*GroundSpeed(pos)*dt);
  if(withoutDamage>=5)hp=Mathf.Min(maxHp,hp+(1+EvolutionData.Value(8,ranks[8])+EvolutionData.Value(11,ranks[11]))*dt);
  Waves(dt);UpdateEnemies(dt);if(hp<=0){Finish(false);return;}
  Attacks(dt);UpdateEffects(dt);Collect(dt);UpdatePlayerArt();
  if(boss!=null&&boss.dead){Finish(true);return;}if(pending>0)OpenUpgrade();
  Vector2 cp=new Vector2(Mathf.Clamp(pos.x,-32+cam.orthographicSize*cam.aspect,32-cam.orthographicSize*cam.aspect),Mathf.Clamp(pos.y,-24+cam.orthographicSize,24-cam.orthographicSize));Vector3 target=new Vector3(cp.x,cp.y,-10);cam.transform.position=Vector3.Lerp(cam.transform.position,target,1-Mathf.Exp(-dt*9));if(shakeTime>0)cam.transform.position+=(Vector3)(Random.insideUnitCircle*.07f*settings.shake);
  #if UNITY_EDITOR || DEVELOPMENT_BUILD
  if(Input.GetKeyDown(KeyCode.F8)){clock=480;SpawnBoss();}if(Input.GetKeyDown(KeyCode.F9)){xp+=200;LevelCheck();}if(Input.GetKeyDown(KeyCode.F7))hp=maxHp;
  if(Input.GetKeyDown(KeyCode.F6)){for(int i=0;i<12;i++)ranks[i]=5;maxHp=120;hp=120;}
  if(Input.GetKeyDown(KeyCode.F10)&&boss!=null)Damage(boss,100000);
  #endif
 }
 void OnApplicationFocus(bool focus){if(!focus&&mode==Mode.Playing)mode=Mode.Pause;}
 void Waves(float dt){
  if(clock>=480){if(!bossSpawned)SpawnBoss();return;}if(clock<3)return;
  float rate=clock<60?1:clock<120?1.5f:clock<240?2:clock<360?2.5f:3;int limit=clock<60?35:clock<120?50:clock<240?75:clock<360?100:120;
  spawnCredit+=dt*rate;while(spawnCredit>=1){spawnCredit-=1;if(enemies.Count(e=>!e.dead)>=limit)continue;float r=Random.value;int type=clock<60?0:clock<120?(r<.8?0:1):(r<(clock<240?.6:.45)?0:r<.8?1:2);TrySpawn(type);}
  if(clock>=180&&!elite1){elite1=true;TrySpawn(3);Banner("UNA HORMIGA ALFA SE ACERCA",3);}if(clock>=360&&!elite2){elite2=true;TrySpawn(3);Banner("EL JARDÍN SE AGITA",3);}
 }
 void TrySpawn(int type){for(int k=0;k<16;k++){Vector2 p=pos+Random.insideUnitCircle.normalized*Random.Range(13f,18f);var v=cam.WorldToViewportPoint(p);if(!Blocked(p,.6f)&&(v.x<-.08f||v.x>1.08f||v.y<-.08f||v.y>1.08f)){Spawn(type,p);return;}}}
 void SpawnBoss(){if(bossSpawned)return;bossSpawned=true;boss=Spawn(4,new Vector2(0,12));boss.timer=2;boss.phase=-1;Banner("DEPREDADOR ALFA · ARAÑA LOBO",4);Circle(boss.pos,2.2f,2,Red);PlayMusic(bossTrack);Tone(100,.6f,.45f);}
 Enemy Spawn(int type,Vector2 p){
  Enemy e;if(enemyPool.Count>0)e=enemyPool.Dequeue();else{e=new Enemy();e.go=new GameObject("Enemigo");e.go.transform.SetParent(actors);e.sr=e.go.AddComponent<SpriteRenderer>();}
  e.go.SetActive(true);e.go.name=new[]{"Hormiga","Araña","Bicho bolita","Hormiga alfa","Araña Lobo"}[type];e.type=type;e.pos=p;e.dead=false;e.phase=0;e.timer=0;e.cooldown=type==0?1:3;e.flash=0;e.attack=0;
  e.frame=0;e.max=new float[]{20,35,65,180,2400}[type]*(type<3?1+.08f*Mathf.Floor(clock/60):1);e.hp=e.max;e.speed=new float[]{1.45f,1.7f,.9f,1.5f,1.1f}[type];e.damage=new float[]{8,12,15,18,20}[type];e.sr.sprite=Art(type==4?"boss0":type==2?"pill0":type==1?"spider0":"ant0");e.go.transform.position=p;e.go.transform.localScale=Vector3.one*(type==4?1.65f:type==3?1.65f:1.15f);enemies.Add(e);return e;
 }
 void UpdateEnemies(float dt){
  for(int index=enemies.Count-1;index>=0;index--){Enemy e=enemies[index];if(e.dead)continue;e.timer-=dt;e.cooldown-=dt;e.flash-=dt;Vector2 to=pos-e.pos;float distance=to.magnitude;
   if(e.type==4){BossTick(e,dt,to,distance);}else{
    if(e.phase==0){EnemyMove(e,to.normalized,e.speed*dt*GroundSpeed(e.pos));if((e.type==0||e.type==3)?distance<1.2f&&e.cooldown<=0:e.cooldown<=0){e.dir=to.normalized;e.phase=1;e.timer=e.type==0?.3f:e.type==3?.4f:e.type==1?.65f:.85f;Line(e.pos,e.pos+e.dir*(e.type==0?1.3f:3),e.timer,Red);}}
    else if(e.phase==1 && e.timer<=0){e.phase=2;e.timer=e.type==0||e.type==3?.15f:e.type==1?.5f:.75f;Tone(e.type==2?90:220,.08f,.12f);}
    else if(e.phase==2){if(e.type==1||e.type==2)EnemyMove(e,e.dir,dt*(e.type==1?5:4));if(Vector2.Distance(pos,e.pos)<(e.type==0||e.type==3?.9f:.75f))PlayerDamage(e.damage);if(e.timer<=0){e.phase=3;e.timer=e.type==2?1:.65f;}}
    else if(e.phase==3&&e.timer<=0){e.phase=0;e.cooldown=e.type==0?1:e.type==3?1.4f:e.type==1?3:5;}
    if(e.phase==0&&distance<.65f)PlayerDamage(e.damage);
   }
   int frame=Mathf.FloorToInt(clock*(e.phase==0?8:12))%4;e.sr.sprite=Art((e.type==4?"boss":e.type==2?"pill":e.type==1?"spider":"ant")+frame);
   e.go.transform.position=e.pos;e.sr.sortingOrder=Order(e.pos.y);e.sr.color=e.flash>0?Color.white:e.type==3?EvolutionData.C("f0cc8d"):e.phase==1?Color.Lerp(Color.white,Red,.5f+Mathf.Sin(clock*22)*.3f):Color.white;
   e.go.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(to.y,to.x)*Mathf.Rad2Deg-90);
  }
 }
 void EnemyMove(Enemy e,Vector2 dir,float step){
  Vector2 d=dir*step;Vector2 next=Move(e.pos,d,e.type==4?.8f:.28f);
  if((next-e.pos).sqrMagnitude<step*step*.1f){Vector2 perpendicular=new Vector2(-dir.y,dir.x);next=Move(e.pos,perpendicular*step,(e.type==4?.8f:.28f));if((next-e.pos).sqrMagnitude<.00001f)next=Move(e.pos,-perpendicular*step);}
  e.pos=next;
 }
 void BossTick(Enemy e,float dt,Vector2 to,float distance){
  if(e.phase==-1){if(e.timer<=0){e.phase=0;e.timer=2;}return;}
  if(e.hp<=e.max*.5f&&e.frame==0&&(e.phase==0||e.phase==3)){e.frame=1;e.phase=7;e.timer=1;Banner("ARAÑA LOBO · FRENESÍ",2);Circle(e.pos,2.8f,1,Red);return;}
  if(e.phase==7){if(e.timer<=0){e.phase=0;e.timer=1;}return;}
  if(e.cooldown<=0){e.cooldown=12;for(int i=0;i<4;i++){Vector2 p=e.pos+Random.insideUnitCircle.normalized*3;if(!Blocked(p)&&Vector2.Distance(p,pos)>2&&enemies.Count<125)Spawn(0,p);}}
  switch(e.phase){
   case 0:EnemyMove(e,to.normalized,e.speed*dt);if(distance<1.3f)PlayerDamage(20);if(e.timer<=0){e.phase=1;e.timer=e.frame==0?.9f:.75f;e.dir=to.normalized;Line(e.pos,e.pos+e.dir*5,e.timer,Red);}break;
   case 1:if(e.timer<=0){e.phase=2;e.timer=1;Tone(75,.2f,.25f);}break;
   case 2:EnemyMove(e,e.dir,5*dt);if(Vector2.Distance(pos,e.pos)<1.5f)PlayerDamage(25);if(e.timer<=0){if(e.frame==1&&e.attack==0){e.attack=1;e.phase=1;e.dir=(pos-e.pos).normalized;e.timer=.75f;Line(e.pos,e.pos+e.dir*5,e.timer,Red);}else{e.attack=0;e.phase=3;e.timer=1.2f;}}break;
   case 3:if(e.timer<=0){e.phase=4;e.timer=1;e.target=pos;Circle(e.target,e.frame==0?1.8f:2.2f,1,Red);}break;
   case 4:if(e.timer<=0){e.phase=5;e.timer=.45f;e.dir=e.pos;}break;
   case 5:e.pos=Vector2.Lerp(e.dir,e.target,1-e.timer/.45f);if(e.timer<=0){if(Blocked(e.pos,.8f))e.pos=e.dir;Circle(e.pos,e.frame==0?1.8f:2.2f,.25f,Gold);if(Vector2.Distance(pos,e.pos)<(e.frame==0?1.8f:2.2f))PlayerDamage(e.frame==0?25:28);e.phase=6;e.timer=1.5f;shakeTime=.2f;Tone(60,.18f,.3f);}break;
   case 6:if(e.timer<=0){e.phase=0;e.timer=2;}break;
  }
 }
 void Attacks(float dt){
  for(int id=0;id<5;id++){if(ranks[id]==0)continue;cooldowns[id]-=dt;if(cooldowns[id]>0)continue;int r=ranks[id]-1;float reach=EvolutionData.Reach[id][r];
   var targets=new List<Enemy>();foreach(var e in enemies){if(e.dead||e.phase==-1||(e.type==4&&e.phase==7))continue;Vector2 v=e.pos-pos;float d=v.magnitude;bool hit=false;
    if(id==0)hit=d<reach&&Vector2.Dot(v.normalized,facing)>.342f;
    if(id==1)hit=d<reach&&Vector2.Dot(v.normalized,facing)>.5f;
    if(id==2)hit=Mathf.Abs(v.x)<reach*.5f&&Mathf.Abs(v.y)<.65f;
    if(id==3)hit=v.y<.2f&&v.y>-reach&&Mathf.Abs(v.x)<.6f;
    if(id==4)hit=v.y>.25f&&v.y<1.7f&&Mathf.Abs(v.x)<reach*.5f;
    if(hit&&Sight(pos,e.pos))targets.Add(e);
   }
   if(id==1&&targets.Count>0){targets.Sort((a,b)=>(a.pos-pos).sqrMagnitude.CompareTo((b.pos-pos).sqrMagnitude));targets.RemoveRange(1,targets.Count-1);}
   // A prepared automatic attack waits for a target instead of wasting its cooldown.
   if(targets.Count==0)continue;cooldowns[id]=EvolutionData.Interval[id][r];float damage=EvolutionData.Damage[id][r]*(1+EvolutionData.Value(10,ranks[10])/100);
   Color col=EvolutionData.Colors[id];if(id==0)Arc(pos,facing,reach,.22f,col);else if(id==1)Line(pos,targets[0].pos,.18f,col);else if(id==2)Line(pos+Vector2.left*reach*.5f,pos+Vector2.right*reach*.5f,.2f,col);else if(id==3)Line(pos,pos+Vector2.down*reach,.18f,col);else Line(pos+new Vector2(-reach*.5f,1),pos+new Vector2(reach*.5f,1),.2f,col);
   foreach(var e in targets){Damage(e,damage);if(e.type!=4)e.pos=Move(e.pos,(e.pos-pos).normalized*(id==2?.25f:.1f));}if(sfxLimiter<=0){Tone(180+id*90,.08f,.18f);sfxLimiter=.07f;}
  }
 }
 void Damage(Enemy e,float amount){if(e.dead)return;totalDamage+=Mathf.Min(e.hp,amount);e.hp-=amount;e.flash=settings.reducedFlash?0:.07f;if(settings.numbers)texts.Add(new FloatText{pos=e.pos+Vector2.up*.5f,text=Mathf.CeilToInt(amount).ToString(),color=Gold,time=.6f});Burst(e.pos,EvolutionData.C("c2b88b"),4);if(e.hp>0)return;e.dead=true;kills++;e.go.SetActive(false);if(e.type!=4)DropXp(e.pos,new int[]{2,3,5,20,0}[e.type]);Burst(e.pos,e.type==4?Gold:EvolutionData.C("b7c48b"),12);}
 void PlayerDamage(float damage){if(invulnerable>0||mode!=Mode.Playing)return;invulnerable=.65f;if(ranks[9]>0&&shellTimer<=0){shellTimer=3;Circle(pos,1,.25f,EvolutionData.Colors[9]);Tone(650,.15f,.25f);return;}hp=Mathf.Max(0,hp-damage);withoutDamage=0;shakeTime=.2f;Burst(pos,Red,8);Tone(85,.18f,.35f);if(settings.numbers)texts.Add(new FloatText{pos=pos+Vector2.up,text="-"+damage.ToString("0"),color=Red,time=.8f});}
 void DropXp(Vector2 p,int value){SpriteRenderer sr=dropPool.Count>0?dropPool.Dequeue():Render("ADN","xp",p,-30,actors);sr.gameObject.SetActive(true);sr.transform.localScale=Vector3.one*(value>=20?.65f:.38f);sr.transform.position=p;drops.Add(new Drop{sr=sr,pos=p,value=value});}
 void Collect(float dt){
  for(int i=drops.Count-1;i>=0;i--){var d=drops[i];float dist=Vector2.Distance(pos,d.pos);if(dist<1.5f)d.pos=Vector2.MoveTowards(d.pos,pos,dt*(4+5*(1.5f-dist)));d.sr.transform.position=d.pos;d.sr.transform.rotation=Quaternion.Euler(0,0,Mathf.Sin(clock*3+i)*10);if(dist<.4f){xp+=d.value;d.sr.gameObject.SetActive(false);dropPool.Enqueue(d.sr);drops.RemoveAt(i);if(sfxLimiter<=0){Tone(660+level*22,.07f,.08f);sfxLimiter=.03f;}}}LevelCheck();
 }
 void LevelCheck(){while(xp>=8+4*(level-1)){xp-=8+4*(level-1);level++;pending++;}}
 void OpenUpgrade(){
  var valid=new List<int>();int attackCount=0,passiveCount=0;for(int i=0;i<12;i++)if(ranks[i]>0){if(i<5)attackCount++;else passiveCount++;}
  for(int i=0;i<12;i++)if(ranks[i]<5&&(ranks[i]>0||(i<5?attackCount<4:passiveCount<4)))valid.Add(i);
  var pick=new List<int>();var attacks=valid.Where(i=>i<5).ToList();if(attacks.Count>0){int a=attacks[Random.Range(0,attacks.Count)];pick.Add(a);valid.Remove(a);}while(pick.Count<3&&valid.Count>0){int j=Random.Range(0,valid.Count);pick.Add(valid[j]);valid.RemoveAt(j);}if(pick.Count==0){pick.Add(12);pick.Add(13);}offers=pick.ToArray();selected=0;upgradeLock=Time.unscaledTime+.25f;mode=Mode.Upgrade;Tone(880,.3f,.3f);
 }
 void Choose(int index){if(mode!=Mode.Upgrade||Time.unscaledTime<upgradeLock||index>=offers.Length)return;int id=offers[index];if(id==12)hp=Mathf.Min(maxHp,hp+20);else if(id==13){maxHp+=5;hp=Mathf.Min(maxHp,hp+5);}else{float old=EvolutionData.Value(9,ranks[9]);ranks[id]++;if(id==9){float add=EvolutionData.Value(9,ranks[9])-old;maxHp+=add;hp+=add;}}pending--;Burst(pos,Mint,14);Tone(1100,.2f,.22f);if(pending>0)OpenUpgrade();else{mode=Mode.Playing;UpdatePlayerArt();}}
 void UpdatePlayerArt(){
  player.transform.position=pos;player.sortingOrder=Order(pos.y)+3;player.sprite=Art("zero"+(Mathf.FloorToInt(clock*9)%6));player.flipX=facing.x<-.1f;player.color=invulnerable>0&&!settings.reducedFlash?new Color(1,1,1,.55f+Mathf.PingPong(clock*6,.45f)):Color.white;
  shadow.transform.position=pos+Vector2.down*.35f;shadow.sortingOrder=Order(pos.y)-6;shadow.transform.localScale=Vector3.one*1.3f;
  Vector2[] offsets={new Vector2(.6f,.1f),new Vector2(.35f,-.6f),new Vector2(-.65f,.05f),new Vector2(0,-.7f),new Vector2(0,.55f),new Vector2(0,.15f),new Vector2(0,-.1f),new Vector2(0,-.35f),new Vector2(0,-.25f),new Vector2(0,-.1f),new Vector2(0,.58f),new Vector2(0,.55f)};
  for(int i=0;i<12;i++){var part=parts[i];part.enabled=ranks[i]>0;if(!part.enabled)continue;part.transform.position=pos+offsets[i];float size=.65f+.065f*ranks[i];if(i==5)size=1.75f+Mathf.Sin(clock*22)*.15f;if(i==6||i==7)size=1.45f+Mathf.Sin(clock*13)*.04f;if(i==8||i==9)size=.85f;part.transform.localScale=new Vector3(size,size,1);part.sortingOrder=Order(pos.y)+(i==5||i==6||i==7?-2:5+i);part.color=i==9?(shellTimer>0?new Color(1,1,1,.15f):Color.white):Color.white;part.transform.rotation=Quaternion.Euler(0,0,i<5?Mathf.Sin(clock*5+i)*8:0);}
 }
 void Burst(Vector2 p,Color color,int count){for(int i=0;i<count&&effects.Count<180;i++){SpriteRenderer s=fxPool.Count>0?fxPool.Dequeue():Render("Partícula","xp",p,300,actors);s.gameObject.SetActive(true);s.transform.position=p;s.transform.localScale=Vector3.one*Random.Range(.07f,.16f);effects.Add(new Fx{sr=s,vel=Random.insideUnitCircle*3,time=.35f,total=.35f,color=color});}}
 void UpdateEffects(float dt){for(int i=effects.Count-1;i>=0;i--){var f=effects[i];f.time-=dt;if(f.time<=0){f.sr.gameObject.SetActive(false);fxPool.Enqueue(f.sr);effects.RemoveAt(i);continue;}f.sr.transform.position+=(Vector3)(f.vel*dt);f.vel*=.9f;f.sr.color=new Color(f.color.r,f.color.g,f.color.b,f.time/f.total);}for(int i=texts.Count-1;i>=0;i--){texts[i].time-=dt;texts[i].pos+=Vector2.up*dt*.9f;if(texts[i].time<=0)texts.RemoveAt(i);}for(int i=markers.Count-1;i>=0;i--){markers[i].time-=dt;if(markers[i].time<=0){Destroy(markers[i].line.gameObject);markers.RemoveAt(i);}}
  // Recycle dead enemies after the update pass, never while iterating attack targets.
  for(int i=enemies.Count-1;i>=0;i--)if(enemies[i].dead&&enemies[i]!=boss){enemyPool.Enqueue(enemies[i]);enemies.RemoveAt(i);}
 }
 LineRenderer Stroke(Vector2[] points,float duration,Color color,float width=.07f){var g=new GameObject("Señal de combate");g.transform.SetParent(actors);var lr=g.AddComponent<LineRenderer>();lr.sharedMaterial=lineMaterial;lr.startColor=lr.endColor=color;lr.startWidth=lr.endWidth=width;lr.positionCount=points.Length;lr.sortingOrder=290;lr.useWorldSpace=true;for(int i=0;i<points.Length;i++)lr.SetPosition(i,new Vector3(points[i].x,points[i].y,-.1f));markers.Add(new Marker{line=lr,time=duration});return lr;}
 void Line(Vector2 a,Vector2 b,float duration,Color c){Stroke(new[]{a,b},duration,c,.1f);Vector2 side=new Vector2(-(b-a).normalized.y,(b-a).normalized.x)*.2f;Stroke(new[]{b-(b-a).normalized*.4f+side,b,b-(b-a).normalized*.4f-side},duration,c,.07f);}
 void Circle(Vector2 p,float r,float duration,Color c){var pts=new Vector2[49];for(int i=0;i<49;i++){float a=i/48f*Mathf.PI*2;pts[i]=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;}Stroke(pts,duration,c);Stroke(new[]{p+new Vector2(-.2f,0),p+new Vector2(.2f,0)},duration,c);}
 void Arc(Vector2 p,Vector2 dir,float radius,float duration,Color c){float angle=Mathf.Atan2(dir.y,dir.x);var pts=new Vector2[22];pts[0]=p;for(int i=1;i<21;i++){float a=angle+Mathf.Lerp(-70,70,(i-1)/19f)*Mathf.Deg2Rad;pts[i]=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;}pts[21]=p;Stroke(pts,duration,c,.1f);}
 void Banner(string text,float seconds){banner=text;bannerTime=seconds;}
 void PlayMusic(AudioClip clip){if(music==null||!clip)return;music.clip=clip;music.Play();}
 void Tone(float freq,float duration,float volume){if(!sfx)return;int rate=22050,n=Mathf.CeilToInt(duration*rate);var data=new float[n];for(int i=0;i<n;i++){float t=i/(float)rate;data[i]=Mathf.Sin(t*freq*2*Mathf.PI*(1-.3f*t/duration))*Mathf.Sin(Mathf.PI*i/n)*volume;}var clip=AudioClip.Create("Síntesis de evento",n,1,rate,false);clip.SetData(data,0);sfx.PlayOneShot(clip,settings.effects);Destroy(clip,duration+1);}
 void Finish(bool victory){if(mode==Mode.Result)return;won=victory;mode=Mode.Result;int score=kills*10+level*100+(won?2000:0);settings.best=Mathf.Max(settings.best,score);Save();Tone(won?880:90,.6f,.35f);}
 void Load(){try{if(File.Exists(savePath))settings=JsonUtility.FromJson<Settings>(File.ReadAllText(savePath))??new Settings();}catch{settings=new Settings();}}
 void Save(){try{string temp=savePath+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(settings,true));if(File.Exists(savePath))File.Copy(savePath,savePath+".bak",true);File.Copy(temp,savePath,true);}catch(Exception e){Debug.LogWarning("No se pudo guardar opciones: "+e.Message);}}
 void OnDestroy(){Time.timeScale=1;if(lineMaterial)Destroy(lineMaterial);}
 #if UNITY_EDITOR
 public void StartPreviews(){StartCoroutine(PreviewSequence());}
 System.Collections.IEnumerator PreviewSequence(){
  Directory.CreateDirectory("Validation/Previews");
  mode=Mode.Menu;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Validation/Previews/01-menu.png");yield return null;
  StartRun();introTime=12;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Validation/Previews/02-cinematica.png");yield return null;
  mode=Mode.Playing;ranks[0]=3;ranks[2]=2;ranks[5]=2;ranks[9]=1;clock=45;hp=100;
  for(int i=0;i<20;i++){Vector2 p=pos+new Vector2(Mathf.Cos(i*.315f),Mathf.Sin(i*.315f))*UnityEngine.Random.Range(3f,7f);if(!Blocked(p))Spawn(i%3,p);}
  yield return new WaitForSeconds(3);pending=0;mode=Mode.Playing;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Validation/Previews/03-combate.png");yield return null;
  xp=40;LevelCheck();OpenUpgrade();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Validation/Previews/04-mejoras.png");yield return null;
  pending=0;mode=Mode.Playing;pos=new Vector2(0,8);cam.transform.position=new Vector3(0,9,-10);SpawnBoss();yield return new WaitForSeconds(2.3f);mode=Mode.Pause;yield return new WaitForEndOfFrame();mode=Mode.Playing;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Validation/Previews/05-jefe.png");yield return null;
  Finish(true);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("Validation/Previews/06-resultados.png");yield return null;
  StartRun();mode=Mode.Menu;PlayMusic(menuTrack);File.WriteAllText("Validation/preview-result.txt","Six rendered previews complete. Staged scenarios for UI verification.");
 }
 public void RunSmokeTest(){
  var results=new List<string>();Action<bool,string> check=(ok,name)=>{results.Add((ok?"PASS ":"FAIL ")+name);if(!ok)Debug.LogError("Smoke: "+name);};
  try{
   StartRun();mode=Mode.Playing;invulnerable=0;
   check(ranks[0]==1&&ranks.Skip(1).All(r=>r==0),"Empieza con tenaza; sin habilidades adicionales");
   Vector2 old=pos;Vector2 wall=new Vector2(-21,-12);check(Blocked(wall),"Macetas bloquean movimiento");
   var e=Spawn(0,pos+Vector2.up);Attacks(.01f);check(Mathf.Abs(e.hp-2)<.01f,"Tenaza rango 1: 18 daño, HP="+e.hp+", rango="+ranks[0]+", distancia="+Vector2.Distance(pos,e.pos)+", sight="+Sight(pos,e.pos));Attacks(1.3f);check(e.dead&&kills==1&&drops.Count==1,"Muerte y ADN una sola vez");Damage(e,100);check(kills==1,"Sin recompensa duplicada");
   float life=hp;PlayerDamage(8);check(hp==life-8,"Daño recibido");PlayerDamage(8);check(hp==life-8,"Invulnerabilidad evita daño duplicado");
   invulnerable=0;ranks[9]=1;shellTimer=0;PlayerDamage(100);check(hp==life-8&&shellTimer==3,"Caparazón bloquea y recarga en 3 s");
   xp=40;LevelCheck();check(level==4&&pending==3&&xp==4,"ADN conserva excedente y encola tres mejoras");OpenUpgrade();check(offers.Distinct().Count()==offers.Length,"Ofertas sin duplicados");upgradeLock=0;Choose(0);check(pending==2,"Una elección consume una subida");
   for(int id=0;id<12;id++){ranks[id]=5;check(!string.IsNullOrEmpty(EvolutionData.Describe(id,5)),"Descripción rango 5: "+EvolutionData.Names[id]);}UpdatePlayerArt();check(parts.All(p=>p.enabled),"Todas las piezas visuales disponibles");
   mode=Mode.Playing;SpawnBoss();check(boss!=null&&boss.hp==2400,"Jefe aparece una vez con 2400 vida");var b=boss;SpawnBoss();check(b==boss,"Sin jefe duplicado");boss.phase=0;boss.timer=0;boss.hp=1100;BossTick(boss,.01f,Vector2.up,5);check(boss.frame==1&&boss.phase==7,"Cambio de fase del jefe");
   hp=100;Damage(boss,9999);Finish(true);check(won&&mode==Mode.Result,"Victoria y resultados");StartRun();mode=Mode.Playing;hp=0;Finish(false);check(!won&&mode==Mode.Result,"Derrota y resultados");StartRun();check(kills==0&&xp==0&&level==1&&boss==null,"Reintento limpia estado");
   // Each anatomical attack is checked at all five ranks, with its intended direction.
   for(int id=0;id<5;id++)for(int rank=1;rank<=5;rank++){
    StartRun();mode=Mode.Playing;Array.Clear(ranks,0,12);ranks[id]=rank;facing=Vector2.up;
    Vector2 offset=id==2?Vector2.right:id==3?Vector2.down:Vector2.up;
    var target=Spawn(3,pos+offset);target.hp=target.max=1000;Attacks(.01f);
    check(Mathf.Abs((1000-target.hp)-EvolutionData.Damage[id][rank-1])<.01f,$"Daño {EvolutionData.Names[id]} rango {rank}");
   }
   StartRun();mode=Mode.Playing;clock=479;Waves(1);check(!bossSpawned,"Jefe no aparece antes del minuto 8");clock=480;Waves(.01f);check(bossSpawned,"Director activa jefe al minuto 8");
   mode=Mode.Menu;PlayMusic(menuTrack);
  }catch(Exception ex){results.Add("FAIL EXCEPTION "+ex);Debug.LogException(ex);}
  Directory.CreateDirectory("Validation");File.WriteAllLines("Validation/smoke-results.txt",results);Debug.Log("Arthropod smoke: "+results.Count+" checks, "+results.Count(x=>x.StartsWith("FAIL"))+" failures");
 }
 #endif

 // Consistent 1280x720 UI coordinates, scaled to every window size.
 void InitGui(){if(guiReady)return;guiReady=true;font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");white=Texture2D.whiteTexture;
  label=new GUIStyle{font=font,fontSize=18,normal={textColor=Mint},alignment=TextAnchor.MiddleLeft};small=new GUIStyle(label){fontSize=13,normal={textColor=Muted}};title=new GUIStyle(label){fontSize=46,fontStyle=FontStyle.Bold};body=new GUIStyle(label){fontSize=17,wordWrap=true,alignment=TextAnchor.UpperLeft};button=new GUIStyle(label){fontSize=18,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
 }
 void Box(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,white);GUI.color=Color.white;}
 void Text(Rect r,string t,int size=18,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){var st=new GUIStyle(label){fontSize=size,alignment=align,wordWrap=true};st.normal.textColor=color??Mint;GUI.Label(r,t,st);}
 void Border(Rect r,Color c,float t=1){Box(new Rect(r.x,r.y,r.width,t),c);Box(new Rect(r.x,r.yMax-t,r.width,t),c);Box(new Rect(r.x,r.y,t,r.height),c);Box(new Rect(r.xMax-t,r.y,t,r.height),c);}
 bool Btn(Rect r,string t,bool primary=false){bool hover=r.Contains(Event.current.mousePosition);Box(r,primary?(hover?EvolutionData.C("c9e5b7"):Mint):(hover?EvolutionData.C("2c4943"):Panel));Border(r,primary?Mint:EvolutionData.C("456055"));Text(r,t,18,primary?Bg:Mint,TextAnchor.MiddleCenter);bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);if(clicked)Tone(500,.06f,.12f);return clicked;}
 void Icon(Rect r,int i){var texture=Art(EvolutionData.Icons[i]).texture;GUI.DrawTexture(r,texture,ScaleMode.ScaleToFit,true);}
 void Bar(Rect r,float value,Color color){Box(r,EvolutionData.C("132321"));Box(new Rect(r.x+2,r.y+2,(r.width-4)*Mathf.Clamp01(value),r.height-4),color);Border(r,EvolutionData.C("536b55"));}
 Vector2 ScreenPoint(Vector2 p){Vector3 v=cam.WorldToViewportPoint(p);return new Vector2(v.x*1280,(1-v.y)*720);}
 void OnGUI(){InitGui();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
  if(mode==Mode.Menu){Menu();return;}if(mode==Mode.Credits){Credits();return;}
  Hud();if(mode==Mode.Intro){Intro();return;}if(mode==Mode.Upgrade){Upgrade();return;}if(mode==Mode.Pause){Pause();return;}if(mode==Mode.Options){Options();return;}if(mode==Mode.Result){Results();return;}
 }
 void Menu(){
  Box(new Rect(0,0,1280,720),new Color(.025f,.07f,.065f,.88f));Box(new Rect(0,0,690,720),new Color(.04f,.095f,.09f,.96f));Box(new Rect(64,72,36,3),Gold);
  Text(new Rect(64,91,540,30),"B O V A T I T O S   /   DEMOSTRACIÓN",13,Gold);Text(new Rect(60,137,640,76),"ARTHROPOD",58,Mint);Text(new Rect(62,210,580,65),"EVOLUTION",51,Mint);Text(new Rect(65,300,510,68),"Del desagüe a la cima\nde la cadena alimenticia.",24,Muted);
  if(Btn(new Rect(65,410,420,56),"ENTRAR AL JARDÍN     →",true))StartRun();
  if(Btn(new Rect(65,480,204,44),"OPCIONES")){returnMode=Mode.Menu;mode=Mode.Options;}
  if(Btn(new Rect(281,480,204,44),"CRÉDITOS"))mode=Mode.Credits;
  GUI.enabled=false;Btn(new Rect(65,540,204,36),"NIVELES (BLOQ.)");Btn(new Rect(281,540,204,36),"GALERÍA (BLOQ.)");GUI.enabled=true;
  if(Btn(new Rect(65,588,420,34),"SALIR"))Application.Quit();
  Text(new Rect(65,638,560,28),"01  EL JARDÍN DE LOS DESCARTES",14,Gold);Text(new Rect(65,669,580,20),"WASD / FLECHAS  ·  ATAQUES AUTOMÁTICOS  ·  ESC PAUSA",12,Muted);
  // Museum-like specimen card ties UI to the game's biological identity.
  var card=new Rect(766,100,420,500);Box(card,Panel);Border(card,EvolutionData.C("536c58"));Text(new Rect(792,119,370,25),"ESPÉCIMEN  000   /   ADN INESTABLE",13,Gold);
  GUI.DrawTexture(new Rect(827,185+Mathf.Sin(Time.realtimeSinceStartup*1.5f)*5,292,270),Art("zero"+(int)(Time.realtimeSinceStartup*5)%6).texture,ScaleMode.ScaleToFit,true);
  Text(new Rect(796,465,355,38),"Zer0",34,Mint);Text(new Rect(796,514,350,56),"Absorbé ADN. Elegí tu anatomía.\nSobreviví al depredador alfa.",17,Muted);Text(new Rect(792,623,400,24),"RÉCORD LOCAL   "+settings.best.ToString("N0"),14,Gold);
 }
 void Hud(){
  Box(new Rect(0,0,1280,92),new Color(.04f,.09f,.085f,.94f));Box(new Rect(0,90,1280,1),EvolutionData.C("48604e"));
  Text(new Rect(24,9,350,23),"MUTACIONES   /   NIVEL "+level,12,Muted);
  int col=0;for(int i=0;i<12;i++)if(ranks[i]>0){Rect r=new Rect(24+col*48,37,40,40);Box(r,Panel);Border(r,EvolutionData.Colors[i]*.65f);Icon(new Rect(r.x+3,r.y+2,32,32),i);Text(new Rect(r.x+24,r.y+23,16,16),ranks[i].ToString(),11,Gold,TextAnchor.MiddleCenter);col++;}
  Text(new Rect(535,9,210,44),$"{(int)clock/60:00}:{(int)clock%60:00}",31,Mint,TextAnchor.MiddleCenter);Text(new Rect(535,53,210,22),clock<480?"SOBREVIVÍ HASTA 08:00":"DERROTÁ A LA ARAÑA LOBO",11,Gold,TextAnchor.MiddleCenter);
  Text(new Rect(1060,15,190,28),kills+"  BAJAS",20,Gold,TextAnchor.MiddleRight);Text(new Rect(1040,49,210,22),"ESC   PAUSA",12,Muted,TextAnchor.MiddleRight);
  Vector2 pp=ScreenPoint(pos);Bar(new Rect(pp.x-31,pp.y-43,62,8),hp/maxHp,hp/maxHp<=.15f?Red:Mint);if(hp/maxHp<=.15f)Text(new Rect(pp.x-50,pp.y-68,100,20),"¡VIDA CRÍTICA!",12,Red,TextAnchor.MiddleCenter);
  if(ranks[9]>0)Text(new Rect(pp.x-50,pp.y+35,100,20),shellTimer>0?$"ESCUDO {shellTimer:0.0}s":"ESCUDO LISTO",10,EvolutionData.Colors[9],TextAnchor.MiddleCenter);
  foreach(var t in texts){Vector2 p=ScreenPoint(t.pos);Text(new Rect(p.x-25,p.y-12,65,25),t.text,17,t.color,TextAnchor.MiddleCenter);}
  Box(new Rect(0,676,1280,44),new Color(.04f,.09f,.085f,.95f));Bar(new Rect(25,688,960,14),xp/(float)(8+4*(level-1)),EvolutionData.C("74c9b5"));Text(new Rect(1003,680,255,32),$"ADN  {xp} / {8+4*(level-1)}",14,Mint,TextAnchor.MiddleRight);
  if(boss!=null&&!boss.dead){Text(new Rect(390,98,500,22),"ARAÑA LOBO  /  "+(boss.frame==0?"DEPREDADOR ALFA":"FRENESÍ"),12,Red,TextAnchor.MiddleCenter);Bar(new Rect(390,124,500,10),boss.hp/boss.max,Red);Vector2 b=ScreenPoint(boss.pos);if(b.x<35||b.x>1245||b.y<150||b.y>640){Vector2 edge=new Vector2(Mathf.Clamp(b.x,50,1230),Mathf.Clamp(b.y,158,630));Text(new Rect(edge.x-60,edge.y-18,120,36),"▲  ARAÑA LOBO",13,Red,TextAnchor.MiddleCenter);}}
  if(bannerTime>0&&mode==Mode.Playing){Box(new Rect(345,154,590,48),new Color(.04f,.09f,.085f,.9f));Text(new Rect(350,154,580,48),banner,18,Gold,TextAnchor.MiddleCenter);}
  if(clock<18&&mode==Mode.Playing)Text(new Rect(370,600,540,40),"WASD para moverte · acercate al ADN para absorberlo",16,Mint,TextAnchor.MiddleCenter);
 }
 void Overlay(){Box(new Rect(0,0,1280,720),new Color(.025f,.06f,.055f,.9f));}
 void Intro(){
  Overlay();Text(new Rect(96,40,1088,35),"PRÓLOGO  /  EL COLECCIONISTA",14,Gold);Text(new Rect(96,80,1088,48),"Un experimento fallido. Un ADN imposible.",30,Mint);
  var tex=Resources.Load<Texture2D>("Art/cinematic");if(tex){tex.filterMode=FilterMode.Bilinear;GUI.DrawTexture(new Rect(96,150,1088,405),tex,ScaleMode.StretchToFill,true);}
  int step=Mathf.Min(2,(int)(introTime/4));for(int i=0;i<3;i++){if(i!=step&&introTime<12)Box(new Rect(96+i*1088/3f,150,1088/3f,405),new Color(.025f,.05f,.04f,.76f));if(i==step&&introTime<12)Border(new Rect(96+i*1088/3f,150,1088/3f,405),Gold,2);}
  string[] captions={"El Coleccionista: «Inestable. Imperfecto. Otro espécimen que no sirve». «Zer0», anota en su registro.","Arrojado al desagüe, Zer0 desaparece entre barro y raíces. El científico vuelve a sus experimentos.","Pero Zer0 sigue vivo. En el jardín, absorber ADN será la diferencia entre devorar... y ser devorado."};
  Text(new Rect(96,570,1088,55),introTime>=12?"WASD / flechas: moverte. Ataques automáticos. Absorbé ADN y elegí mutaciones para sobrevivir hasta la Araña Lobo.":captions[step],20,Mint,TextAnchor.MiddleCenter);
  if(Btn(new Rect(775,644,409,44),introTime>=12?"SOBREVIVIR   /   ENTER":"SIGUIENTE VIÑETA   /   ENTER",true)){if(introTime>=12)mode=Mode.Playing;else introTime=Mathf.Min(12,(Mathf.Floor(introTime/4)+1)*4);}
  if(Btn(new Rect(96,644,210,44),"SALTAR CINEMÁTICA"))mode=Mode.Playing;
 }
 void Upgrade(){Overlay();Text(new Rect(70,45,1140,28),"EVOLUCIÓN  /  NIVEL "+level,14,Gold,TextAnchor.MiddleCenter);Text(new Rect(70,87,1140,58),"Tu ADN quiere cambiar.",38,Mint,TextAnchor.MiddleCenter);Text(new Rect(70,148,1140,28),"Elegí una mutación · la partida está en pausa",17,Muted,TextAnchor.MiddleCenter);
  float w=330,gap=26,total=offers.Length*w+(offers.Length-1)*gap,start=(1280-total)/2;
  for(int i=0;i<offers.Length;i++){int id=offers[i];var r=new Rect(start+i*(w+gap),209,w,365);Box(r,Panel);Border(r,i==selected?Gold:EvolutionData.C("3b584c"),i==selected?2:1);if(id<12){Icon(new Rect(r.x+22,r.y+20,66,66),id);Text(new Rect(r.x+105,r.y+20,205,30),ranks[id]==0?"NUEVA MUTACIÓN":$"RANGO {ranks[id]}  →  {ranks[id]+1}",12,Gold);Text(new Rect(r.x+105,r.y+51,205,24),EvolutionData.Tags[id],10,Muted);Text(new Rect(r.x+22,r.y+104,286,57),EvolutionData.Names[id],23,EvolutionData.Colors[id]);Text(new Rect(r.x+22,r.y+174,286,130),EvolutionData.Describe(id,ranks[id]+1),16,Mint,TextAnchor.UpperLeft);Text(new Rect(r.x+22,r.y+309,286,44),EvolutionData.Change(id,ranks[id]),13,Gold);}else{Text(new Rect(r.x+22,r.y+60,286,80),id==12?"Recuperar 20 de vida":"+5 de vida máxima",25,Mint);}
   if(Btn(new Rect(r.x+16,r.y+382,w-32,49),"ELEGIR   ["+(i+1)+"]",i==selected)&&Time.unscaledTime>upgradeLock)Choose(i);
  }
  Text(new Rect(100,661,1080,25),"← → seleccionar   ·   ENTER confirmar   ·   4 ataques y 4 pasivas como máximo",13,Muted,TextAnchor.MiddleCenter);
 }
 void Pause(){Overlay();Text(new Rect(420,161,440,60),"En pausa",42,Mint,TextAnchor.MiddleCenter);if(Btn(new Rect(440,271,400,52),"CONTINUAR",true))mode=Mode.Playing;if(Btn(new Rect(440,340,400,48),"OPCIONES")){returnMode=Mode.Pause;mode=Mode.Options;}if(Btn(new Rect(440,405,400,48),"REINICIAR DEMOSTRACIÓN"))StartRun();if(Btn(new Rect(440,470,400,48),"VOLVER AL MENÚ")){mode=Mode.Menu;PlayMusic(menuTrack);}Text(new Rect(340,570,600,40),"El tiempo, las oleadas y el combate están detenidos.",16,Muted,TextAnchor.MiddleCenter);}
 float Slider(float y,string name,float v){Text(new Rect(300,y,300,35),name,18,Mint);v=GUI.HorizontalSlider(new Rect(640,y+12,250,25),v,0,1);Text(new Rect(920,y,60,35),Mathf.RoundToInt(v*100)+"%",16,Gold);return v;}
 void Options(){Overlay();Text(new Rect(300,65,680,65),"Opciones",42,Mint);Text(new Rect(300,135,680,25),"SONIDO Y LEGIBILIDAD",13,Gold);settings.music=Slider(190,"Música",settings.music);settings.effects=Slider(247,"Efectos",settings.effects);settings.shake=Slider(304,"Vibración de cámara",settings.shake);music.volume=settings.music*.42f;
  if(Btn(new Rect(300,370,680,43),"NÚMEROS DE DAÑO     "+(settings.numbers?"ACTIVADOS":"DESACTIVADOS")))settings.numbers=!settings.numbers;
  if(Btn(new Rect(300,425,680,43),"DESTELLOS REDUCIDOS     "+(settings.reducedFlash?"SÍ":"NO")))settings.reducedFlash=!settings.reducedFlash;
  if(Btn(new Rect(300,480,330,43),Screen.fullScreen?"PANTALLA: COMPLETA":"PANTALLA: VENTANA")){Screen.fullScreen=!Screen.fullScreen;settings.fullScreen=Screen.fullScreen;}
  if(Btn(new Rect(650,480,330,43),"LÍMITE   "+settings.cap+" FPS")){settings.cap=settings.cap==30?60:settings.cap==60?120:30;QualitySettings.vSyncCount=0;Application.targetFrameRate=settings.cap;}
  if(Btn(new Rect(300,576,680,52),"GUARDAR Y VOLVER",true)){Save();mode=returnMode;}
 }
 void Results(){Overlay();Text(new Rect(180,69,920,28),won?"JARDÍN SUPERADO  /  FIN DE LA DEMOSTRACIÓN":"EL JARDÍN RECLAMA A SU PRESA",14,Gold,TextAnchor.MiddleCenter);Text(new Rect(180,112,920,67),won?"La evolución continúa.":"Todavía podés evolucionar.",40,Mint,TextAnchor.MiddleCenter);
  Text(new Rect(205,220,300,40),"TIEMPO",13,Muted);Text(new Rect(205,260,300,50),$"{(int)clock/60:00}:{(int)clock%60:00}",36,Gold);Text(new Rect(520,220,250,40),"BAJAS",13,Muted);Text(new Rect(520,260,250,50),kills.ToString(),36,Gold);Text(new Rect(850,220,240,40),"PUNTUACIÓN",13,Muted);Text(new Rect(850,260,240,50),(kills*10+level*100+(won?2000:0)).ToString("N0"),36,Gold);
  Border(new Rect(205,342,870,1),Muted);Text(new Rect(205,361,870,30),"ANATOMÍA FINAL   /   NIVEL "+level+"   /   DAÑO TOTAL "+totalDamage.ToString("N0"),14,Muted);int c=0;for(int i=0;i<12;i++)if(ranks[i]>0){Icon(new Rect(205+c*105,412,58,58),i);Text(new Rect(205+c*105,479,90,24),"RANGO "+ranks[i],12,EvolutionData.Colors[i]);c++;}
  if(Btn(new Rect(300,570,330,53),"VOLVER A EVOLUCIONAR",true))StartRun();if(Btn(new Rect(650,570,330,53),"MENÚ")){mode=Mode.Menu;PlayMusic(menuTrack);}
 }
 void Credits(){Overlay();Text(new Rect(220,80,840,65),"Bovatitos",46,Mint);Text(new Rect(220,165,840,260),"ARTHROPOD EVOLUTION\n\nSantino Bovati · Lead Programmer\nBrandon Emmanuel Li · Game Designer\nVictor Ome Joven · Level Designer\nMauro Arcadigni · Game Artist\n\nDemo Unity: arte pixelado, música y efectos originales generados para este prototipo. Diseño basado en el GDD del equipo. Inspiración de género: Vampire Survivors.",21,Muted,TextAnchor.UpperLeft);Text(new Rect(220,470,840,70),"Campaña completa: Jardín → Tronco → Estanque → Colmena → Terrario. Esta demostración incluye únicamente el Jardín.",18,Gold);if(Btn(new Rect(220,584,840,50),"VOLVER",true))mode=Mode.Menu;}
}
}



