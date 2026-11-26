using Godot;
using System.Collections.Generic;
using System.Linq;
using CyberDown.Rules;
using CyberDown.Players;
using CyberDown.Playbook;

namespace CyberDown.Gameplay
{
    public enum PlayState
    {
        PreSnap,
        Kickoff,
        InPlay,
        BallCaught,
        PlayDead
    }

    public partial class PlayManager : Node
    {
        public static PlayManager Instance { get; private set; }
        public PlayState CurrentState { get; private set; } = PlayState.PreSnap;
        public GameRulesEngine Rules { get; private set; }

        [Export] public Node2D QuarterbackNode { get; set; }
        [Export] public Godot.Collections.Array<WideReceiverController> Receivers { get; set; } = new();
        [Export] public Godot.Collections.Array<DefenderController> Defenders { get; set; } = new();
        [Export] public Camera2D MainCamera { get; set; }

        [Export] public ColorRect LineOfScrimmageVisual { get; set; }
        [Export] public ColorRect FirstDownLineVisual { get; set; }
        [Export] public SpecialTeamsController SpecialTeams { get; set; }

        private Node2D _cameraTarget;
        private Node2D _activeBallCarrier;
        private float _furthestBallY;

        private List<PlayDefinition> _playbook = new();
        private int _selectedPlayIndex = 0;

        private float _qbOffsetFromLos = 80.0f;
        private float _defOffsetFromLos = -180.0f;

        public Node2D ActiveBallCarrier => _activeBallCarrier;


        public override void _EnterTree()
        {
            Instance = this;
            Rules = new GameRulesEngine(2000.0f);
            _playbook = PlaybookRepository.LoadPlays();
        }


        public override void _Ready()
        {
            ResetPlay();
        }


        public override void _Process(double delta)
        {
            float dt = (float)delta;

            if (MainCamera != null && _cameraTarget != null)
            {
                MainCamera.GlobalPosition = _cameraTarget.GlobalPosition;
            }

            if (CurrentState == PlayState.InPlay || CurrentState == PlayState.BallCaught)
            {
                Rules.TickClock(dt);

                Node2D carrier = (CurrentState == PlayState.BallCaught && _activeBallCarrier != null)
                    ? _activeBallCarrier
                    : QuarterbackNode;

                if (carrier != null)
                {
                    if (carrier.GlobalPosition.Y < _furthestBallY)
                        _furthestBallY = carrier.GlobalPosition.Y;

                    if (_furthestBallY <= GameRulesEngine.OpponentEndZoneY)
                    {
                        EndPlay("Touchdown", isIncomplete: false);
                        return;
                    }

                    if (carrier.GlobalPosition.X <= 14.0f || carrier.GlobalPosition.X >= 1266.0f)
                    {
                        EndPlay("Fuera de banda (Out of Bounds)", isIncomplete: false);
                    }
                }
            }
        }


        public override void _Input(InputEvent @event)
        {
            if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
                return;

            if (CurrentState == PlayState.PreSnap)
            {
                if (keyEvent.Keycode == Key.Key1) SelectPlay(0);
                else if (keyEvent.Keycode == Key.Key2) SelectPlay(1);
                else if (keyEvent.Keycode == Key.Key3) SelectPlay(2);
                else if (keyEvent.Keycode == Key.K) StartKickoff();
                else if (keyEvent.Keycode == Key.G) StartFieldGoal();
                else if (keyEvent.Keycode == Key.Space || @event.IsActionPressed("snap_ball"))
                    StartSnap();
            }
            else if (CurrentState == PlayState.PlayDead && keyEvent.Keycode == Key.R)
            {
                ResetPlay();
            }
        }


        public void SelectPlay(int index)
        {
            if (index < 0 || index >= _playbook.Count) return;
            _selectedPlayIndex = index;
            ApplyPlayFormation();
            GD.Print($"JUGADA SELECCIONADA: {_playbook[_selectedPlayIndex].Name}");
        }


        public void StartSnap()
        {
            CurrentState = PlayState.InPlay;
            _activeBallCarrier = QuarterbackNode;
            _furthestBallY = Rules.LineOfScrimmageY;
            GD.Print($"¡SNAP! Ejecutando: {(_playbook.Count > 0 ? _playbook[_selectedPlayIndex].Name : "Jugada Base")}");
        }


        public void OnPassCompleted(Node2D receiver)
        {
            _activeBallCarrier = receiver;
            _cameraTarget = receiver;

            if (receiver.GlobalPosition.Y < _furthestBallY)
                _furthestBallY = receiver.GlobalPosition.Y;

            if (_furthestBallY <= GameRulesEngine.OpponentEndZoneY)
            {
                EndPlay("Touchdown", isIncomplete: false);
                return;
            }

            CurrentState = PlayState.BallCaught;
            GD.Print("¡PASE COMPLETO! Corre con el receptor.");
        }


