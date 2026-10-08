using System.Text.Json;
using DesktopBoxLY.Models;

namespace DesktopBoxLY.Services;

public static class BoxStateStore
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopBoxLY");
    private static readonly string StatePath = Path.Combine(DirectoryPath, "boxes.json");
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static List<BoxConfig> Load()
    {
        try
        {
            if (!File.Exists(StatePath)) return [];
            return JsonSerializer.Deserialize<List<BoxConfig>>(File.ReadAllText(StatePath)) ?? [];
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    public static void Save(IEnumerable<BoxConfig> boxes)
    {
        Directory.CreateDirectory(DirectoryPath);
        var tempPath = StatePath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(boxes, Options));
        File.Move(tempPath, StatePath, overwrite: true);
    }
}
