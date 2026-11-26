using Godot;
using System;

namespace CyberDown.Gameplay
{
    public enum KickPhase
    {
        Inactive,
        AimingDirection,
        ChargingPower,
        BallInAir
    }

    public partial class SpecialTeamsController : Node2D
    {
        [Export] public PackedScene FootballScene { get; set; }
        [Export] public float MaxKickDistancePixels { get; set; } = 1350.0f; // ~67 yardas
        [Export] public float MaxHorizontalSpread { get; set; } = 320.0f;

        public KickPhase CurrentPhase { get; private set; } = KickPhase.Inactive;

        private float _directionCursor = 0.0f; // Oscila entre -1.0 (izq) y +1.0 (der)
        private float _directionSign = 1.0f;
        private float _powerValue = 0.0f;      // Sube de 0.0 a 1.0
        private float _powerSign = 1.0f;

        private Action<Football> _onBallKicked;
        private Action<Vector2> _onBallLandedCallback;
        private Vector2 _kickOrigin;
        private Action<Vector2> _onKickLaunchedCallback;

        public void BeginKickoff(Vector2 origin, Action<Football> onBallKicked, Action<Vector2> onBallLanded)
        {
            _kickOrigin = origin;
            GlobalPosition = origin;
            _directionCursor = 0.0f;
            _powerValue = 0.0f;
            CurrentPhase = KickPhase.AimingDirection;
            _onBallKicked = onBallKicked;
            _onBallLandedCallback = onBallLanded;
            Visible = true;
            GD.Print("KICKOFF: Presiona [ESPACIO] para fijar dirección.");
        }


        public override void _Process(double delta)
        {
            if (CurrentPhase == KickPhase.Inactive || CurrentPhase == KickPhase.BallInAir) return;

            float dt = (float)delta;

            if (CurrentPhase == KickPhase.AimingDirection)
            {
                _directionCursor += _directionSign * 2.4f * dt;
                if (_directionCursor >= 1.0f) { _directionCursor = 1.0f; _directionSign = -1.0f; }
                else if (_directionCursor <= -1.0f) { _directionCursor = -1.0f; _directionSign = 1.0f; }
            }
            else if (CurrentPhase == KickPhase.ChargingPower)
            {
                _powerValue += _powerSign * 1.8f * dt;
                if (_powerValue >= 1.0f) { _powerValue = 1.0f; _powerSign = -1.0f; }
                else if (_powerValue <= 0.1f) { _powerValue = 0.1f; _powerSign = 1.0f; }
            }

            QueueRedraw(); // Redibuja el medidor en tiempo real
        }


        public override void _Input(InputEvent @event)
        {
            if (CurrentPhase == KickPhase.Inactive || CurrentPhase == KickPhase.BallInAir) return;

            if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Space)
            {
                GetViewport().SetInputAsHandled(); // Evita que Espacio dispare otra acción en PlayManager

                if (CurrentPhase == KickPhase.AimingDirection)
                {
                    CurrentPhase = KickPhase.ChargingPower;
                    GD.Print($"Dirección fijada ({_directionCursor:F2}). Presiona [ESPACIO] para POTENCIA.");
                }
                else if (CurrentPhase == KickPhase.ChargingPower)
                {
                    ExecuteKick();
                }
            }
        }


        private void ExecuteKick()
        {
            CurrentPhase = KickPhase.BallInAir;
            Visible = false;

            // Ajustamos distancia realista de Kickoff (entre 35 y 60 yardas = 700 a 1200 px)
            float kickDistance = Mathf.Lerp(700.0f, 1200.0f, _powerValue);
            float targetX = Mathf.Clamp(_kickOrigin.X + (_directionCursor * MaxHorizontalSpread), 60.0f, 1220.0f);
            float targetY = Mathf.Max(_kickOrigin.Y - kickDistance, 40.0f);
            Vector2 landingPoint = new Vector2(targetX, targetY);

            if (FootballScene != null)
            {
                Football ball = FootballScene.Instantiate<Football>();
                ball.IsKick = true;
                GetParent().AddChild(ball);
                ball.IsKick = true;
                ball.Launch(_kickOrigin, landingPoint, customSpeed: 500.0f, customArcHeight: 185.0f);

                // 1. Avisamos a PlayManager que el balón ya va en el aire para que la cámara lo siga
                _onBallKicked?.Invoke(ball);

                // 2. Esperamos a que el balón aterrice de verdad antes de mover a los jugadores
                ball.OnBallLanded += (landPos) =>
                {
                    CurrentPhase = KickPhase.Inactive;
                    _onBallLandedCallback?.Invoke(landPos);
                };
            }

            GD.Print($"¡PATADA! Potencia: {(int)(_powerValue * 100)}%");
        }

        // Dibujo vectorial limpio del medidor de patada directamente sobre el césped
        public override void _Draw()
        {
            if (CurrentPhase != KickPhase.AimingDirection && CurrentPhase != KickPhase.ChargingPower) return;

            // Barra horizontal de dirección
            DrawRect(new Rect2(-120, -50, 240, 14), new Color(0, 0, 0, 0.7f));
            DrawRect(new Rect2(-15, -52, 30, 18), new Color("#00ff66")); // Zona dulce central
            float cursorX = _directionCursor * 120.0f;
            DrawRect(new Rect2(cursorX - 4, -56, 8, 26), new Color("#00f0ff"));

            // Barra de potencia (visible al fijar dirección)
            if (CurrentPhase == KickPhase.ChargingPower)
            {
                DrawRect(new Rect2(-120, -28, 240, 14), new Color(0, 0, 0, 0.7f));
                DrawRect(new Rect2(-120, -28, 240 * _powerValue, 14), new Color("#ff7b00"));
            }
        }


        public void BeginFieldGoal(Vector2 origin, Action<Football> onBallKicked, Action<bool> onFieldGoalResolved)
        {
            _kickOrigin = origin;
            GlobalPosition = origin;
            _directionCursor = 0.0f;
            _powerValue = 0.0f;
            CurrentPhase = KickPhase.AimingDirection;
            _onBallKicked = onBallKicked;
            _onBallLandedCallback = (landPos) =>
            {
                CurrentPhase = KickPhase.Inactive;
                // Es bueno si cruzó la línea de los postes (Y <= 125) y quedó dentro del ancho (540 a 740)
                bool isGood = landPos.Y <= 125.0f && landPos.X >= 540.0f && landPos.X <= 740.0f;
                onFieldGoalResolved?.Invoke(isGood);
            };
            Visible = true;
            GD.Print("FIELD GOAL: Presiona [ESPACIO] para dirección y luego potencia hacia los postes dorados.");
        }
    }
}