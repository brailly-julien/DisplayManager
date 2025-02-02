using DisplayManager.Domain.Entities;

namespace DisplayManager.Domain.Interfaces;

public interface IScreenManagementService
{
    List<DisplaysConfiguration> DetectConnectedScreens();
    void ActivateScreen(int screenId);
    void DesactivateScreen(int screenId);
}
