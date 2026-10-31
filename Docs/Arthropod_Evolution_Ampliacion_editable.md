
# Ampliación final | Demo Unity 1.1

31 de octubre de 2026. Se conservan las 14 páginas originales, sus imágenes, nombres, historia y reglas. Sólo se cambia Construct 3 por Unity en las referencias al motor. Esta ampliación sustituye la ampliación preliminar entregada en el chat, no el contenido original del equipo.


## Objetivo de la demostración

Construir y presentar el Nivel 1: El Jardín de los Descartes. Zer0 comienza junto al drenaje, sobrevive a oleadas durante ocho minutos, absorbe ADN, desarrolla piezas anatómicas y derrota a la Araña Lobo. Duración esperada: 8 minutos más el combate final. La pantalla de victoria aclara que termina la demostración, sin marcar la campaña de cinco niveles como completada.

Inspiración de Vampire Survivors: movimiento cenital, ataque automático, enemigos numerosos, ADN recogible y decisiones de mejora con el juego detenido. Identidad propia: larva mutágena, fauna de jardín, anatomía acumulable, pixel art diurno y cinemáticas en viñetas. No se reutilizan recursos de los juegos de referencia.


## Contenido implementado

Proyecto Unity 6000.3.25f1, escena Jardin, arena de 64 x 48 unidades, tres enemigos comunes, hormiga alfa, jefe de dos fases, doce mutaciones originales con cinco rangos, menú, prólogo, HUD, pausa, opciones, créditos y resultados. Música de menú, Jardín y jefe; efectos sintetizados, animación de sprites y piezas anatómicas visibles.

La demo empieza con Tenaza de escorpión de rango 1. El disparo de veneno que apareció en la propuesta preliminar se elimina de la ampliación: era únicamente el ejemplo de progresión del usuario. Todas las habilidades jugables provienen de la lista original.


## Estado y límites

Compilación Windows verificada: Succeeded, 0 errors. Prueba automática: 57 comprobaciones aprobadas, incluidas las cinco habilidades ofensivas en sus cinco rangos. Se verificaron visualmente menú, cinemática, combate, mejoras, jefe y resultados mediante escenarios preparados. Esto no constituye una medición de rendimiento ni una prueba de balance de una partida completa.

Pendientes para la campaña completa: niveles 2 a 5, galería, aspectos desbloqueables, cinemáticas de transición y final con el Coleccionista. El selector y la galería figuran bloqueados en la demo.


# Reglas comunes | Movimiento, salud y ADN


## Zer0 y movimiento

100 de vida máxima inicial; velocidad 3 unidades/s; una unidad equivale a un tile de 32 píxeles. Movimiento con WASD o flechas, diagonal normalizada. Colisión cuadrada de semiancho aproximado 0,3 unidades, menor que el sprite de Zer0. Las mutaciones modifican la apariencia sin agrandar su colisión.

La cámara cenital sigue a Zer0 y se limita al terreno. Macetas y piedras bloquean paso y línea de visión. Charcos reducen la velocidad de criaturas terrestres un 25%. Las alas mejoran velocidad sin atravesar obstáculos. Bonificaciones de movimiento sumadas sobre base, máximo conjunto +50%; después se aplica el charco.


## Daño y supervivencia

Después de recibir daño: invulnerabilidad compartida de 0,65 s. Un grupo superpuesto no aplica daño en cada fotograma. Tras 5 s sin daño, regenerar 1 vida/s más las pasivas adquiridas. No superar vida máxima. Con 15% o menos, la barra sobre Zer0 cambia a rojo y aparece la advertencia de vida crítica.

Caparazón: bloquea por completo un ataque, desaparece y vuelve exactamente después de 3 s. El bloqueo activa los 0,65 s de invulnerabilidad y no reinicia los 5 s sin daño. Mejorar caparazón aumenta vida máxima, sin alterar la regla de recarga original.


## ADN y niveles

Nivel inicial 1. Umbral para pasar del nivel n al siguiente: 8 + 4 x (n-1). Las recogidas atraen a 1,5 unidades y se absorben a 0,4. Se conserva el ADN sobrante. Si una recogida produce varias subidas, se encolan y se resuelven una por una.

