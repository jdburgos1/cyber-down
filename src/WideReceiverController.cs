using Godot;
using CyberDown.Gameplay;

namespace CyberDown.Players
{
    public partial class WideReceiverController : CharacterBody2D
    {
        [Export] public float RunSpeed { get; set; } = 175.0f;
        [Export] public float BallCarrierSpeed { get; set; } = 250.0f; // Corre más rápido tras atrapar
        [Export] public float MaxCatchHeight { get; set; } = 55.0f;

        // Puntos relativos a donde inicia el jugador (X = derecha/izquierda, Y negativo = hacia arriba)
        [Export]
        public Vector2[] RouteWaypoints { get; set; } = new Vector2[]
        {
            new Vector2(0, -320),    // 1. Corre recto hacia arriba 220px
            new Vector2(-160, -480)   // 2. Corta en diagonal hacia la derecha (ruta Post)
        };

        private Vector2 _startPosition;
        private int _currentWaypointIndex = 0;
        private bool _hasBall = false;
        private Area2D _catchZone;
        private ColorRect _visual;
        public bool HasBall => _hasBall;

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
            if (PlayManager.Instance == null) return;

            PlayState state = PlayManager.Instance.CurrentState;

            // 1. Antes del Snap: quieto en posición
            if (state == PlayState.PreSnap || state == PlayState.PlayDead)
            {
                Velocity = Vector2.Zero;
                return;
            }

            // 2. Jugada en marcha (sin balón): corre la ruta automática
            if (state == PlayState.InPlay && !_hasBall)
            {
                RunRoute();
            }
            // 3. Ya atrapó el balón: el jugador toma el control con WASD/Flechas
            else if (state == PlayState.BallCaught && _hasBall)
            {
                ControlBallCarrier();
            }
        }

        public void AssignRoute(Vector2[] newWaypoints)
        {
            RouteWaypoints = newWaypoints;
            _currentWaypointIndex = 0;
        }

        private void RunRoute()
        {
            if (_currentWaypointIndex < RouteWaypoints.Length)
            {
                Vector2 rawTarget = _startPosition + RouteWaypoints[_currentWaypointIndex];

                // Limitamos el destino para que la ruta nunca saque al receptor de los límites del campo
                Vector2 targetPoint = new Vector2(
                    Mathf.Clamp(rawTarget.X, 14.0f, 1266.0f),
                    Mathf.Max(rawTarget.Y, 30.0f)
                );

                Vector2 direction = GlobalPosition.DirectionTo(targetPoint);

                Velocity = direction * RunSpeed;
                MoveAndSlide();

                if (GlobalPosition.DistanceTo(targetPoint) < 10.0f)
                {
                    _currentWaypointIndex++;
                }
            }
            else
            {
                Velocity = Vector2.Zero;
            }
        }

        private void ControlBallCarrier()
        {
            Vector2 inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            Velocity = inputDirection * BallCarrierSpeed;
            MoveAndSlide();
        }

        private void OnBallEntered(Area2D area)
        {
            // Verificamos si el área que entró es nuestro Football y si no tenemos ya el balón
            if (_hasBall || area is not Football ball || ball.IsKick) return;

            // Solo atrapa el balón si está en el aire Y a una altura alcanzable por las manos
            if (ball.IsInAir && ball.CurrentHeightZ <= MaxCatchHeight)
            {
                _hasBall = true;
                ball.Catch();

                // Cambiamos el color del receptor a verde brillante como feedback visual de "¡ATRAPADA!"
                _visual.Color = new Color("#00ff66");
                GD.Print("¡PASE COMPLETO! El receptor atrapó el balón.");

                // Avisamos al PlayManager para que transfiera la cámara y el control
                PlayManager.Instance?.OnPassCompleted(this);
            }
        }

        public void ResetPlayer()
        {
            _startPosition = GlobalPosition;
            _hasBall = false;
            _currentWaypointIndex = 0;
            Velocity = Vector2.Zero;
            if (_visual != null)
                _visual.Color = new Color("#ffe600");
        }
    }
}