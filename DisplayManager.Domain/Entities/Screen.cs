namespace DisplayManager.Domain.Entities;

public class Screen
{
    public string DeviceName { get; set; }
    public string DeviceString { get; set; }
    public string DeviceID { get; set; }
    public string DeviceKey { get; set; }
    public int StateFlags { get; set; }
    public int PositionX { get; set; } = 0;
    public int PositionY { get; set; } = 0;
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;
    public bool IsActive => (StateFlags & DISPLAY_DEVICE_ACTIVE) != 0;
    public bool IsPrimary => (StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0;

    public string DisplayMode { get; set; }

    public const int DISPLAY_DEVICE_ACTIVE = 0x1;
    public const int DISPLAY_DEVICE_PRIMARY_DEVICE = 0x4;
}