Hasta 4 habilidades ofensivas y 4 pasivas. Adquirir una mutación ocupa una ranura; subir su rango no ocupa otra. Rango 0 indica no adquirida; rangos válidos 1 a 5. Cada subida ofrece hasta tres opciones distintas y elegibles, con al menos una ofensiva si queda alguna elegible. Las mutaciones al máximo salen del conjunto de opciones.

Si no quedan mejoras elegibles: ofrecer recuperar 20 de vida o sumar 5 de vida máxima, recuperando esos 5 puntos. Cada elección consume una sola subida pendiente. Bloqueo de confirmación de 0,25 s al abrir una oferta. Elegir con ratón, 1/2/3, o flechas y Enter.


## Resolución

Daño final = daño del rango x (1 + porcentaje de Fuerza de rinoceronte / 100). Se aplica una sola vez. Cada activación golpea una vez por enemigo; no hay crítico ni fuego amigo. Los ataques preparados esperan hasta tener un objetivo válido. Las áreas requieren línea de visión, para que las piedras no permitan atacar a través de ellas.

Pausa, mejoras, prólogo y resultados detienen el reloj de combate, oleadas y recargas. Las animaciones y controles de UI usan tiempo real. Al perder foco, la partida activa entra en pausa. Si Zer0 llega a cero, se resuelve derrota antes que victoria.


# Mutaciones ofensivas | Tenaza, cola y pinza


## Tenaza de escorpión

Arco frontal de 140 grados que sigue la última dirección de movimiento. Afecta a todos los blancos válidos dentro del arco. Brazo articulado visible y trazo de barrido.

Rango | Daño | Intervalo (s) | Alcance / ancho (u)

1 | 18 | 1,25 | 2,1

2 | 26 | 1,15 | 2,2

3 | 36 | 1,05 | 2,3

4 | 48 | 0,95 | 2,4

5 | 64 | 0,85 | 2,6


## Cola de Escorpión

Un objetivo, el más cercano dentro del cono frontal de 120 grados. Conserva su identidad de ataque puntual. Cola curvada visible, estocada y retorno.

Rango | Daño | Intervalo (s) | Alcance / ancho (u)

1 | 25 | 1,8 | 2,7

2 | 38 | 1,65 | 2,9

3 | 54 | 1,5 | 3,1

4 | 74 | 1,35 | 3,3

5 | 100 | 1,2 | 3,5


## Pinza de cangrejo

Franja horizontal centrada en Zer0, ancho según la tabla y semialtura 0,65 unidades. Ataca a ambos lados. Retroceso de 0,25 unidades a comunes; el jefe no retrocede.

Rango | Daño | Intervalo (s) | Alcance / ancho (u)

1 | 20 | 2 | 3,2

2 | 30 | 1,85 | 3,4

3 | 43 | 1,7 | 3,6

4 | 58 | 1,55 | 3,8

5 | 76 | 1,4 | 4


# Mutaciones ofensivas | Aguijón y mantis


## Aguijón de Abeja

Estocada vertical hacia abajo del escenario. Semiancho 0,6 unidades; longitud según tabla. Mantiene la dirección inferior del diseño original.

Rango | Daño | Intervalo (s) | Alcance / ancho (u)

1 | 24 | 1,8 | 2,6

2 | 36 | 1,65 | 2,8

3 | 51 | 1,5 | 3

4 | 70 | 1,35 | 3,2

5 | 92 | 1,2 | 3,4


## Patas delanteras de Mantis

Corte horizontal por encima de Zer0, entre 0,25 y 1,7 unidades de altura; ancho según tabla. Ambas patas representan una sola activación, sin duplicar daño.

Rango | Daño | Intervalo (s) | Alcance / ancho (u)

1 | 22 | 2,1 | 3,4

2 | 33 | 1,95 | 3,6

3 | 47 | 1,8 | 3,8

4 | 64 | 1,65 | 4

5 | 84 | 1,5 | 4,2


## Sentido de las mejoras

Cada rango suma daño, acorta el intervalo y amplía moderadamente la zona eficaz. La tenaza limpia un frente amplio; la cola concentra daño; pinza cubre costados; aguijón protege la retaguardia inferior; mantis despeja la parte superior. No se agregan efectos de veneno a estas habilidades.


## Variantes visuales

Desde la adquisición aparece la pieza anatómica correspondiente. La escala crece de 0,715 en rango 1 a 0,975 en rango 5 para piezas ofensivas. Cada pieza mantiene su color e icono; oscilación de articulaciones y trazos de ataque comunican la activación. Las tarjetas comparan daño e intervalo actual con el siguiente rango.


