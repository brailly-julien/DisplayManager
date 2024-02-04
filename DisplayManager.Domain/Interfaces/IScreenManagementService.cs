using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayManager.Domain.Interfaces;

public interface IScreenManagementService
{
    void DetectConnectedScreens();
    void ActivateScreen(int screenId);
    void DeactivateScreen(int screenId);
}
