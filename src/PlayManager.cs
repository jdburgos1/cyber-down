using Godot;

namespace CyberDown.Gameplay
{
    // Definimos los estados posibles de una jugada
    public enum PlayState
    {
        PreSnap,    // Esperando el Snap en la línea
        InPlay,     // Jugada en curso (QB tiene el balón o está en el aire)
        BallCaught  // El receptor atrapó el balón y ahora tú lo controlas
    }

    public partial class PlayManager : Node
    {
        // Singleton ligero para que QB y Receptor consulten el estado fácilmente
        public static PlayManager Instance { get; private set; }

        public PlayState CurrentState { get; private set; } = PlayState.PreSnap;

        // Referencias a los actores en escena que conectaremos en el Inspector
        [Export] public Node2D QuarterbackNode { get; set; }
        [Export] public Node2D ReceiverNode { get; set; }
        [Export] public Camera2D MainCamera { get; set; }

        private Node2D _cameraTarget;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            CurrentState = PlayState.PreSnap;
            _cameraTarget = QuarterbackNode;

            if (MainCamera != null && _cameraTarget != null)
            {
                MainCamera.GlobalPosition = _cameraTarget.GlobalPosition;
                MainCamera.ResetSmoothing();
            }

            GD.Print("Estado: PRE-SNAP. Presiona ESPACIO para iniciar la jugada.");
        }

        public override void _Process(double delta)
        {
            // La cámara sigue siempre al objetivo activo; el Smoothing nativo de Camera2D suaviza el viaje
            if (MainCamera != null && _cameraTarget != null)
            {
                MainCamera.GlobalPosition = _cameraTarget.GlobalPosition;
            }
        }

        public override void _Input(InputEvent @event)
        {
            bool isSpacePressed = @event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.Space;

            if (CurrentState == PlayState.PreSnap && (@event.IsActionPressed("snap_ball") || isSpacePressed))
            {
                StartSnap();
            }
        }

        public void StartSnap()
        {
            CurrentState = PlayState.InPlay;
            GD.Print("¡SNAP! Jugada en marcha.");
        }

        // Método que llamará el Receptor cuando atrape el balón
        public void OnPassCompleted(Node2D receiver)
        {
            CurrentState = PlayState.BallCaught;
            _cameraTarget = receiver; // Solo cambiamos el objetivo; cero saltos ni reparenting
            GD.Print("¡Control transferido al Receptor! Corre hacia arriba.");
        }
    }

} 