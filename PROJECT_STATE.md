# PROJECT_STATE.md — DiberDown / CyberDown (Nombre tentativo)

## 1. Identidad y Visión del Juego
- **Género:** Fútbol americano 2D arcade-táctico (1 jugador vs CPU), perspectiva vertical (atacando de abajo hacia arriba, estilo Madden clásico)[cite: 3].
- **Estética:** Pixel art retro-futurista / cyberpunk ligero + estilo cómic (armaduras estilizadas, estelas de neón, retratos expresivos y onomatopeyas visuales como "¡CRACK!", "¡TOUCHDOWN!")[cite: 3].
- **Plataformas:** Windows (desarrollo principal) y Android (pruebas en tablet Samsung Galaxy Tab A8, orientación horizontal)[cite: 3].
- **Propiedad Intelectual:** 100% original (equipos, ligas, nombres y escudos ficticios para monetización segura)[cite: 3].

## 2. Stack Tecnológico y Arquitectura
- **Motor:** Godot Engine 4 (.NET) — Renderizador: Compatibility[cite: 3].
- **Lenguaje:** C# (.NET 8 SDK) — IDE: Visual Studio Community[cite: 3].
- **Control de Versiones:** Git + GitHub (rama `master`) usando Conventional Commits[cite: 3].
- **Estructura de Carpetas:** `res://src/`, `res://scenes/`, `res://assets/`, `res://data/`[cite: 3].

## 3. Estado Actual (Completado)
- [x] **Módulo 1 (Entorno y QB):** Ventana `1280x720` (`canvas_items`, `expand`), Input Map (`move_up/down/left/right`, `snap_ball`), `Quarterback` (`CharacterBody2D`) con visual centrado (`-14, -14`) y movimiento vectorial (`QuarterbackController.cs`)[cite: 3].
- [x] **Módulo 2 (Cancha Vertical, Cámara y Balón):** Cancha `1280x2600` (`Mouse Filter = Ignore`), `Camera2D` independiente en `MatchField` (`Zoom = (0.8, 0.8)`, `Position Smoothing = 5.0`, `Offset.Y = -120`); escena `Football.tscn` (`Z Index = 10`) y `Football.cs` con parábola virtual (`CurrentHeightZ`), sombra y margen de aterrizaje (`0.1s`)[cite: 3].
- [x] **Módulo 3 (Receptor y Rutas Ofensivas):** Escena `WideReceiver.tscn` (`CharacterBody2D` + `Area2D` `CatchZone` radio `32`) y `WideReceiverController.cs` (`RunSpeed = 175`, `BallCarrierSpeed = 250`, `MaxCatchHeight = 55`) con navegación por `RouteWaypoints` y detección de atrapada por señal `AreaEntered`[cite: 3].
- [x] **Módulo 4 (Máquina de Estados de la Jugada):** Controlador `PlayManager.cs` (`PlayState`: `PreSnap`, `InPlay`, `BallCaught`). Los jugadores esperan en la línea hasta presionar `Espacio` (`StartSnap`), y al atrapar el balón se transfiere el control de movimiento y el objetivo de cámara (`_cameraTarget`) suavemente hacia el receptor.

## 4. Plan de Módulos (Roadmap)
- [x] M1: Entorno y QB | [x] M2: Cancha, Cámara y Balón | [x] M3: Receptor y Rutas | [x] M4: Máquina de Estados de Jugada[cite: 3]
- [ ] M5: Defensa básica (IA de persecución, cobertura simple y tacleadas que finalizan la jugada)[cite: 3]
- [ ] M6: Motor de reglas deportivas puro en C# (Downs, yardas, Touchdown, reloj, pruebas unitarias)[cite: 3]
- [ ] M7: Equipos especiales (Kickoff, despeje, gol de campo)[cite: 3]
- [ ] M8: HUD deportivo, tarjetas cómic y controles táctiles para Samsung Tab A8[cite: 3]
- [ ] M9: Prueba temprana de exportación Android (`.apk`)[cite: 3]
- [ ] M10: Arte final en LibreSprite (armaduras neón, onomatopeyas) y audio en ChipTone/Audacity[cite: 3]

## 5. Próximo Paso Inmediato
- **Módulo 5:** Crear al primer defensor (`Defender.tscn` y `DefenderController.cs`) que cubra o persiga al portador del balón y detecte la tacleada para cambiar el estado de `PlayManager` a `PlayDead` (fin de jugada) y reiniciar la formación.