# Mutaciones pasivas | Valores de cinco rangos

Mutación | R1 | R2 | R3 | R4 | R5

Alas de libélula: velocidad % | 6 | 10 | 14 | 18 | 22

Patas de araña: velocidad % | 5 | 8 | 11 | 14 | 17

Patas de Ciempiés: velocidad % | 4 | 7 | 10 | 13 | 16

Exoesqueleto Regenerativo: vida/s | 0.5 | 1 | 1.5 | 2 | 3

Caparazón: vida máxima extra | 0 | 5 | 10 | 15 | 20

Fuerza de Escarabajo Rinoceronte: daño % | 5 | 10 | 15 | 20 | 25

Antenas de Crustáceo: vida/s | 0.25 | 0.5 | 0.75 | 1 | 1.5


## Movimiento

Las tres familias de locomoción suman sus porcentajes, con límite conjunto de +50%. Alas incorporan una silueta dorsal y batido oscilante; araña agrega patas laterales; ciempiés agrega segmentos inferiores en marcha. Las piezas ocupan capas distintas, por lo que pueden coexistir.


## Regeneración

Exoesqueleto y antenas se suman entre sí y a la recuperación base de 1 vida/s, siempre después de 5 s sin recibir daño. No reducen ese tiempo de espera. Las antenas se muestran como apéndices visibles desde el rango 1; sus rangos aumentan la escala. El exoesqueleto se representa con placas regenerativas.


## Caparazón

Todos los rangos bloquean un único golpe completo y recargan en 3 s. Rango 1 respeta exactamente el comportamiento original; los rangos posteriores aumentan la vida máxima total en +5/+10/+15/+20. Al subir el máximo se recupera únicamente su incremento. La pieza se ve tenue durante la recarga; el HUD muestra ESCUDO LISTO o los segundos restantes.


## Fuerza de Escarabajo Rinoceronte

Rango 1 conserva el +5% del GDD. Los siguientes rangos suman cinco puntos porcentuales cada uno, hasta +25%. El cuerno visible aumenta con el rango. Este bonus afecta a todos los ataques sin modificar sus patrones, alcance o tiempo de recarga.


# Nivel 1 | Jardín de los Descartes

Escenario diurno con borde sólido en x +/-32 e y +/-24; movimiento limitado aproximadamente a x +/-31 e y +/-23. Origen central, a diferencia del plano preliminar. Zer0 inicia en (0,-10), cerca del drenaje (0,-12,5). La Araña Lobo aparece en (0,12).


## Distribución y navegación

Macetas: (-22,-14), (14,-12), (-20,10), (14,10), todas de 4 x 4. Piedras: (-10,12) y (6,-17), de 3 x 3. Charcos: (-16,-4), de 4 x 5, y (11,1), de 4 x 4. Las coordenadas indican la esquina inferior izquierda. El mapa se genera con posiciones fijas; la vegetación usa una semilla repetible.

El claro central deja espacio para esquivar y recoger ADN; las macetas proponen rodeos y zonas de presión. Los enemigos usan persecución con deslizamiento local alrededor de obstáculos. No se implementa búsqueda global de rutas: verificar posibles atascos forma parte del ajuste futuro del nivel.

No hay apariciones comunes durante los primeros 3 s. Después se buscan puntos en un anillo de 13 a 18 unidades alrededor de Zer0, fuera del campo visible ampliado un 8%. Se descartan posiciones bloqueadas, hasta 16 intentos por aparición. Los fallos no se acumulan.


# Oleadas y enemigos | Ritmo de supervivencia

Tiempo | Ritmo | Mezcla | Límite vivo

0:03-1:00 | 1/s | 100% hormiga | 35

1:00-2:00 | 1,5/s | 80% hormiga / 20% araña | 50

2:00-4:00 | 2/s | 60% hormiga / 20% araña / 20% bolita | 75

4:00-6:00 | 2,5/s | 45% hormiga / 35% araña / 20% bolita | 100

6:00-8:00 | 3/s | 45% hormiga / 35% araña / 20% bolita | 120

8:00 en adelante | Sin oleadas comunes | Jefe y sus invocaciones | Hasta 125 en invocaciones

