using Godot;
using CyberDown.Gameplay;

namespace CyberDown.Players
{
    public partial class DefenderController : CharacterBody2D
    {
        [Export] public float CoverageSpeed { get; set; } = 165.0f; // Ligeramente menor al WR para premiar buenos cortes
        [Export] public float PursuitSpeed { get; set; } = 235.0f;  // Sprint de persecución tras la atrapada
        [Export] public Node2D TargetReceiver { get; set; }

        // Desfase táctico: cubre al receptor 45px por delante (hacia arriba) para vigilar el pase profundo
        private readonly Vector2 _coverageOffset = new Vector2(0, -45);
        private Area2D _tackleZone;

        public override void _Ready()
        {
            _tackleZone = GetNode<Area2D>("TackleZone");
            _tackleZone.BodyEntered += OnBodyEnteredTackleZone;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (PlayManager.Instance == null || TargetReceiver == null) return;

            PlayState state = PlayManager.Instance.CurrentState;

            // Quieto antes del Snap o cuando la jugada ya murió
            if (state == PlayState.PreSnap || state == PlayState.PlayDead)
            {
                Velocity = Vector2.Zero;
                return;
            }

            // 1. Cobertura hombre a hombre mientras el balón no ha sido atrapado
            if (state == PlayState.InPlay)
            {
                Vector2 guardPoint = TargetReceiver.GlobalPosition + _coverageOffset;
                MoveTowardsPoint(guardPoint, CoverageSpeed);
            }
            // 2. Persecución directa al portador tras atrapar el balón
            else if (state == PlayState.BallCaught)
            {
                MoveTowardsPoint(TargetReceiver.GlobalPosition, PursuitSpeed);
            }
        }

        private void MoveTowardsPoint(Vector2 destination, float speed)
        {
            if (GlobalPosition.DistanceTo(destination) < 6.0f)
            {
                Velocity = Vector2.Zero;
                return;
            }

            Vector2 direction = GlobalPosition.DirectionTo(destination);
            Velocity = direction * speed;
            MoveAndSlide();
        }

        private void OnBodyEnteredTackleZone(Node2D body)
        {
            // Solo puede taclear si el balón ya fue atrapado y el cuerpo que tocó es el Receptor con balón
            if (PlayManager.Instance != null &&
                PlayManager.Instance.CurrentState == PlayState.BallCaught &&
                body == TargetReceiver)
            {
                PlayManager.Instance.EndPlay("¡CRACK! Tacleada completada.");
            }
        }

        public void ResetPlayer()
        {
            Velocity = Vector2.Zero;
        }
    }
}