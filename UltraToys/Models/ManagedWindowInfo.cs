namespace UltraToys.Models;

public class ManagedWindowInfo
{
    public required IntPtr Hwnd { get; init; }
    public required string Title { get; init; }
    public required string ProcessName { get; init; }
}
