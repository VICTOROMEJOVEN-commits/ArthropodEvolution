using UnityEngine;

namespace Arthropod {
public static class EvolutionData {
 public static readonly string[] Names={"Tenaza de escorpión","Cola de escorpión","Pinza de cangrejo","Aguijón de abeja","Patas de mantis","Alas de libélula","Patas de araña","Patas de ciempiés","Exoesqueleto regenerativo","Caparazón de escarabajo","Fuerza de rinoceronte","Antenas de crustáceo"};
 public static readonly string[] Tags={"BARRIDO FRONTAL","UN OBJETIVO","CORTE HORIZONTAL","ESTOCADA POSTERIOR","CORTE FRONTAL","MOVILIDAD","MOVILIDAD","MOVILIDAD","REGENERACIÓN","PROTECCIÓN","DAÑO GLOBAL","REGENERACIÓN"};
 public static readonly string[] Icons={"claw","tail","crab","sting","mantis","wings","legs","centipede","regen","shell","horn","antenna"};
 public static readonly Color[] Colors={C("e9ad67"),C("c89bda"),C("f18473"),C("f5d87c"),C("a1d883"),C("8bd3dc"),C("c1a3cc"),C("d9a270"),C("a4e1a7"),C("a0b9d8"),C("eccb97"),C("b5e5cb")};
 public static readonly float[][] Damage={new float[]{18,26,36,48,64},new float[]{25,38,54,74,100},new float[]{20,30,43,58,76},new float[]{24,36,51,70,92},new float[]{22,33,47,64,84}};
 public static readonly float[][] Interval={new float[]{1.25f,1.15f,1.05f,.95f,.85f},new float[]{1.8f,1.65f,1.5f,1.35f,1.2f},new float[]{2,1.85f,1.7f,1.55f,1.4f},new float[]{1.8f,1.65f,1.5f,1.35f,1.2f},new float[]{2.1f,1.95f,1.8f,1.65f,1.5f}};
 public static readonly float[][] Reach={new float[]{2.1f,2.2f,2.3f,2.4f,2.6f},new float[]{2.7f,2.9f,3.1f,3.3f,3.5f},new float[]{3.2f,3.4f,3.6f,3.8f,4},new float[]{2.6f,2.8f,3,3.2f,3.4f},new float[]{3.4f,3.6f,3.8f,4,4.2f}};
 public static readonly float[][] Passive={new float[]{6,10,14,18,22},new float[]{5,8,11,14,17},new float[]{4,7,10,13,16},new float[]{.5f,1,1.5f,2,3},new float[]{0,5,10,15,20},new float[]{5,10,15,20,25},new float[]{.25f,.5f,.75f,1,1.5f}};
 public static Color C(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
 public static float Value(int id,int rank){return rank<=0?0:Passive[id-5][rank-1];}
 public static string Change(int id,int rank){if(rank==0)return "Se agrega una nueva parte a Zer0.";int r=rank-1;if(id<5)return $"Daño {Damage[id][r]:0} → {Damage[id][r+1]:0}  |  {Interval[id][r]:0.##} → {Interval[id][r+1]:0.##} s";return id==9?$"Vida máxima +{Passive[id-5][r]:0} → +{Passive[id-5][r+1]:0}":$"Bonificación {Passive[id-5][r]:0.##} → {Passive[id-5][r+1]:0.##}";}
 public static string Describe(int id,int rank){
  int r=Mathf.Clamp(rank,1,5)-1;
  if(id<5){string shape=new[]{"Arco frontal de 140°. Golpea a todos en el arco.","Estocada al enemigo más cercano frente a Zer0.","Pinza horizontal a ambos lados del cuerpo.","Aguijón hacia atrás desde el extremo posterior.","Cuchillas delante de Zer0, en el eje de la cabeza."}[id];return $"{Damage[id][r]:0} daño · cada {Interval[id][r]:0.##} s\nAlcance {Reach[id][r]:0.#} u.\n\n{shape}";}
  float v=Passive[id-5][r];
  switch(id){case 5:case 6:case 7:return $"+{v:0}% velocidad de movimiento.\n\nLas bonificaciones se suman hasta +50%. No permite atravesar obstáculos.";
   case 8:case 11:return $"+{v:0.##} vida por segundo.\n\nSe activa tras 5 s sin recibir daño. Se suma a la regeneración base y otras mutaciones.";
   case 9:return $"Bloquea un ataque completo.\nSe regenera después de 3 s.\n+{v:0} vida máxima.\n\nLas mejoras refuerzan el cuerpo sin cambiar la recarga original.";
   default:return $"+{v:0}% a todo el daño.\n\nEl cuerno potencia todas las mutaciones de ataque. Se aplica una sola vez.";
  }
 }
}
}
