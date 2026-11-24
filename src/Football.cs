using Godot;
using System;

namespace CyberDown.Gameplay
{
    public partial class Football : Area2D
    {
        [Export] public float FlightSpeed { get; set; } = 650.0f;
        [Export] public float MaxArcHeight { get; set; } = 90.0f;

        private Vector2 _startPos;
        private Vector2 _targetPos;
        private float _flightDuration;
        private float _elapsedTime = 0.0f;
        private bool _isInAir = false;

        private Control _ballVisual;

        public override void _Ready()
        {
            // Obtenemos referencia al nodo visual del balón para elevarlo en el eje Y
            _ballVisual = GetNode<Control>("BallVisual");
        }

        // Método público que llamará el Quarterback al lanzar
        public void Launch(Vector2 from, Vector2 to)
        {
            _startPos = from;
            _targetPos = to;
            GlobalPosition = _startPos;

            float distance = _startPos.DistanceTo(_targetPos);
            // Tiempo = Distancia / Velocidad (evitamos división por cero con Mathf.Max)
            _flightDuration = Mathf.Max(distance / FlightSpeed, 0.1f);
            _elapsedTime = 0.0f;
            _isInAir = true;

            // Rotamos el balón para que apunte hacia su destino
            Rotation = (_targetPos - _startPos).Angle();
        }

        public override void _Process(double delta)
        {
            if (!_isInAir) return;

            _elapsedTime += (float)delta;

            // Calculamos el porcentaje del viaje entre 0.0 (inicio) y 1.0 (destino)
            float progress = Mathf.Clamp(_elapsedTime / _flightDuration, 0.0f, 1.0f);

            // 1. Movimiento sobre el suelo (la sombra sigue esta posición)
            GlobalPosition = _startPos.Lerp(_targetPos, progress);

            // 2. Altura parabólica usando Sin(progress * PI) -> sube de 0 a 1 y baja a 0
            float heightZ = Mathf.Sin(progress * Mathf.Pi) * MaxArcHeight;

            // Elevamos visualmente el balón (en Godot 2D, Y negativo es hacia arriba)
            _ballVisual.Position = new Vector2(-7, -4 - heightZ);

            // Escalamos ligeramente el balón según su altura para dar sensación de cercanía a la cámara
            float scaleFactor = 1.0f + (heightZ / MaxArcHeight) * 0.45f;
            _ballVisual.Scale = new Vector2(scaleFactor, scaleFactor);

            // 3. Verificamos si llegó al suelo (pase incompleto si nadie lo atrapó antes)
            if (progress >= 1.0f)
            {
                _isInAir = false;
                GD.Print("¡Pase incompleto! El balón tocó el césped.");
                QueueFree(); // Destruye el balón de la escena
            }
        }
    }
}