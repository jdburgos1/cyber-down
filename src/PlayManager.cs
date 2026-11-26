using Godot;
using CyberDown.Rules;

namespace CyberDown.Gameplay
{
    // Definimos los estados posibles de una jugada
    public enum PlayState
    {
        PreSnap,    // Esperando el Snap en la línea
        InPlay,     // Jugada en curso (QB tiene el balón o está en el aire)
        BallCaught,  // El receptor atrapó el balón y ahora tú lo controlas
        PlayDead    // Jugada terminada (tacleada o pase incompleto)
    }

    public partial class PlayManager : Node
    {
        // Singleton ligero para que QB y Receptor consulten el estado fácilmente
        public static PlayManager Instance { get; private set; }
        public PlayState CurrentState { get; private set; } = PlayState.PreSnap;
        public GameRulesEngine Rules { get; private set; }

        // Referencias a los actores en escena que conectaremos en el Inspector
        [Export] public Node2D QuarterbackNode { get; set; }
        [Export] public Node2D ReceiverNode { get; set; }
        [Export] public Node2D DefenderNode { get; set; }
        [Export] public Camera2D MainCamera { get; set; }

        // Lineas visuales en la cancha
        [Export] public ColorRect LineOfScrimmageVisual { get; set; }
        [Export] public ColorRect FirstDownLineVisual { get; set; }

        private Node2D _cameraTarget;
        private float _furthestBallY; // Guardamos la posición más avanzada del balón para respetar el Forward Progress

        // Posiciones relativas de cada jugador 
        private float _qbOffsetFromLos = 80.0f; // QB está a 4 yardas detrás de la línea de scrimmage
        private float _wrOffsetFromLos = 0.0f; // WR inicia junto la línea
        private float _defOffsetFromLos = -180.0f; // DEF empieza 6 yardas adelante de la línea

        public override void _EnterTree()
        {
            Instance = this;
            Rules = new GameRulesEngine(2000.0f); // Iniciamos en la yarda 10 propia
        }


        public override void _Ready()
        {
            ResetPlay();
        }


        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // 1. La cámara siempre sigue al objetivo activo
            if (MainCamera != null && _cameraTarget != null)
            {
                MainCamera.GlobalPosition = _cameraTarget.GlobalPosition;
            }

            // 2. El reloj, el avance máximo y la detección de Touchdown SOLO corren mientras la jugada está viva
            if (CurrentState == PlayState.InPlay || CurrentState == PlayState.BallCaught)
            {
                Rules.TickClock(dt);

                // Determinamos quién tiene el balón (el Receptor si ya atrapó, o el QB si aún no lanza)
                float currentCarrierY = (CurrentState == PlayState.BallCaught && ReceiverNode != null)
                    ? ReceiverNode.GlobalPosition.Y
                    : (QuarterbackNode != null ? QuarterbackNode.GlobalPosition.Y : Rules.LineOfScrimmageY);

                // Guardamos el punto más lejano (Y menor) alcanzado para respetar el Forward Progress
                if (currentCarrierY < _furthestBallY)
                {
                    _furthestBallY = currentCarrierY;
                }

                // Si el portador activo (QB o Receptor) cruza la línea de anotación (Y <= 200), es Touchdown
                if (_furthestBallY <= GameRulesEngine.OpponentEndZoneY)
                {
                    EndPlay("Touchdown", isIncomplete: false);
                    return;
                }

                float currentCarrierX = (CurrentState == PlayState.BallCaught && ReceiverNode != null)
                    ? ReceiverNode.GlobalPosition.X
                    : (QuarterbackNode != null ? QuarterbackNode.GlobalPosition.X : 640.0f);

                if (currentCarrierX <= 14.0f || currentCarrierX >= 1266.0f)
                {
                    EndPlay("Fuera de banda (Out of Bounds)", isIncomplete: false);
                }
            }
        }


