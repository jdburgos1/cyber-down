using Godot;

namespace CyberDown.Players
{
    public partial class QuarterbackController : CharacterBody2D
    {
        // [Export] permite ajustar estos valores desde el Inspector de Godot sin tocar código
        [Export] public float MaxSpeed { get; set; } = 260.0f;
        [Export] public float Acceleration { get; set; } = 1400.0f;
        [Export] public float Friction { get; set; } = 1100.0f;

        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;

            // 1. Obtenemos un vector normalizado (-1 a 1 en X y Y) según las teclas presionadas
            Vector2 inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");

            // 2. Calculamos la velocidad objetivo
            Vector2 targetVelocity = inputDirection * MaxSpeed;

            // 3. Aplicamos aceleración si hay movimiento, o fricción si soltamos las teclas
            if (inputDirection != Vector2.Zero)
            {
                Velocity = Velocity.MoveToward(targetVelocity, Acceleration * dt);
            }
            else
            {
                Velocity = Velocity.MoveToward(Vector2.Zero, Friction * dt);
            }

            // 4. Ejecutamos el movimiento físico de Godot
            MoveAndSlide();
        }
    }
}