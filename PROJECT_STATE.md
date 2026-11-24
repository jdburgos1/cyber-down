# PROJECT_STATE.md — CyberDown (Nombre tentativo)

## 1. Identidad y Visión del Juego
- **Género:** Fútbol americano 2D arcade-táctico (1 jugador vs CPU), perspectiva vertical (atacando de abajo hacia arriba, estilo Madden clásico).
- **Estética:** Pixel art retro-futurista / cyberpunk ligero + estilo cómic (armaduras estilizadas, estelas de neón, retratos expresivos y onomatopeyas visuales como "¡CRACK!", "¡TOUCHDOWN!").
- **Plataformas:** Windows (desarrollo principal) y Android (pruebas en tablet Samsung Galaxy Tab A8, orientación horizontal).
- **Propiedad Intelectual:** 100% original (equipos, ligas, nombres y escudos ficticios para monetización segura).

## 2. Stack Tecnológico y Arquitectura
- **Motor:** Godot Engine 4 (.NET) — Renderizador: Compatibility.
- **Lenguaje:** C# (.NET 8 SDK) — IDE: Visual Studio Community.
- **Control de Versiones:** Git + GitHub (rama `master`) usando Conventional Commits.
- **Estructura de Carpetas:** `res://src/`, `res://scenes/`, `res://assets/`, `res://data/`.

## 3. Estado Actual (Completado)
- [x] **Módulo 1 (Entorno y QB):** Ventana `1280x720` (`canvas_items`, `expand`), Input Map (`move_up/down/left/right`), `Quarterback` (`CharacterBody2D`) con visual centrado (`-14, -14`) y movimiento vectorial (`QuarterbackController.cs`).
- [x] **Módulo 2 (Cancha Vertical, Cámara y Balón):** Cancha `1280x2600` (`Mouse Filter = Ignore`), `Camera2D` con `Zoom = (0.8, 0.8)`, `Position Smoothing = 5.0` y `Offset.Y = -120`; escena `Football.tscn` (`Z Index = 10`) y `Football.cs` con parábola virtual (`CurrentHeightZ`), sombra y margen de aterrizaje (`0.1s`).
- [x] **Módulo 3 (Receptor y Rutas Ofensivas):** Escena `WideReceiver.tscn` (`CharacterBody2D` + `Area2D` `CatchZone` radio `32`) y `WideReceiverController.cs` (`RunSpeed = 175`, `MaxCatchHeight = 55`) con navegación por `RouteWaypoints` y detección de atrapada por señal `AreaEntered`.

## 4. Plan de Módulos (Roadmap)
- [x] M1: Entorno y QB | [x] M2: Cancha, Cámara y Balón | [x] M3: Receptor y Rutas
- [ ] M4: Máquina de Estados de Jugada (Pre-Snap, Snap, cambio de control al receptor con balón)
- [ ] M5: Defensa básica (IA persecución, cobertura y tacleadas)
- [ ] M6: Motor de reglas deportivas puro en C# (Downs, yardas, Touchdown, reloj, pruebas unitarias)
- [ ] M7: Equipos especiales (Kickoff, despeje, gol de campo)
- [ ] M8: HUD deportivo, tarjetas cómic y controles táctiles para Samsung Tab A8
- [ ] M9: Prueba temprana de exportación Android (`.apk`)
- [ ] M10: Arte final en LibreSprite (armaduras neón, onomatopeyas) y audio en ChipTone/Audacity

## 5. Próximo Paso Inmediato
- **Módulo 4:** Implementar un controlador central de jugada (`PlayManager.cs` con Máquina de Estados: `PreSnap`, `InPlay`, `PlayDead`) para que los jugadores esperen en la línea hasta presionar `Espacio` (Snap), y transferir el control de movimiento del jugador y la cámara al Receptor cuando atrape el balón.