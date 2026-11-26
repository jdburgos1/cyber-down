using Godot;
using System.Collections.Generic;
using System.Text.Json;

namespace CyberDown.Playbook
{
    public static class PlaybookRepository
    {
        public static List<PlayDefinition> LoadPlays(string path = "res://data/playbook/plays.json")
        {
            if (!FileAccess.FileExists(path))
            {
                GD.PrintErr($"No se encontró el archivo del Playbook en: {path}");
                return new List<PlayDefinition>();
            }

            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            string jsonText = file.GetAsText();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<PlayDefinition>>(jsonText, options) ?? new List<PlayDefinition>();
        }
    }
}