using DisplayManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayManager.Domain.Interfaces;

public interface IScreenManagementService
{
    List<DisplaysConfiguration> DetectConnectedScreens();
    void ActivateScreen(int screenId);
    void DesactivateScreen(int screenId);
}
