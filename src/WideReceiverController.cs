using Godot;
using CyberDown.Gameplay;

namespace CyberDown.Players
{
    public partial class WideReceiverController : CharacterBody2D
    {
        [Export] public float RunSpeed { get; set; } = 240.0f;
        [Export] public float MaxCatchHeight { get; set; } = 38.0f;

        // Puntos relativos a donde inicia el jugador (X = derecha/izquierda, Y negativo = hacia arriba)
        [Export]
        public Vector2[] RouteWaypoints { get; set; } = new Vector2[]
        {
            new Vector2(0, -220),    // 1. Corre recto hacia arriba 220px
            new Vector2(160, -380)   // 2. Corta en diagonal hacia la derecha (ruta Post)
        };

        private Vector2 _startPosition;
        private int _currentWaypointIndex = 0;
        private bool _hasBall = false;
        private Area2D _catchZone;
        private ColorRect _visual;

        public override void _Ready()
        {
            _startPosition = GlobalPosition;
            _visual = GetNode<ColorRect>("VisualPlaceholder");
            _catchZone = GetNode<Area2D>("CatchZone");

            // Conectamos por código la señal AreaEntered de CatchZone a nuestro método OnBallEntered
            _catchZone.AreaEntered += OnBallEntered;
        }

        public override void _PhysicsProcess(double delta)
        {
            // Si aún quedan puntos en la ruta por recorrer, corremos hacia el siguiente punto
            if (_currentWaypointIndex < RouteWaypoints.Length)
            {
                Vector2 targetPoint = _startPosition + RouteWaypoints[_currentWaypointIndex];
                Vector2 direction = GlobalPosition.DirectionTo(targetPoint);

                Velocity = direction * RunSpeed;
                MoveAndSlide();

                // Si ya estamos a menos de 10 píxeles del punto actual, pasamos al siguiente corte de la ruta
                if (GlobalPosition.DistanceTo(targetPoint) < 10.0f)
                {
                    _currentWaypointIndex++;
                }
            }
            else
            {
                // Al terminar la ruta, frena suavemente
                Velocity = Vector2.Zero;
            }
        }

        private void OnBallEntered(Area2D area)
        {
            // Verificamos si el área que entró es nuestro Football y si no tenemos ya el balón
            if (_hasBall || area is not Football ball) return;

            // Solo atrapa el balón si está en el aire Y a una altura alcanzable por las manos
            if (ball.IsInAir && ball.CurrentHeightZ <= MaxCatchHeight)
            {
                _hasBall = true;
                ball.Catch();

                // Cambiamos el color del receptor a verde brillante como feedback visual de "¡ATRAPADA!"
                _visual.Color = new Color("#00ff66");
                GD.Print("¡PASE COMPLETO! El receptor atrapó el balón.");
            }
        }
    }
}