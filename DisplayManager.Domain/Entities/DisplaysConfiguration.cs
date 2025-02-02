namespace DisplayManager.Domain.Entities;

public class DisplaysConfiguration
{
    public string ConfigName { get; set; } = string.Empty;
    public bool IsDefaultConfig{ get; set; } = false;
    public List<Screen> Screens { get; set; }

    public DisplaysConfiguration() { }

    public DisplaysConfiguration(string name, bool defaultConfig, List<Screen> screens)
    {
        ConfigName = name;
        IsDefaultConfig = defaultConfig;
        Screens = screens;
    }
}
