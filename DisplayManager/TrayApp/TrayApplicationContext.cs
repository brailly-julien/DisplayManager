using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayManager.TrayApp;

public class TrayApplicationContext : ApplicationContext
{
    private NotifyIcon trayIcon;

    public TrayApplicationContext()
    {
        // Initialisation du NotifyIcon
        trayIcon = new NotifyIcon()
        {
            Icon = Properties.Resources.AppIcon, // Assurez-vous d'avoir une icône AppIcon dans les ressources du projet
            ContextMenuStrip = new ContextMenuStrip(),
            Visible = true
        };

        // Ajout des items au menu contextuel
        trayIcon.ContextMenuStrip.Items.Add("Configurer les écrans", null, ConfigureScreens);
        trayIcon.ContextMenuStrip.Items.Add("Activer/Désactiver écrans", null, ToggleScreens);
        trayIcon.ContextMenuStrip.Items.Add("-");
        trayIcon.ContextMenuStrip.Items.Add("Quitter", null, Exit);

        // Affiche une bulle de notification au démarrage
        trayIcon.ShowBalloonTip(1000, "DisplayManager", "L'application est lancée et prête à l'emploi.", ToolTipIcon.Info);
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
