namespace DesktopBoxLY.Models;

public sealed class BoxConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "新盒子";
    public string FolderPath { get; set; } = string.Empty;
    public int X { get; set; } = 80;
    public int Y { get; set; } = 100;
    public int Width { get; set; } = 360;
    public int Height { get; set; } = 460;
    public double Opacity { get; set; } = 0.94;
}