        public override void _Input(InputEvent @event)
        {
            bool isSpacePressed = @event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.Space;
            bool isResetPressed = @event is InputEventKey resetEvent && resetEvent.Pressed && !resetEvent.Echo && resetEvent.Keycode == Key.R;

            if (CurrentState == PlayState.PreSnap && (@event.IsActionPressed("snap_ball") || isSpacePressed))
            {
                StartSnap();
            }
            else if (CurrentState == PlayState.PlayDead && isResetPressed)
            {
                ResetPlay();
            }
        }


        public void StartSnap()
        {
            CurrentState = PlayState.InPlay;
            _furthestBallY = Rules.LineOfScrimmageY;
            GD.Print("¡SNAP! Jugada en marcha.");
        }


        // Método que llamará el Receptor cuando atrape el balón
        public void OnPassCompleted(Node2D receiver)
        {
            _cameraTarget = receiver;

            // Actualizamos el avance máximo con el punto exacto donde atrapó el balón
            if (receiver.GlobalPosition.Y < _furthestBallY)
                _furthestBallY = receiver.GlobalPosition.Y;

            // Si atrapó el balón ya dentro de la zona de anotación, es TOUCHDOWN inmediato
            if (_furthestBallY <= GameRulesEngine.OpponentEndZoneY)
            {
                EndPlay("Touchdown", isIncomplete: false);
                return;
            }

            CurrentState = PlayState.BallCaught;
            GD.Print("¡PASE COMPLETO! Corre hacia la línea amarilla o hasta la zona verde de Touchdown.");
        }


        // Se llama cuando hay tacleada o cuando el balón cae incompleto
        public void EndPlay(string reason, bool isIncomplete = false)
        {
            if (CurrentState == PlayState.PlayDead) return;

            CurrentState = PlayState.PlayDead;

            // Determinamos dónde quedó el balón (donde cayó el receptor o en la línea si fue incompleto)
            float ballEndY = (ReceiverNode != null) ? ReceiverNode.GlobalPosition.Y : Rules.LineOfScrimmageY;
            PlayResult result = Rules.ResolvePlay(_furthestBallY, isIncomplete);

            GD.Print($"--- {reason.ToUpper()} ---");
            GD.Print(result.Message);
            GD.Print("Presiona [R] para alinear la siguiente jugada.");
        }


        public void ResetPlay()
        {
            CurrentState = PlayState.PreSnap;
            _cameraTarget = QuarterbackNode;

            float losY = Rules.LineOfScrimmageY;

            // Alineamos a todos los jugadores respecto a la nueva Línea de Scrimmage
            if (QuarterbackNode != null)
            {
                QuarterbackNode.GlobalPosition = new Vector2(640, losY + _qbOffsetFromLos);
                QuarterbackNode.Call("ResetPlayer");
            }

            if (ReceiverNode != null)
            {
                ReceiverNode.GlobalPosition = new Vector2(1120, losY + _wrOffsetFromLos);
                ReceiverNode.Call("ResetPlayer");
            }

            if (DefenderNode != null)
            {
                DefenderNode.GlobalPosition = new Vector2(640, losY + _defOffsetFromLos);
                DefenderNode.Call("ResetPlayer");
            }

            // Movemos las líneas azul (Scrimmage) y amarilla (First Down) en la cancha
            if (LineOfScrimmageVisual != null)
                LineOfScrimmageVisual.Position = new Vector2(0, losY);

            if (FirstDownLineVisual != null)
                FirstDownLineVisual.Position = new Vector2(0, Rules.FirstDownLineY);
                FirstDownLineVisual.Visible = Rules.FirstDownLineY > GameRulesEngine.OpponentEndZoneY;

            if (MainCamera != null && _cameraTarget != null)
            {
                MainCamera.GlobalPosition = _cameraTarget.GlobalPosition;
                MainCamera.ResetSmoothing();
            }

            GD.Print($"PRE-SNAP | {Rules.GetSituationSummary()} | Presiona [ESPACIO]");
        }
    }

} 