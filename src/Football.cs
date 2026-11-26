using Godot;
using System;

namespace CyberDown.Gameplay
{
    public partial class Football : Area2D
    {
        [Export] public float FlightSpeed { get; set; } = 600.0f;
        [Export] public float MaxArcHeight { get; set; } = 85.0f;

        public float CurrentHeightZ { get; private set; } = 0.0f;
        public bool IsInAir { get; private set; } = false;
        public bool IsKick { get; set; } = false;

        private Vector2 _startPos;
        private Vector2 _targetPos;
        private float _flightDuration;
        private float _elapsedTime = 0.0f;
        private float _groundGraceTimer = 0.0f;
        private bool _landed = false;
        private Control _ballVisual;

        public event Action<Vector2> OnBallLanded;

        public override void _Ready()
        {
            _ballVisual = GetNode<Control>("BallVisual");
        }

        public void Launch(Vector2 from, Vector2 to, float customSpeed = 600.0f, float customArcHeight = 85.0f)
        {
            _startPos = from;
            _targetPos = to;
            GlobalPosition = _startPos;

            MaxArcHeight = customArcHeight;
            float distance = _startPos.DistanceTo(_targetPos);
            _flightDuration = Mathf.Max(distance / customSpeed, 0.1f);
            _elapsedTime = 0.0f;
            _groundGraceTimer = 0.0f;
            _landed = false;
            IsInAir = true;

            Rotation = (_targetPos - _startPos).Angle();
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            if (_landed)
            {
                // 0.1 segundos extra en el suelo para que el Area2D del receptor alcance a detectar el balón
                _groundGraceTimer += dt;
                if (_groundGraceTimer >= 0.1f)
                {
                    IsInAir = false;

                    if (IsKick)
                    {
                        OnBallLanded?.Invoke(GlobalPosition);
                    }
                    else
                    {
                        PlayManager.Instance?.EndPlay("Pase incompleto", isIncomplete: true);
                    }

                    GD.Print("El balón tocó el césped.");
                    QueueFree();
                }
                return;
            }

            if (!IsInAir) return;

            _elapsedTime += dt;
            float progress = Mathf.Clamp(_elapsedTime / _flightDuration, 0.0f, 1.0f);

            GlobalPosition = _startPos.Lerp(_targetPos, progress);

            CurrentHeightZ = Mathf.Sin(progress * Mathf.Pi) * MaxArcHeight;
            _ballVisual.Position = new Vector2(-7, -4 - CurrentHeightZ);

            float scaleFactor = 1.0f + (CurrentHeightZ / MaxArcHeight) * 0.45f;
            _ballVisual.Scale = new Vector2(scaleFactor, scaleFactor);

            if (progress >= 1.0f)
            {
                CurrentHeightZ = 0.0f;
                _landed = true;
            }
        }

        public void Catch()
        {
            IsInAir = false;
            _landed = false;
            QueueFree();
        }
    }
}