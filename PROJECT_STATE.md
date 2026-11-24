# PROJECT_STATE.md — CyberDown (Nombre tentativo)

## 1. Identidad y Visión del Juego
- **Género:** Fútbol americano 2D arcade-táctico (1 jugador vs CPU).
- **Estética:** Pixel art retro-futurista / cyberpunk ligero + estilo cómic (armaduras estilizadas, estelas de neón, retratos expresivos y onomatopeyas visuales como "¡CRACK!", "¡TOUCHDOWN!").
- **Plataformas:** Windows (desarrollo principal) y Android (pruebas en tablet Samsung Galaxy Tab A8, orientación horizontal).
- **Propiedad Intelectual:** 100% original (equipos, ligas, nombres y escudos ficticios para monetización segura).

## 2. Stack Tecnológico y Arquitectura
- **Motor:** Godot Engine 4 (.NET) — Renderizador: Compatibility.
- **Lenguaje:** C# (.NET 8 SDK) — IDE: Visual Studio Community.
- **Control de Versiones:** Git local (rama `master`) usando Conventional Commits (`feat:`, `fix:`, `chore:`, `refactor:`).
- **Arte y Audio (fases futuras):** LibreSprite y Audacity / ChipTone.
- **Estructura de Carpetas:**
  - `res://src/` -> Scripts C# (`CyberDown.*`).
  - `res://scenes/` -> Escenas `.tscn`.
  - `res://assets/` -> Sprites, fuentes y audio.
  - `res://data/` -> Configuraciones JSON / Resources.

## 3. Estado Actual (Completado)
- [x] Entorno configurado (Godot 4 .NET, Visual Studio Community, Git local, `.gitignore`).
- [x] Resolución configurada a `1280x720` con Stretch Mode `canvas_items` y Aspect `expand`.
- [x] Input Map configurado (`move_up`, `move_down`, `move_left`, `move_right` con WASD/Flechas).
- [x] Escena `res://scenes/MatchField.tscn` con fondo temporal y nodo `Quarterback` (`CharacterBody2D`).
- [x] Script `res://src/QuarterbackController.cs` con movimiento vectorial (aceleración e inercia).

## 4. Reglas de Mentoría
- Explicar el porqué técnico de cada nodo de Godot y de las líneas clave de C# (rol de profesor/mentor).
- No quemar (hardcodear) el nombre del juego en múltiples scripts para facilitar cambios futuros.

## 5. Próximo Paso Inmediato
- **Módulo 2:** Añadir `Camera2D` con seguimiento suave (`Smoothing`) al Quarterback, ampliar la cancha para que tenga largo real de juego y programar la escena del balón (`Football.tscn`) con trayectoria parabólica (eje Z virtual de altura + sombra en el césped).