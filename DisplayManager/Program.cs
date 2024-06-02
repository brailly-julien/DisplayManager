using DisplayManager.Applications.Services;
using DisplayManager.TrayApp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayManager;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Créez le service de gestion des écrans et détectez les écrans immédiatement
        //var screenService = new ScreenManagementService();
        //screenService.DetectConnectedScreens();

        Application.Run(new TrayApplicationContext());
    }
}