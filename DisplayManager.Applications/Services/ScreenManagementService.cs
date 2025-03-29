using DisplayManager.Domain.Entities;
using DisplayManager.Domain.Interfaces;
using DisplayManager.Infrastructure.WindowsDisplayAPI;

namespace DisplayManager.Applications.Services;

public class ScreenManagementService : IScreenManagementService
{
    private readonly WindowsDisplayApiWrapper _windowsDisplayApiWrapper;

    public ScreenManagementService()
    {
        _windowsDisplayApiWrapper = new WindowsDisplayApiWrapper();
    }

    public List<DisplaysConfiguration> DetectConnectedScreens()
    {
        List<DisplaysConfiguration> displaysConfig = GetDisplayDevices();
        PrintDisplayInfo(displaysConfig);
        /*_windowsDisplayApiWrapper.DetectDisplaysWMI();
        _windowsDisplayApiWrapper.DetectDevices();*/
        return displaysConfig;
    }

    private List<DisplaysConfiguration> GetDisplayDevices()
    {
        DisplaysConfiguration display = new()
        {
            ConfigName = "Default Configuration", // Assurez-vous de donner un nom
            IsDefaultConfig = false, // Assumer une valeur par défaut ou déduire selon votre logique
            Screens = _windowsDisplayApiWrapper.DetectDisplays()
        };

        // Créer une liste de configurations d'écrans et ajouter la configuration ci-dessus
        List<DisplaysConfiguration> displaysConfig = new();
        displaysConfig.Add(display);

        return displaysConfig;
    }

    public void PrintDisplayInfo(List<DisplaysConfiguration> displaysConfig)
    {
        var displays = displaysConfig.First().Screens;
        foreach (var display in displays)
        {
            System.Diagnostics.Debug.WriteLine($"Device Name: {display.DeviceName}");
            System.Diagnostics.Debug.WriteLine($"Device String: {display.DeviceString}");
            System.Diagnostics.Debug.WriteLine($"Device ID: {display.DeviceID}");
            System.Diagnostics.Debug.WriteLine($"Device Key: {display.DeviceKey}");
            System.Diagnostics.Debug.WriteLine($"State Flags: {Convert.ToString(display.StateFlags, 2).PadLeft(32, '0')}");
            System.Diagnostics.Debug.WriteLine($"Active: {display.IsActive}");
            System.Diagnostics.Debug.WriteLine($"Primary: {display.IsPrimary}");
            System.Diagnostics.Debug.WriteLine($"Position: ({display.PositionX}, {display.PositionY})");
            System.Diagnostics.Debug.WriteLine($"Resolution: {display.Width}x{display.Height}");
            System.Diagnostics.Debug.WriteLine($"Display Mode: {display.DisplayMode}");
            System.Diagnostics.Debug.WriteLine("---------------------------------------------------");
        }
    }

    public void ActivateScreen(int screenId)
    {
        // Implémentez la logique pour activer un écran
        // Ceci pourrait impliquer de modifier la configuration d'affichage Windows
    }

    public void DesactivateScreen(int screenId)
    {
        // Implémentez la logique pour désactiver un écran
        // Ceci pourrait également impliquer de modifier la configuration d'affichage Windows
    }

    public void ApplyDisplayConfiguration(DisplaysConfiguration config)
    {
        // Récupérer la liste complète des écrans
        var allSystemScreens = _windowsDisplayApiWrapper.DetectDisplays();

        // Définir l’écran principal s’il y en a un dans la config (IsPrimary)
        var primaryScreen = config.Screens.FirstOrDefault(s => s.IsPrimary);
        if (primaryScreen != null)
        {
            WindowsDisplayApiWrapper.SetPrimaryMonitor(primaryScreen);
        }

        // Activer / Désactiver les écrans selon la config
        foreach (var screen in config.Screens)
        {
            if (screen.IsActive)
                WindowsDisplayApiWrapper.ActivateMonitor(screen);
            else
                WindowsDisplayApiWrapper.DeactivateMonitor(screen);

            Console.WriteLine($"Applying settings for {screen.DeviceName} - Mode: {screen.DisplayMode}");
        }

        // Désactiver les écrans qui ne sont pas dans la config
        foreach (var sysScreen in allSystemScreens)
        {
            bool isInConfig = config.Screens.Any(c => c.DeviceName == sysScreen.DeviceName);
            if (!isInConfig && sysScreen.IsActive)
            {
                WindowsDisplayApiWrapper.DeactivateMonitor(sysScreen);
            }
        }
    }
}
