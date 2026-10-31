# Arthropod Evolution

Demostración del **Jardín de los Descartes** para PC, basada en el GDD de Bovatitos. Unity **6000.3.25f1**, proyecto 2D con renderizado integrado.

## Jugar

Abrir el proyecto con Unity Hub, esperar la importación y abrir `Assets/Scenes/Jardin.unity`. La escena también se prepara automáticamente desde el menú **Arthropod → Preparar y abrir Jardín**. Presionar Play y **Entrar al Jardín**.

- WASD o flechas: moverse; la dirección diagonal se normaliza.
- Los ataques son automáticos; los ataques frontales siguen la última dirección de movimiento.
- Acercarse a los cristales para absorber ADN.
- Elegir mejoras con el ratón o 1/2/3; A/D o flechas y Enter también están disponibles con la vista Game enfocada.
- Escape: pausa. Perder el foco también pausa el combate.
- Sobrevivir ocho minutos y derrotar a la Araña Lobo.

La cinemática tiene tres viñetas con avance automático, subtítulos y opción de saltarla. Después muestra los controles y espera la confirmación del jugador.

## Contenido

Una arena diurna de 64 × 48 unidades con macetas, piedras, charcos, drenaje y vegetación; hormigas, arañas y bichos bolita; hormigas alfa a los minutos 3 y 6; Araña Lobo con embestidas, salto e invocaciones y una segunda fase al 50% de salud.

Las doce mutaciones del GDD tienen cinco rangos y piezas visibles. Zer0 empieza con **Tenaza de escorpión**. El ejemplo de disparo de veneno no se incluye. Hay cuatro ranuras ofensivas y cuatro pasivas. La regeneración comienza tras cinco segundos sin daño; el caparazón bloquea un golpe y vuelve tras tres segundos.

Menú, prólogo, HUD, selección de mejoras, pausa, opciones, créditos y resultados. Los créditos muestran a los cuatro integrantes con avatares pixelados animados, nombres completos y un tema cómico dedicado de 140 BPM. Música, efectos sintetizados, animación por cuadros, partículas y señales de ataque. Se guardan opciones y récord local; no se reanuda una partida cerrada.

## Compilar

Menú **Arthropod → Compilar demo Windows**. Salida: `Build/Windows/ArthropodEvolution.exe`. Distribuir **toda la carpeta Windows**, incluyendo la carpeta Data y DLL necesarias.

## Organización

- `Assets/Scripts/DemoGame.cs`: ciclo de partida, arena, enemigos, jefe, combate, UI, audio y persistencia.
- `Assets/Scripts/EvolutionData.cs`: nombres, cinco rangos, daño, intervalos, alcance y pasivas.
- `Assets/Resources/Art`: sprites originales, piezas anatómicas y viñetas.
- `Assets/Resources/Audio`: cuatro bucles musicales originales.
- `Assets/Editor`: importación pixelada, preparación de escena, compilación y validación.
- `Validation`: resultados de pruebas y capturas de escenarios preparados para verificar la interfaz; las capturas no son una grabación de una partida completa.

El escenario se construye al iniciar la partida a partir de posiciones fijas y una semilla de decoración. No requiere paquetes de arte externos ni conexión a Internet durante el juego. El código utiliza Input clásico y OnGUI; no requiere Input System ni TextMeshPro.

## Validación y presentación

El menú **Arthropod → Validar datos** comprueba los rangos de ataque. Las pruebas internas del editor verifican combate, ADN, escudo, ranuras, jefe y reinicio. Los resultados y el estado real de la compilación se incluyen en `Validation`.

Sólo en el editor o compilación de desarrollo: F7 recupera salud, F8 salta al jefe, F9 añade ADN, F6 muestra todas las mutaciones al máximo y F10 derrota al jefe. Estas ayudas no se incluyen en la compilación normal.

La demo cubre el Nivel 1. El resto de la campaña, selector de niveles, galería, skins y enfrentamiento final con el Coleccionista permanecen como diseño futuro del GDD. El prólogo representa la variante del descarte del espécimen; las dos formulaciones narrativas originales se preservan en el documento.

## Créditos

Bovatitos: Santino Bovati (Lead Programmer), Brandon Emmanuel Li (Game Designer), Victor Ome Joven (Level Designer), Mauro Arcadigni (Game Artist), según el GDD original.

Sprites pixelados, música y efectos creados para este prototipo. Viñetas del Coleccionista: científico insectólogo caricaturesco de aspecto zombi, terrario, descarte al drenaje y despertar de Zer0. La referencia visual solicitada fue el arquetipo de científico zombi de Plants vs. Zombies; no se reutilizaron archivos de ese juego. Inspiración de jugabilidad: Vampire Survivors; no se reutilizaron sus recursos.

El proyecto conserva los créditos y la autoría del diseño original. No se agrega una licencia de redistribución al diseño del equipo sin su decisión.