Hormiga alfa al minuto 3 y al minuto 6, con un intento de ubicación por evento. Comunes: vida base x [1 + 0,08 x minutos completos]. Daño sin escalado temporal. Élites y jefe mantienen valores fijos. Si se alcanza el límite, descartar el intento sin generar deuda de aparición.

Tipo | Vida base | Velocidad | Daño | ADN

Hormiga | 20 | 1.45 | 8 | 2

Araña | 35 | 1.7 | 12 | 3

Bicho bolita | 65 | 0.9 | 15 | 5

Hormiga alfa | 180 | 1.5 | 18 | 20


## Comportamientos y avisos

Hormiga: persigue, prepara mordida 0,3 s y activa golpe breve durante 0,15 s. Hormiga alfa: silueta mayor y tonalidad dorada; aviso 0,4 s. Ambas recuperan antes de volver a perseguir.

Araña: persigue; cada ciclo prepara avance con línea direccional durante 0,65 s, fija dirección y recorre hasta 2,5 unidades en 0,5 s. Recupera 0,65 s. Bicho bolita: aviso de 0,85 s, rodada hasta 3 unidades en 0,75 s, recuperación 1 s. La colisión del escenario detiene o desliza su avance.

El contacto reduce salud sólo si terminó la invulnerabilidad de Zer0. Las señales rojas usan líneas, puntas y círculos. Un enemigo muerto desactiva colisión y ataques, entrega una única recogida y vuelve al conjunto reutilizable.


# Jefe | Araña Lobo

2400 de vida; velocidad de persecución 1,1 unidades/s; contacto 20; inmune al retroceso. Aparece al minuto 8 en el claro norte con aviso de 2 s. Las oleadas comunes cesan, pero los enemigos existentes permanecen. La barra del jefe y su nombre aparecen debajo del reloj; si está fuera de cámara se muestra un indicador de dirección.


## Fase 1: más del 50% de vida

Perseguir durante 2 s; avisar embestida 0,9 s con una línea de cinco unidades; fijar dirección; embestir a velocidad 5 durante 1 s con daño 25; recuperar 1,2 s. Después, fijar posición objetivo del salto en la ubicación actual de Zer0 y mostrar un círculo de radio 1,8 durante 1 s. Tras 0,45 s de salto, aterrizar con daño 25 y recuperar 1,5 s. Repetir el ciclo.


## Fase 2: 50% o menos

Al llegar a un estado de persecución o recuperación compatible, entrar a transición de 1 s con aviso de FRENESÍ e inmunidad durante esa transición. A partir de ahí, hacer dos embestidas consecutivas, cada una con su propio aviso de 0,75 s. El salto usa radio 2,2 y causa 28 de daño. No se acortan los avisos por debajo de esos valores.


## Invocaciones y reglas justas

Invoca hasta cuatro hormigas cada 12 s; primer contador preparado a los 3 s después de aparecer. Se valida suelo libre y distancia mínima de 2 unidades respecto de Zer0, hasta un límite de 125 entidades en la lista del combate. No aplicar daño al crear la invocación.

El contacto sólo daña durante persecución. Las embestidas y el aterrizaje usan sus golpes respectivos y respetan los 0,65 s de invulnerabilidad del jugador. El salto fija su destino al iniciar el aviso; si el punto final queda bloqueado, vuelve al origen del salto. El jefe no golpea por contacto en el aire.

Al llegar a cero, se desactiva el jefe y se resuelve victoria si Zer0 sigue vivo. El estado de resultados detiene la simulación. No se continúa al Nivel 2: la pantalla comunica fin de la demostración.


## Puntuación

Puntos = bajas x 10 + nivel x 100 + 2000 al vencer al jefe. Mostrar tiempo, bajas, nivel, daño total, puntuación e iconos con sus rangos. Guardar récord local. Botones para reintentar o volver al menú; el reintento limpia enemigos, ADN, mutaciones y temporizadores.


# Cinemática, arte, animación y audio


## El Coleccionista

Prólogo en tres viñetas: 1) científico insectólogo observa con desprecio a Zer0 en un terrario; 2) descarta el espécimen hacia el drenaje; 3) la larva despierta en el barro del jardín, con una araña como amenaza futura. Dirección visual solicitada: científico caricaturesco de aspecto zombi, gran cabeza, bata, guantes violetas y gafas de coleccionista, inspirado en el arquetipo de Plants vs. Zombies.

