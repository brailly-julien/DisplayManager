using DisplayManager.Applications.Services;
using DisplayManager.Domain.Entities;
using DisplayManager.Views;
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
        InitialItems();
        //trayIcon.ContextMenuStrip.Items.Add("-");
        //trayIcon.ContextMenuStrip.Items.Add("Save new Configuration", null, OnDetectScreensClicked);
        //trayIcon.ContextMenuStrip.Items.Add("Quitter", null, Exit);
        // Affiche une bulle de notification au démarrage
        //trayIcon.ShowBalloonTip(1000, "DisplayManager", "L'application est lancée et prête à l'emploi.", ToolTipIcon.Info);
    }

    private void UpdateContextMenuWithConfigurations()
    {
        List<DisplaysConfiguration> configurations = configService.LoadConfigurations();
        foreach (DisplaysConfiguration config in configurations)
        { // Création d'un menuItem pour chaque configuration avec un sous-menu
            ToolStripMenuItem menuItem = new(config.ConfigName)
            {
                Tag = config
            };

            // Sous-menu pour définir comme configuration par défaut
            ToolStripMenuItem setDefaultItem = new("Set as Default", null, (sender, e) => SetAsDefaultConfig(config))
            {
                Checked = config.IsDefaultConfig
            };

            // Sous-menu pour renommer la configuration
            ToolStripMenuItem renameItem = new("Rename", null, (sender, e) => RenameConfig(config));

            // Sous-menu pour supprimer la configuration
            ToolStripMenuItem deleteItem = new("Delete", null, (sender, e) => DeleteConfig(config));

            // Ajout des sous-menus au menu principal de la configuration
            menuItem.DropDownItems.Add(setDefaultItem);
            menuItem.DropDownItems.Add(renameItem);
            menuItem.DropDownItems.Add(deleteItem);

            // Ajouter le menu de configuration à la barre d'état système
            trayIcon.ContextMenuStrip.Items.Insert(0, menuItem);
        }
    }

    private void InitialItems()
    {
        trayIcon.ContextMenuStrip.Items.Add("-");
        trayIcon.ContextMenuStrip.Items.Add("Save new Configuration", null, OnDetectScreensClicked);
        trayIcon.ContextMenuStrip.Items.Add("Quitter", null, Exit);
    }

    private void OnDetectScreensClicked(object sender, EventArgs e)
    {
        AddNewConfig();
    }

    private void AddNewConfig()
    {
        ScreenManagementService screenService = new();
        DisplaysConfiguration config = screenService.DetectConnectedScreens().First(); // Suppose this method returns List<DisplaysConfiguration>
        string configName = PromptForConfigurationName();

        if (!string.IsNullOrEmpty(configName))
        {
            config.ConfigName = configName;

            List<DisplaysConfiguration> existingConfigurations = configService.LoadConfigurations();
            if (existingConfigurations.Count == 0)
                config.IsDefaultConfig = true;
            else
                config.IsDefaultConfig = false;

            SaveConfigResult result = configService.TrySaveConfiguration(config);
            if (result == SaveConfigResult.Success)
                RefreshContextMenu();
            else if (result == SaveConfigResult.Exists)
                HandleExistingConfiguration(config, configService);
        }
    }

    private void HandleExistingConfiguration(DisplaysConfiguration config, ConfigurationService configService)
    {
        CustomConfigDialog customDialog = new();
        DialogResult result = customDialog.ShowDialog(config.ConfigName);

        if (result == DialogResult.Yes)
        {
            configService.TrySaveConfiguration(config);

            ToolStripMenuItem? existingMenuItem = trayIcon.ContextMenuStrip.Items.Cast<ToolStripMenuItem>().FirstOrDefault(item => item.Text == config.ConfigName);
            if (existingMenuItem != null)
                existingMenuItem.Tag = config;
            else
            {
                ToolStripMenuItem menuItem = new(config.ConfigName, null, OnConfigurationSelected)
                {
                    Tag = config
                };
                trayIcon.ContextMenuStrip.Items.Insert(0, menuItem);
            }
        }
        else if (result == DialogResult.No)
            AddNewConfig();
    }

    private void OnConfigurationSelected(object sender, EventArgs e)
    {
        ToolStripMenuItem? menuItem = sender as ToolStripMenuItem;
        if (menuItem.Tag is DisplaysConfiguration config)
        {
            // Appliquer la configuration
            ScreenManagementService screenService = new();
            screenService.ApplyDisplayConfiguration(config);
        }
    }

    private static string PromptForConfigurationName(string promptMessage = "Enter Config Name")
    {
        using (Form form = new())
        {
            form.Text = promptMessage;
            TextBox inputBox = new() { Left = 50, Top = 20, Width = 200 };

            Button buttonOk = new() { Text = "Ok", Left = 150, Top = 50, Width = 100, DialogResult = DialogResult.OK };
            Button buttonCancel = new() { Text = "Cancel", Left = 50, Top = 50, Width = 100, DialogResult = DialogResult.Cancel };

            form.Controls.Add(inputBox);
            form.Controls.Add(buttonOk);
            form.Controls.Add(buttonCancel);
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(inputBox.Text))
                {
                    MessageBox.Show("Please enter a valid configuration name.", "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                return inputBox.Text;
            }
            return null;
        }
    }

    private void SetAsDefaultConfig(DisplaysConfiguration config)
    {
        List<DisplaysConfiguration> configurations = configService.LoadConfigurations();

        foreach (DisplaysConfiguration c in configurations)
        {
            c.IsDefaultConfig = false;
        }

        DisplaysConfiguration? selectedConfig = configurations.FirstOrDefault(c => c.ConfigName == config.ConfigName);
        if (selectedConfig != null)
            selectedConfig.IsDefaultConfig = true;

        configService.SaveAllConfigurations(configurations);
        MessageBox.Show($"{config.ConfigName} is now {(config.IsDefaultConfig ? "the default" : "no longer the default")}.", "Configuration Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RefreshContextMenu();
    }

    private void RenameConfig(DisplaysConfiguration config)
    {
        string newName = PromptForConfigurationName("Enter new configuration name:");
        if (!string.IsNullOrEmpty(newName))
        {
            if (configService.RenameConfiguration(config.ConfigName, newName))
            {
                MessageBox.Show($"Configuration renamed to {newName}.", "Rename Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshContextMenu();
            }
            else
                MessageBox.Show("Failed to rename the configuration. The name might already be in use.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


    private void DeleteConfig(DisplaysConfiguration config)
    {
        if (MessageBox.Show($"Are you sure you want to delete {config.ConfigName}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            if (configService.DeleteConfiguration(config.ConfigName))
            {
                MessageBox.Show($"{config.ConfigName} has been deleted.", "Configuration Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Si la configuration supprimée était la configuration par défaut
                if (config.IsDefaultConfig)
                {
                    List<DisplaysConfiguration> configurations = configService.LoadConfigurations();
                    if (configurations.Any())
                    {
                        // Définir la première configuration restante comme par défaut
                        configurations[0].IsDefaultConfig = true;
                        configService.SaveAllConfigurations(configurations);
                    }
                }

                RefreshContextMenu();
            }
            else
            {
                MessageBox.Show("Failed to delete the configuration.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void RefreshContextMenu()
    {
        trayIcon.ContextMenuStrip.Items.Clear();
        UpdateContextMenuWithConfigurations();
        InitialItems();
    }

    private void Exit(object sender, EventArgs e)
    {
        // Cache l'icône de la barre des tâches et libère toutes les ressources
        trayIcon.Visible = false;
        Application.Exit();
    }
}
