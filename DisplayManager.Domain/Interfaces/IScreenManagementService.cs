using DisplayManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayManager.Domain.Interfaces;

public interface IScreenManagementService
{
    void DetectConnectedScreens();
    List<Screen> GetDisplayDevices();
    void PrintDisplayInfo();
    void ActivateScreen(int screenId);
    void DesactivateScreen(int screenId);
}