Cada viñeta se destaca durante aproximadamente 4 s mientras las demás quedan oscurecidas. Subtítulos independientes de la imagen. Enter o botón avanza; Saltar cinemática inicia la partida. Tras el prólogo aparecen los controles y se espera confirmación. Las dos formulaciones del prólogo del documento original permanecen intactas; esta demo representa la del descarte.


## Pixel art y anatomía

Tiles y Zer0 de 32 px; enemigos de 32 px, jefe de 64 px, escalados en el mundo. Texturas Point, sin mipmaps ni compresión. Zer0 usa seis cuadros; enemigos y jefe cuatro cuadros. Piezas anatómicas se agregan por capas: espalda, costados, vientre, cola y cabeza. Alas y patas oscilan; ofensivas muestran trazos de arco, estocada o corte. La demo usa cámara ortográfica; no implementa encuadre pixel-perfect entero.


## Sonido

Tres bucles originales sintetizados: menú a 90 BPM, Jardín a 120 BPM y jefe a 144 BPM. La música cambia al aparecer la Araña Lobo. Efectos de disparador de ataque físico, impacto, ADN, daño, bloqueo, selección y resultado mediante síntesis breve. Volumen de música y efectos ajustable por separado.

Los bucles y sprites forman parte del proyecto; no requieren descargas en ejecución. Las viñetas fueron generadas con ImageGen integrado. No hay voces grabadas ni una película pre-renderizada: la cinemática se reproduce dentro del juego mediante imagen, énfasis temporal y subtítulos.


# Interfaz, opciones y estructura del proyecto


## HUD y menús

Se mantienen las posiciones originales: salud sobre Zer0, ADN en la parte inferior, mutaciones arriba a la izquierda, tiempo arriba al centro y dirección del jefe cuando corresponde. Se agregan rangos, bajas y barra del jefe. El HUD mantiene fondo oscuro para legibilidad frente al césped.

Tarjetas de evolución: nombre, icono, categoría, rango siguiente, descripción y comparación de valores. La pausa ofrece continuar, opciones, reiniciar y menú. La pantalla de resultados muestra la anatomía adquirida por sus iconos y rangos. Selector de niveles y galería están visibles y bloqueados en la demo.


## Opciones realmente implementadas

Música y efectos 0-100%; intensidad de vibración 0-100%; números de daño activables; destellos reducidos; ventana/pantalla completa; límite 30/60/120 FPS. UI escalada a una referencia de 1280 x 720. Persistencia local en JSON dentro de la carpeta de datos persistentes de Unity, con escritura temporal y respaldo del archivo anterior.

La reasignación de teclas, selector detallado de resolución, mezclador por grupos de ambiente y escala de UI independiente quedan como ampliaciones futuras. Los objetivos de la propuesta preliminar no se presentan como funciones ya terminadas.


## Implementación y edición

DemoGame.cs coordina partida, generación del mapa, enemigos, jefe, salud, ADN, mejoras, UI y audio. EvolutionData.cs contiene las tablas de rangos. Assets/Resources/Art y Audio contienen los recursos. DemoBuilder prepara la escena, valida datos y genera la compilación. PixelImporter establece el tratamiento de sprites.

La arena se construye al entrar a Play; las posiciones del plano están en BuildGarden. Los enemigos, ADN y partículas reutilizan objetos. Los avisos de ataque se crean por activación y se retiran al expirar. El ciclo de simulación usa Update y un delta limitado a 0,05 s. No se afirma una arquitectura de ScriptableObjects o FixedUpdate que esta demo no utilice.


## Comprobación y entrega

Proyecto en el Escritorio: ArthropodEvolution. Ejecutable en Build/Windows; distribuir la carpeta completa. Proyecto fuente y documentación preparados para repositorio público, excluyendo Library, Temp, Logs, UserSettings y Build mediante .gitignore. Los cinco niveles originales siguen siendo el alcance de campaña del equipo.

Evidencia: 57 pruebas aprobadas y compilación Windows sin errores; aplicación independiente abierta y opciones inspeccionadas. Las capturas de Validation/Previews corresponden a escenarios preparados para revisar UI. Falta medir FPS en la computadora de exposición y ajustar dificultad mediante partidas completas con jugadores.
