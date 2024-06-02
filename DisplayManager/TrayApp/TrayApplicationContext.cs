using DisplayManager.Applications.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayManager.TrayApp;

public class TrayApplicationContext : ApplicationContext
{
    private NotifyIcon trayIcon;
    //private ScreenManagementService screenManagementService;

    public TrayApplicationContext()
    {
        //screenManagementService = new ScreenManagementService();

        // Initialisation du NotifyIcon
        trayIcon = new NotifyIcon()
        {
            Icon = Properties.Resources.AppIcon,
            ContextMenuStrip = new ContextMenuStrip(),
            Visible = true
        };

        // Ajout des items au menu contextuel de votre NotifyIcon
        trayIcon.ContextMenuStrip.Items.Add("Save new Configuration", null, OnDetectScreensClicked);
        trayIcon.ContextMenuStrip.Items.Add("Configurer les écrans", null, ConfigureScreens);
        trayIcon.ContextMenuStrip.Items.Add("Activer/Désactiver écrans", null, ToggleScreens);
        trayIcon.ContextMenuStrip.Items.Add("-");
        trayIcon.ContextMenuStrip.Items.Add("Quitter", null, Exit);

        // Affiche une bulle de notification au démarrage
        trayIcon.ShowBalloonTip(1000, "DisplayManager", "L'application est lancée et prête à l'emploi.", ToolTipIcon.Info);
    }

    private void OnDetectScreensClicked(object sender, EventArgs e)
    {
        var screenService = new ScreenManagementService();
        var screens = screenService.GetDisplayDevices();
        var configService = new ConfigurationService();

        string configName = PromptForConfigurationName();
        if (!string.IsNullOrEmpty(configName))
        {
            var result = configService.TrySaveConfiguration(screens, configName);
            while (result == ConfigurationService.SaveConfigResult.Exists)
            {
                var choice = MessageBox.Show("A configuration with this name already exists. Do you want to replace it?", "Configuration Exists", MessageBoxButtons.YesNoCancel);

                if (choice == DialogResult.Yes)
                {
                    configService.TrySaveConfiguration(screens, configName); // Force save
                    break;
                }
                else if (choice == DialogResult.No)
                {
                    configName = PromptForConfigurationName(); // Reprompt for a new name
                    result = configService.TrySaveConfiguration(screens, configName);
                }
                else
                {
                    break; // Cancel the operation
                }
            }
        }
        //var screenService = new ScreenManagementService();
        //screenService.DetectConnectedScreens();
        //screenManagementService.DetectConnectedScreens();
        //var displayService = new DisplayManagementService();
        //displayService.PrintDisplayInfo();
    }

    private string PromptForConfigurationName()
    {
        using (var form = new Form())
        {
            var inputBox = new TextBox() { Left = 50, Top = 50, Width = 200 };
            var buttonOk = new Button() { Text = "Ok", Left = 150, Width = 100, Top = 100, DialogResult = DialogResult.OK };
            form.Controls.Add(inputBox);
            form.Controls.Add(buttonOk);
            form.AcceptButton = buttonOk;

            return form.ShowDialog() == DialogResult.OK ? inputBox.Text : null;
        }
    }

    private void ConfigureScreens(object sender, EventArgs e)
    {
        // Implémentez la logique pour ouvrir une fenêtre de configuration ou un dialogue
    }

    private void ToggleScreens(object sender, EventArgs e)
    {
        // Implémentez la logique pour activer/désactiver les écrans
    }

    private void Exit(object sender, EventArgs e)
    {
        // Cache l'icône de la barre des tâches et libère toutes les ressources
        trayIcon.Visible = false;
        Application.Exit();
    }
}
