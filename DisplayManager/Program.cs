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

        #if DEBUG
            // Remonte de 3 niveaux : 
            //  bin\Debug\netX\ =>   bin\Debug\     =>  bin\    =>  <ProjectRoot>
            var exeFolder = AppContext.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(exeFolder, @"..\..\..\"));
            Directory.SetCurrentDirectory(projectRoot);
        #endif

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Cr�ez le service de gestion des �crans et d�tectez les �crans imm�diatement
        //var screenService = new ScreenManagementService();
        //screenService.DetectConnectedScreens();

        Application.Run(new TrayApplicationContext());
    }
}