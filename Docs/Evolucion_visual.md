# Evolución visual y animaciones

Zer0 conserva el boceto aprobado: larva rosada segmentada, dos ojos negros, dientes triangulares y cola puntiaguda. La cinemática usa el mismo personaje apoyado sobre el vientre y con la boca orientada hacia delante.

Cada una de las doce mutaciones tiene cinco modelos anatómicos y cuatro cuadros por modelo. Una mejora cambia el modelo visible al instante. Se mantienen la especie, los colores y la función de la mutación: la progresión añade articulaciones, placas, dientes o superficies útiles, sin introducir poderes nuevos.

| Mutación | Progresión de rango 1 a 5 |
|---|---|
| Tenaza de escorpión | Pinza simple, base reforzada, mandíbulas dentadas, placas más gruesas y pinza madura con espolones protectores. |
| Cola de escorpión | Cola corta, más segmentos, punta recurvada, articulaciones acorazadas y aguijón maduro. |
| Pinza de cangrejo | Pinza pequeña, bisagra robusta, dientes intercalados, placas protectoras y mandíbula trituradora madura. |
| Aguijón de abeja | Punta lisa, base segmentada, primeras púas hacia atrás, dobles púas y aguijón reforzado con más barbas. |
| Patas de mantis | Hoces simples, codos reforzados, espinas interiores, cuchillas curvas más largas y hoces maduras dentadas. |
| Alas de libélula | Alas cortas, membrana más larga, nervaduras definidas, raíces reforzadas y alas maduras articuladas. |
| Patas de araña | Patas pequeñas, mayor longitud, rodillas visibles, articulaciones protegidas y patas maduras con ganchos. |
| Patas de ciempiés | Tres, cuatro, cinco, seis y siete pares, con desarrollo gradual de articulaciones y extremos. |
| Exoesqueleto regenerativo | Una placa, dos, tres, placas solapadas articuladas y refuerzos laterales. |
| Caparazón de escarabajo | Cubierta simple, dos élitros más amplios, nervio central, borde reforzado y carapacho maduro con costillas. |
| Fuerza de rinoceronte | Pequeño cuerno, cuerno curvado, punta bifurcada, base reforzada y cuerno maduro con estrías. |
| Antenas de crustáceo | Antenas cortas, segmentos adicionales, primeros pelos sensitivos, extremos ramificados y abanicos táctiles maduros. |

## Puntos de unión y dirección

Las raíces de las piezas permanecen en el cuerpo durante el golpe. El cuerpo y sus puntos de unión giran juntos con la dirección de movimiento. Los ataques y sus zonas de impacto usan el mismo eje anatómico:

- Tenaza: lateral delantero, barrido hacia delante.
- Cola: unión posterior; se curva sobre el cuerpo para alcanzar blancos delanteros.
- Pinzas de cangrejo: pareja a ambos lados; cierre y ataque lateral.
- Aguijón: extremo posterior; estocada hacia atrás.
- Mantis: unión delantera; corte frontal de las dos cuchillas.
- Alas y patas: detrás del cuerpo; caparazón y placas sobre el dorso; cuerno y antenas en la cabeza.

Esto precisa las direcciones respecto del cuerpo, en lugar de fijar arriba y abajo a la pantalla. Los alcances, daños e intervalos por rango se conservan.

## Respuesta del ataque

Cada ataque tiene anticipación, extensión, impacto y recuperación. El modelo cambia de pose, la extremidad se despliega desde su raíz y deja una estela breve de su propio sprite. Zer0 comprime el cuerpo levemente al golpear. Las alas aletean, las patas caminan y las antenas oscilan. El caparazón se atenúa mientras recarga.

Los enemigos tienen cuatro cuadros de marcha, compresión antes de atacar y extensión al embestir. Su orientación durante una carga coincide con la trayectoria anunciada. La Araña Lobo tiene una elevación visual durante el salto.

## Condiciones de partida

La Araña Lobo aparece a los 8 minutos. Se gana derrotándola antes de 10:00 y conservando vida. Se pierde por vida en cero o al alcanzar 10:00. El reloj activo no avanza durante pausa, opciones ni selección de mejoras.

## Ambiente y dificultad progresiva

Luces suaves aditivas alrededor de Zer0 y el jefe, reflejo del desagüe, polen flotante, polvo al caminar e impactos luminosos. Son efectos visuales; no cambian los valores de las mutaciones ni sus recargas.

La aparición de comunes crece por tramos de 1,1 a 5,2 por segundo. Hasta el minuto 8, los nuevos comunes alcanzan 2,28 veces la vida inicial, 1,24 veces la velocidad y 1,48 veces el daño. Las hormigas embisten al morder, las arañas flanquean y anticipan levemente el movimiento, y los bichos bolita reducen su descanso gradualmente. Se conserva la anticipación corporal de las animaciones. No se dibujan flechas, líneas ni zonas de daño enemigas en el suelo. El jefe conserva 2400 de vida e invoca refuerzos con un límite de 16 comunes activos durante su combate.