        public void EndPlay(string reason, bool isIncomplete = false)
        {
            if (CurrentState == PlayState.PlayDead) return;

            CurrentState = PlayState.PlayDead;
            PlayResult result = Rules.ResolvePlay(_furthestBallY, isIncomplete);

            GD.Print($"--- {reason.ToUpper()} ---");
            GD.Print(result.Message);
            GD.Print("Presiona [R] para alinear la siguiente jugada.");
        }


        public void ResetPlay()
        {
            CurrentState = PlayState.PreSnap;
            _cameraTarget = QuarterbackNode;
            _activeBallCarrier = QuarterbackNode;

            float losY = Rules.LineOfScrimmageY;

            if (QuarterbackNode != null)
            {
                QuarterbackNode.GlobalPosition = new Vector2(640, losY + _qbOffsetFromLos);
                QuarterbackNode.Call("ResetPlayer");
            }

            ApplyPlayFormation();

            if (LineOfScrimmageVisual != null)
                LineOfScrimmageVisual.Position = new Vector2(0, losY);

            if (FirstDownLineVisual != null)
            {
                FirstDownLineVisual.Position = new Vector2(0, Rules.FirstDownLineY);
                FirstDownLineVisual.Visible = Rules.FirstDownLineY > GameRulesEngine.OpponentEndZoneY;
            }

            if (MainCamera != null && _cameraTarget != null)
            {
                MainCamera.GlobalPosition = _cameraTarget.GlobalPosition;
                MainCamera.ResetSmoothing();
            }

            string currentPlayName = _playbook.Count > 0 ? _playbook[_selectedPlayIndex].Name : "Sin Playbook";
            GD.Print($"PRE-SNAP | {Rules.GetSituationSummary()} | Jugada: {currentPlayName} (Teclas 1-3 para cambiar, ESPACIO para Snap)");
        }


        private void ApplyPlayFormation()
        {
            float losY = Rules.LineOfScrimmageY;
            PlayDefinition play = (_playbook.Count > _selectedPlayIndex) ? _playbook[_selectedPlayIndex] : null;

            for (int i = 0; i < Receivers.Count; i++)
            {
                var wr = Receivers[i];
                if (wr == null) continue;

                if (play != null && i < play.Receivers.Count)
                {
                    var assignment = play.Receivers[i];
                    wr.GlobalPosition = new Vector2(640 + assignment.StartOffset.X, losY + assignment.StartOffset.Y);
                    wr.ResetPlayer();
                    wr.AssignRoute(assignment.Waypoints.Select(p => p.ToVector2()).ToArray());
                }

                // Alineamos al defensor correspondiente frente a este receptor
                if (i < Defenders.Count && Defenders[i] != null)
                {
                    Defenders[i].GlobalPosition = new Vector2(wr.GlobalPosition.X, losY + _defOffsetFromLos);
                    Defenders[i].ResetPlayer();
                }
            }
        }


        public void StartKickoff()
        {
            if (SpecialTeams == null || QuarterbackNode == null) return;

            CurrentState = PlayState.Kickoff;
            Vector2 kickPos = new Vector2(640, 1700); // Yarda 25 propia (más espacio hacia arriba)
            QuarterbackNode.GlobalPosition = kickPos;
            _cameraTarget = QuarterbackNode;

            if (MainCamera != null)
                MainCamera.Zoom = new Vector2(0.55f, 0.55f);

            SpecialTeams.BeginKickoff(
                kickPos,
                onBallKicked: (ballInAir) =>
                {
                    // La cámara sigue el trayecto del balón por toda la cancha
                    _cameraTarget = ballInAir;
                },
                onBallLanded: (landingPoint) =>
                {
                    // Si cae dentro de la endzone rival (Y <= 200), es Touchback (arranca en yarda 25 = Y 1700)
                    float newScrimmageY = (landingPoint.Y <= GameRulesEngine.OpponentEndZoneY)
                        ? 1700.0f
                        : Mathf.Clamp(landingPoint.Y, 300.0f, 1800.0f);

                    Rules.ResetDrive(newScrimmageY);

                    if (MainCamera != null)
                        MainCamera.Zoom = new Vector2(0.8f, 0.8f);

                    ResetPlay();
                }
            );
        }


        public void StartFieldGoal()
        {
            if (SpecialTeams == null || QuarterbackNode == null) return;

            CurrentState = PlayState.Kickoff; // Reutiliza el estado de patada
            Vector2 kickPos = new Vector2(640, Rules.LineOfScrimmageY + 140.0f); // 7 yardas atrás de la línea
            QuarterbackNode.GlobalPosition = kickPos;
            _cameraTarget = QuarterbackNode;

            if (MainCamera != null)
                MainCamera.Zoom = new Vector2(0.55f, 0.55f);

            SpecialTeams.BeginFieldGoal(
                kickPos,
                onBallKicked: (ballInAir) => { _cameraTarget = ballInAir; },
                onFieldGoalResolved: (isGood) =>
                {
                    PlayResult res = Rules.ResolveFieldGoal(isGood);
                    GD.Print(res.Message);

                    if (MainCamera != null)
                        MainCamera.Zoom = new Vector2(0.8f, 0.8f);

                    ResetPlay();
                }
            );
        }
    }
}