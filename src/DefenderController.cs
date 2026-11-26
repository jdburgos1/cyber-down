using Godot;
using CyberDown.Gameplay;

namespace CyberDown.Players
{
    public partial class DefenderController : CharacterBody2D
    {
        [Export] public float CoverageSpeed { get; set; } = 165.0f;
        [Export] public float PursuitSpeed { get; set; } = 235.0f;
        [Export] public Node2D TargetReceiver { get; set; }
        [Export] public Node2D QuarterbackNode { get; set; }

        private readonly Vector2 _coverageOffset = new Vector2(0, -80);
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

            if (state == PlayState.PreSnap || state == PlayState.PlayDead)
            {
                Velocity = Vector2.Zero;
                return;
            }

            // 1. Mientras el pase no se haya completado (InPlay)
            if (state == PlayState.InPlay)
            {
                // Si el QB cruza la línea de scrimmage corriendo con el balón, el defensa abandona cobertura y lo ataca
                bool qbIsRunningPastLos = QuarterbackNode != null &&
                                          QuarterbackNode.GlobalPosition.Y < PlayManager.Instance.Rules.LineOfScrimmageY;

                if (qbIsRunningPastLos)
                {
                    if (_tackleZone.OverlapsBody(QuarterbackNode) || GlobalPosition.DistanceTo(QuarterbackNode.GlobalPosition) <= 36.0f)
                    {
                        PlayManager.Instance.EndPlay("¡SACK! Quarterback tacleado.");
                        return;
                    }
                    MoveTowardsPoint(QuarterbackNode.GlobalPosition, PursuitSpeed);
                }
                else
                {
                    // Cobertura normal sobre el receptor
                    Vector2 guardPoint = TargetReceiver.GlobalPosition + _coverageOffset;
                    MoveTowardsPoint(guardPoint, CoverageSpeed);
                }
            }
            // 2. Si el receptor atrapó el balón (BallCaught), lo persigue y taclea
            else if (state == PlayState.BallCaught)
            {
                if (_tackleZone.OverlapsBody(TargetReceiver))
                {
                    PlayManager.Instance.EndPlay("¡CRACK! Tacleada completada.");
                    return;
                }

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
            if (PlayManager.Instance == null) return;

            PlayState state = PlayManager.Instance.CurrentState;

            // Tacleada al Receptor cuando tiene el balón
            if (state == PlayState.BallCaught && body == TargetReceiver)
            {
                PlayManager.Instance.EndPlay("¡CRACK! Tacleada completada.");
            }
            // Tacleada / Sack al Quarterback si sigue con el balón en InPlay
            else if (state == PlayState.InPlay && body == QuarterbackNode)
            {
                PlayManager.Instance.EndPlay("¡SACK! Quarterback tacleado.");
            }
        }

        public void ResetPlayer()
        {
            Velocity = Vector2.Zero;
        }
    }
}