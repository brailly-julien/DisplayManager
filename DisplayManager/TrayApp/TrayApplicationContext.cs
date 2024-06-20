using DisplayManager.Applications.Services;
using DisplayManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DisplayManager.Applications.Services.ConfigurationService;

namespace DisplayManager.TrayApp;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon trayIcon;
    private readonly ConfigurationService configService;
    //private ScreenManagementService screenManagementService;

    public TrayApplicationContext()
    {
        configService = new ConfigurationService();
        //screenManagementService = new ScreenManagementService();

        // Initialisation du NotifyIcon
        trayIcon = new NotifyIcon()
        {
            Icon = Properties.Resources.AppIcon,
            ContextMenuStrip = new ContextMenuStrip(),
            Visible = true
        };

        UpdateContextMenuWithConfigurations();

        // Ajout des items au menu contextuel de votre NotifyIcon

        trayIcon.ContextMenuStrip.Items.Add("-");
        trayIcon.ContextMenuStrip.Items.Add("Save new Configuration", null, OnDetectScreensClicked);
        //trayIcon.ContextMenuStrip.Items.Add("Configurer les écrans", null, ConfigureScreens);
        //trayIcon.ContextMenuStrip.Items.Add("Activer/Désactiver écrans", null, ToggleScreens);
        //trayIcon.ContextMenuStrip.Items.Add("-");
        trayIcon.ContextMenuStrip.Items.Add("Quitter", null, Exit);
        // Affiche une bulle de notification au démarrage
        trayIcon.ShowBalloonTip(1000, "DisplayManager", "L'application est lancée et prête à l'emploi.", ToolTipIcon.Info);
    }

    private void UpdateContextMenuWithConfigurations()
    {
        var configurations = configService.LoadConfigurations2();
        foreach (var config in configurations)
        {
            // Création d'un menuItem pour chaque configuration
            var menuItem = new ToolStripMenuItem(config.ConfigName, null, OnConfigurationSelected)
            {
                Tag = config // Stockez l'objet DisplaysConfiguration dans le Tag pour un accès facile
            };
            trayIcon.ContextMenuStrip.Items.Insert(0, menuItem); // Insérez au début pour que les plus récents apparaissent en haut
        }
    }

    //private void OnDetectScreensClicked(object sender, EventArgs e)
    //{
    //    var displaysConfig = new ScreenManagementService().DetectConnectedScreens().First();
    //    var screens = displaysConfig.Screens;

    //    string configName = PromptForConfigurationName();

    //    if (!string.IsNullOrEmpty(configName))
    //    {
    //        var result = configService.TrySaveConfiguration(displaysConfig.Screens, configName);
    //        if (result == ConfigurationService.SaveConfigResult.Success)
    //        {
    //            var menuItem = new ToolStripMenuItem(configName, null, OnConfigurationSelected)
    //            {
    //                Tag = displaysConfig.Screens
    //            };
    //            trayIcon.ContextMenuStrip.Items.Insert(0, menuItem);
    //        }
    //        while (result == ConfigurationService.SaveConfigResult.Exists)
    //        {
    //            var choice = MessageBox.Show("A configuration with this name already exists. Do you want to replace it?", "Configuration Exists", MessageBoxButtons.YesNoCancel);

    //            if (choice == DialogResult.Yes)
    //            {
    //                configService.TrySaveConfiguration(displaysConfig.Screens, configName);

    //                var existingMenuItem = trayIcon.ContextMenuStrip.Items.Cast<ToolStripMenuItem>().FirstOrDefault(item => item.Text == configName);
    //                if (existingMenuItem != null)
    //                    existingMenuItem.Tag = displaysConfig.Screens;
    //                else
    //                {
    //                    var menuItem = new ToolStripMenuItem(configName, null, OnConfigurationSelected)
    //                    {
    //                        Tag = displaysConfig.Screens
    //                    };
    //                    trayIcon.ContextMenuStrip.Items.Insert(0, menuItem);
    //                }

    //                break;
    //            }
    //            else if (choice == DialogResult.No)
    //            {
    //                configName = PromptForConfigurationName();
    //                result = configService.TrySaveConfiguration(displaysConfig.Screens, configName);
    //            }
    //            else
    //            {
    //                break;
    //            }
    //        }
    //    }
    //    //var screenService = new ScreenManagementService();
    //    //screenService.DetectConnectedScreens();
    //    //screenManagementService.DetectConnectedScreens();
    //    //var displayService = new DisplayManagementService();
    //    //displayService.PrintDisplayInfo();
    //}

    private void OnDetectScreensClicked(object sender, EventArgs e)
    {
        var configService = new ConfigurationService();
        var screenService = new ScreenManagementService();
        var config = screenService.DetectConnectedScreens().First(); // Suppose this method returns List<DisplaysConfiguration>
        string configName = PromptForConfigurationName();

        if (!string.IsNullOrEmpty(configName))
        {
            config.ConfigName = configName;
            var result = configService.TrySaveConfiguration2(config);
            if (result == SaveConfigResult.Success)
            {
                var menuItem = new ToolStripMenuItem(config.ConfigName, null, OnConfigurationSelected)
                {
                    Tag = config
                };
                trayIcon.ContextMenuStrip.Items.Insert(0, menuItem);
            }
            else if (result == SaveConfigResult.Exists)
            {
                HandleExistingConfiguration(config);
            }
        }
    }

    private void HandleExistingConfiguration(DisplaysConfiguration config)
    {
        var choice = MessageBox.Show("A configuration with this name already exists. Do you want to replace it?", "Configuration Exists", MessageBoxButtons.YesNoCancel);
        if (choice == DialogResult.Yes)
        {
            var configService = new ConfigurationService();
            configService.TrySaveConfiguration2(config); // Assuming overwrite logic is handled inside

            var existingMenuItem = trayIcon.ContextMenuStrip.Items.Cast<ToolStripMenuItem>().FirstOrDefault(item => item.Text == config.ConfigName);
            if (existingMenuItem != null)
                existingMenuItem.Tag = config;
            else
            {
                var menuItem = new ToolStripMenuItem(config.ConfigName, null, OnConfigurationSelected)
                {
                    Tag = config
                };
                trayIcon.ContextMenuStrip.Items.Insert(0, menuItem);
            }
        }
        else if (choice == DialogResult.No)
        {
            // Optionally reprompt for name or handle user cancellation
        }
    }


    //private void OnConfigurationSelected(object sender, EventArgs e)
    //{
    //    var menuItem = sender as ToolStripMenuItem;
    //    var screens = menuItem.Tag as List<Domain.Entities.Screen>;
    //    // Logique pour appliquer la configuration sélectionnée
    //}

    private void OnConfigurationSelected(object sender, EventArgs e)
    {
        var menuItem = sender as ToolStripMenuItem;
        var config = menuItem.Tag as DisplaysConfiguration; // Obtenir la configuration depuis le Tag du menuItem

        if (config != null)
        {
            // Logique pour appliquer la configuration sélectionnée
            // Exemple: Changer la configuration d'affichage des écrans selon les détails dans config
            ApplyDisplayConfiguration(config);
        }
    }
    private void ApplyDisplayConfiguration(DisplaysConfiguration config)
    {
        // Cette méthode devrait appliquer la configuration à l'ensemble des écrans
        // selon les détails contenus dans la configuration fournie
        // Vous aurez probablement besoin de méthodes supplémentaires pour changer les configurations d'affichage actuelles

        foreach (var screen in config.Screens)
        {
            // Appliquez la configuration pour chaque écran
            // Par exemple, ajuster la résolution, l'orientation, etc.
            Console.WriteLine($"Applying settings for {screen.DeviceName} - Mode: {screen.DisplayMode}");
            // Ici, vous pourriez appeler d'autres fonctions API pour ajuster les paramètres réels du moniteur
        }
    }

    private string PromptForConfigurationName()
    {
        using (var form = new Form())
        {
            form.Text = "Enter Config Name";
            var inputBox = new TextBox() { Left = 50, Top = 20, Width = 200 };

            var buttonOk = new Button() { Text = "Ok", Left = 150, Top = 50, Width = 100, DialogResult = DialogResult.OK };
            var buttonCancel = new Button() { Text = "Cancel", Left = 50, Top = 50, Width = 100, DialogResult = DialogResult.Cancel };

            form.Controls.Add(inputBox);
            form.Controls.Add(buttonOk);
            form.Controls.Add(buttonCancel);
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(inputBox.Text) || inputBox.Text == "Enter a name...")
                {
                    MessageBox.Show("Please enter a valid configuration name.", "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                return inputBox.Text;
            }
            return null;
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
