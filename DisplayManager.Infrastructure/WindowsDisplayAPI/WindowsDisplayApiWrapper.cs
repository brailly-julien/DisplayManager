using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace DisplayManager.Infrastructure.WindowsDisplayAPI;

public class WindowsDisplayApiWrapper
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);
    // Définissez la structure MONITORINFOEX
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DISPLAY_DEVICE
    {
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public int StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    // Un delegate pour MonitorEnumProc utilisé par EnumDisplayMonitors
    public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    public WindowsDisplayApiWrapper()
    {
    }

    public void DetectDisplaysWMI()
    {
        try
        {
            // Préparer la requête WMI
            string wmiQuery = "SELECT * FROM Win32_PnPEntity WHERE PNPClass='Monitor'";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);

            // Exécuter la requête
            foreach (ManagementObject obj in searcher.Get())
            {
                string name = obj["Name"] as string; // Nom convivial du moniteur
                string deviceId = obj["DeviceID"] as string; // Identifiant unique du dispositif
                string caption = obj["Caption"] as string; // Description courte

                // Afficher les informations
                Debug.WriteLine($"Name: {name}");
                Debug.WriteLine($"DeviceID: {deviceId}");
                Debug.WriteLine($"Caption: {caption}");
                Debug.WriteLine("---------------------------------------------------");
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"An error occurred: {e.Message}");
        }
    }

    private bool MonitorEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
    {
        // Ici, vous pouvez traiter chaque moniteur détecté
        // Pour cet exemple, nous allons simplement imprimer les coordonnées du moniteur
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
