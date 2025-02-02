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
        var configurations = configService.LoadConfigurations();
        foreach (var config in configurations)
        {
            // Création d'un menuItem pour chaque configuration avec un sous-menu
            var menuItem = new ToolStripMenuItem(config.ConfigName)
            {
                Tag = config
            };

            // Sous-menu pour définir comme configuration par défaut
            var setDefaultItem = new ToolStripMenuItem("Set as Default", null, (sender, e) => SetAsDefaultConfig(config))
            {
                Checked = config.IsDefaultConfig
            };

            // Sous-menu pour renommer la configuration
            var renameItem = new ToolStripMenuItem("Rename", null, (sender, e) => RenameConfig(config));

            // Sous-menu pour supprimer la configuration
            var deleteItem = new ToolStripMenuItem("Delete", null, (sender, e) => DeleteConfig(config));

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

            var existingConfigurations = configService.LoadConfigurations();
            if (existingConfigurations.Count == 0)
                config.IsDefaultConfig = true;
            else
                config.IsDefaultConfig = false;

            var result = configService.TrySaveConfiguration(config);
            if (result == SaveConfigResult.Success)
                RefreshContextMenu();
            else if (result == SaveConfigResult.Exists)
                HandleExistingConfiguration(config, configService);
        }
    }

    private void HandleExistingConfiguration(DisplaysConfiguration config, ConfigurationService configService)
    {
        var customDialog = new CustomConfigDialog();
        var result = customDialog.ShowDialog(config.ConfigName);

        if (result == DialogResult.Yes)
        {
            configService.TrySaveConfiguration(config);

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
        else if (result == DialogResult.No)
        {
            AddNewConfig();
        }
    }

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

    private string PromptForConfigurationName(string promptMessage = "Enter Config Name")
    {
        using (var form = new Form())
        {
            form.Text = promptMessage;
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
        var configurations = configService.LoadConfigurations();

        // Mettre IsDefaultConfig à false pour toutes les configurations
        foreach (var c in configurations)
        {
            c.IsDefaultConfig = false;
        }

        // Mettre IsDefaultConfig à true pour la configuration sélectionnée
        var selectedConfig = configurations.FirstOrDefault(c => c.ConfigName == config.ConfigName);
        if (selectedConfig != null)
        {
            selectedConfig.IsDefaultConfig = true;
        }

        // Sauvegarder toutes les configurations mises à jour
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
            {
                MessageBox.Show("Failed to rename the configuration. The name might already be in use.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    var configurations = configService.LoadConfigurations();
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
