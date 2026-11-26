# PROJECT_STATE.md — DiberDown / CyberDown (Nombre tentativo)

## 1. Identidad y Visión del Juego
- **Género:** Fútbol americano 2D arcade-táctico (1 jugador vs CPU), perspectiva vertical (atacando de abajo hacia arriba, estilo Madden clásico).
- **Estética:** Pixel art retro-futurista / cyberpunk ligero + estilo cómic (armaduras estilizadas, estelas de neón, retratos expresivos y onomatopeyas visuales como "¡CRACK!", "¡TOUCHDOWN!").
- **Plataformas:** Windows (desarrollo principal) y Android (pruebas en tablet Samsung Galaxy Tab A8, orientación horizontal).
- **Propiedad Intelectual:** 100% original (equipos, ligas, nombres y escudos ficticios para monetización segura).

## 2. Stack Tecnológico y Arquitectura
- **Motor:** Godot Engine 4 (.NET) — Renderizador: Compatibility.
- **Lenguaje:** C# (.NET 8 SDK) — IDE: Visual Studio Community.
- **Control de Versiones:** Git + GitHub (rama `master`) usando Conventional Commits.
- **Estructura de Carpetas:** `res://src/`, `res://scenes/`, `res://assets/`, `res://data/playbook/`.

## 3. Estado Actual (Completado — 55% del MVP)
- [x] **Módulo 1 (Entorno y QB):** Ventana `1280x720` (`canvas_items`, `expand`), Input Map (`move_up/down/left/right`, `snap_ball`), `Quarterback` (`CharacterBody2D`) con visual centrado (`-14, -14`) y movimiento vectorial (`QuarterbackController.cs`).
- [x] **Módulo 2 (Cancha Vertical, Cámara y Balón):** Cancha `1280x2600` (`Mouse Filter = Ignore`), `Camera2D` independiente en `MatchField` (`Zoom = (0.8, 0.8)`, `Position Smoothing = 5.0`, `Offset.Y = -120`); escena `Football.tscn` (`Z Index = 10`) y `Football.cs` con parábola virtual (`CurrentHeightZ`), bandera `IsKick`, evento `OnBallLanded` y margen de aterrizaje (`0.1s`).
- [x] **Módulo 3 (Receptor y Rutas Ofensivas):** Escena `WideReceiver.tscn` (`CharacterBody2D` + `Area2D` `CatchZone` radio `32`) y `WideReceiverController.cs` (`RunSpeed = 175`, `BallCarrierSpeed = 250`, `MaxCatchHeight = 55`) con navegación por `RouteWaypoints` (con `Clamp` en bordes) y detección de atrapada por señal `AreaEntered`.
- [x] **Módulo 4 (Máquina de Estados de la Jugada):** Controlador `PlayManager.cs` (`PlayState`: `PreSnap`, `Kickoff`, `InPlay`, `BallCaught`, `PlayDead`). Gestiona el `Snap` con `Espacio`, transfiere control y objetivo de cámara (`_cameraTarget`) al portador activo (`ActiveBallCarrier`) y reinicia la formación con `R` (`ResetPlay`).
- [x] **Módulo 5 (Defensa Básica y Tacleadas):** Escena `Defender.tscn` (`CharacterBody2D` + `Area2D` `TackleZone` radio `34`, visual `#ff2a55`) y `DefenderController.cs` (`CoverageSpeed = 165`, `PursuitSpeed = 235`) con cobertura individual en `InPlay` y persecución/tacleada exclusiva sobre el `ActiveBallCarrier` en `BallCaught`.
- [x] **Módulo 6 (Motor de Reglas Deportivas en C# Puro):** Clase `GameRulesEngine.cs` (escala `20px = 1 yarda`, zona de anotación rival `Y <= 200`, propia `Y = 2200`) con sistema de 4 *Downs*, `1st & Goal` en las últimas 10 yardas, *Forward Progress* (`_furthestBallY`), detección de *Out of Bounds* (`X <= 14` o `X >= 1266`), *Touchdown* (+7 pts temporales), *Field Goal* (+3 pts), *Turnover on Downs*, reloj de cuarto y líneas visuales dinámicas (`LineOfScrimmage` azul y `FirstDownLine` amarilla).
- [x] **Módulo 7 (Playbook JSON 3v3 y Equipos Especiales):**
  - `res://data/playbook/plays.json`, `PlayDefinition.cs` y `PlaybookRepository.cs` con 3 jugadas seleccionables (`1`, `2`, `3` en `PreSnap`) para formación ofensiva/defensiva 3v3.
  - `SpecialTeamsController.cs` y postes dorados (`FieldGoalPosts` en `X = 560..720, Y = 120`) con medidor oscilante de 2 fases (dirección y potencia) y zoom dinámico de cámara (`0.55` en vuelo -> `0.8` al aterrizar) para **Kickoff (`K`)** y **Field Goal (`G`)**.

## 4. Plan de Módulos (Roadmap)
- [x] M1: Entorno y QB | [x] M2: Cancha, Cámara y Balón | [x] M3: Receptor y Rutas | [x] M4: Máquina de Estados | [x] M5: Defensa Básica | [x] M6: Motor de Reglas | [x] M7: Playbook 3v3 y Equipos Especiales
- [ ] M8: HUD deportivo (marcador, down & distancia, reloj, selector visual de jugadas, opción de Punto Extra / 2 pts), tarjetas cómic y controles táctiles para Samsung Tab A8
- [ ] M9: Prueba temprana de exportación Android (`.apk`) y perfilado a 60 FPS
- [ ] M10: Arte final en LibreSprite (armaduras neón, onomatopeyas) y audio en ChipTone/Audacity

## 5. Próximo Paso Inmediato
- **Módulo 8:** Crear la interfaz deportiva (`CanvasLayer` HUD) para dejar de depender de la consola de texto: mostrar marcador, cuarto, reloj, Down & Distancia, botones táctiles/visuales para elegir jugada (`1–3`), hacer `Snap`, intentar `Field Goal` o elegir entre Punto Extra (+1) / Conversión (+2) tras un Touchdown, y joystick virtual para Android.