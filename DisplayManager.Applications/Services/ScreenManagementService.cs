using DisplayManager.Domain.Interfaces;
using DisplayManager.Infrastructure.WindowsDisplayAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayManager.Applications.Services;

public class ScreenManagementService : IScreenManagementService
{
    private readonly WindowsDisplayApiWrapper _windowsDisplayApiWrapper;

    public ScreenManagementService(/*WindowsDisplayApiWrapper windowsDisplayApiWrapper*/)
    {
        _windowsDisplayApiWrapper = new WindowsDisplayApiWrapper();
    }

    public void DetectConnectedScreens()
    {
        // Implémentez la logique pour détecter les écrans connectés
        // Utilisez _windowsDisplayApiWrapper pour interagir avec les API Windows
        _windowsDisplayApiWrapper.DetectDisplaysWMI();
    }

    public void ActivateScreen(int screenId)
    {
        // Implémentez la logique pour activer un écran
        // Ceci pourrait impliquer de modifier la configuration d'affichage Windows
    }

    public void DeactivateScreen(int screenId)
    {
        // Implémentez la logique pour désactiver un écran
        // Ceci pourrait également impliquer de modifier la configuration d'affichage Windows
    }
}
