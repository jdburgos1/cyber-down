using Godot;
using System.Collections.Generic;

namespace CyberDown.Playbook
{
    public class Point2DData
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2 ToVector2() => new Vector2(X, Y);
    }

    public class ReceiverAssignment
    {
        public Point2DData StartOffset { get; set; }
        public List<Point2DData> Waypoints { get; set; } = new();
    }

    public class PlayDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<ReceiverAssignment> Receivers { get; set; } = new();
    }
}