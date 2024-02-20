using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayManager.Infrastructure.WindowsDisplayAPI;

public class WindowsDisplayApiWrapper
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    // Un delegate pour MonitorEnumProc utilisé par EnumDisplayMonitors
    public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    public WindowsDisplayApiWrapper()
    {
    }

    public void DetectDisplays()
    {
        // Utilisez les API Windows pour détecter les affichages
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, new MonitorEnumProc(MonitorEnum), IntPtr.Zero);
    }

    private bool MonitorEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
    {
        // Ici, vous pouvez traiter chaque moniteur détecté
        // Pour cet exemple, nous allons simplement imprimer les coordonnées du moniteur
        Debug.WriteLine("Test");
        Debug.WriteLine($"Monitor bounds are {lprcMonitor.Left}, {lprcMonitor.Top}, {lprcMonitor.Right}, {lprcMonitor.Bottom}");

        return true; // Pour continuer l'énumération
    }

    public void SetDisplayState(int displayId, bool enable)
    {
        // Active ou désactive l'affichage spécifié
    }

    // Une structure RECT nécessaire pour l'utilisation avec l'API
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
