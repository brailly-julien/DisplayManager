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

    public List<Screen> GetDisplayDevices()
    {
        return _windowsDisplayApiWrapper.DetectDisplays();
    }

    public void PrintDisplayInfo()
    {
        var displays = GetDisplayDevices();
        foreach (var display in displays)
        {
            System.Diagnostics.Debug.WriteLine($"Device Name: {display.DeviceName}");
            System.Diagnostics.Debug.WriteLine($"Device String: {display.DeviceString}");
            System.Diagnostics.Debug.WriteLine($"Device ID: {display.DeviceID}");
            System.Diagnostics.Debug.WriteLine($"Device Key: {display.DeviceKey}");
            System.Diagnostics.Debug.WriteLine($"State Flags: {System.Convert.ToString(display.StateFlags, 2).PadLeft(32, '0')}");
            System.Diagnostics.Debug.WriteLine($"Active: {display.IsActive}");
            System.Diagnostics.Debug.WriteLine($"Primary: {display.IsPrimary}");
            System.Diagnostics.Debug.WriteLine($"Position: ({display.PositionX}, {display.PositionY})");
            System.Diagnostics.Debug.WriteLine($"Resolution: {display.Width}x{display.Height}");
            System.Diagnostics.Debug.WriteLine($"Display Mode: {display.DisplayMode}");
            System.Diagnostics.Debug.WriteLine("---------------------------------------------------");
        }
    }

    public void DetectConnectedScreens()
    {
        // Implémentez la logique pour détecter les écrans connectés
        // Utilisez _windowsDisplayApiWrapper pour interagir avec les API Windows
        PrintDisplayInfo();
        _windowsDisplayApiWrapper.DetectDisplaysWMI();
        /*_windowsDisplayApiWrapper.DetectDisplaysWMI();
        _windowsDisplayApiWrapper.DetectDevices();*/
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
}
