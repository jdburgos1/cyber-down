using Godot;
using CyberDown.Gameplay; // Importamos el namespace donde está Football

namespace CyberDown.Players
{
    public partial class QuarterbackController : CharacterBody2D
    {
        [Export] public float MaxSpeed { get; set; } = 260.0f;
        [Export] public float Acceleration { get; set; } = 1400.0f;
        [Export] public float Friction { get; set; } = 1100.0f;

        // Arrastraremos aquí la escena Football.tscn desde el Inspector
        [Export] public PackedScene FootballScene { get; set; }

        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;
            Vector2 inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            Vector2 targetVelocity = inputDirection * MaxSpeed;

            if (inputDirection != Vector2.Zero)
                Velocity = Velocity.MoveToward(targetVelocity, Acceleration * dt);
            else
                Velocity = Velocity.MoveToward(Vector2.Zero, Friction * dt);

            MoveAndSlide();
        }

        // _UnhandledInput detecta clics o teclas que no fueron capturados por botones de interfaz
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                ThrowBall(GetGlobalMousePosition());
            }
        }

        private void ThrowBall(Vector2 targetPosition)
        {
            if (FootballScene == null)
            {
                GD.PrintErr("¡Falta asignar FootballScene en el Inspector del Quarterback!");
                return;
            }

            // Instanciamos una copia del balón y la añadimos a la escena principal (GetParent())
            Football ball = FootballScene.Instantiate<Football>();
            GetParent().AddChild(ball);
            ball.Launch(GlobalPosition, targetPosition);
        }
    }
}