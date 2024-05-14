using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using DisplayManager.Domain.Entities;

namespace DisplayManager.Infrastructure.WindowsDisplayAPI;

public class WindowsDisplayApiWrapper
{
    [DllImport("user32.dll")]
    static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

    private const int ENUM_CURRENT_SETTINGS = -1;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    public List<Screen> DetectDisplays()
    {
        List<Screen> displays = new List<Screen>();

        int deviceNum = 0;
        DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
        displayDevice.cb = Marshal.SizeOf(displayDevice);

        while (EnumDisplayDevices(null, (uint)deviceNum, ref displayDevice, 0))
        {
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(devMode);

            Screen info = new Screen
            {
                DeviceName = displayDevice.DeviceName,
                DeviceString = displayDevice.DeviceString,
                DeviceID = displayDevice.DeviceID,
                DeviceKey = displayDevice.DeviceKey,
                StateFlags = displayDevice.StateFlags,
                
            };

            if (EnumDisplaySettings(displayDevice.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
            {
                info.PositionX = devMode.dmPositionX;
                info.PositionY = devMode.dmPositionY;
                info.Width = devMode.dmPelsWidth;
                info.Height = devMode.dmPelsHeight;
            }
            displays.Add(info);

            deviceNum++;
        }
        GetDisplayModes(displays);

        return displays;
    }

    private void GetDisplayModes(List<Screen> displays)
    {
        for (int i = 0; i < displays.Count; i++)
        {
            Screen currentDisplay = displays[i];

            // Si le moniteur n'est pas actif, définissez le mode sur "Deactivated"
            if (!currentDisplay.IsActive)
            {
                currentDisplay.DisplayMode = "Deactivated";
                continue; // Passez au moniteur suivant
            }

            // Comparer les positions pour déterminer le mode d'affichage
            bool isDuplicated = false;

            foreach (Screen otherDisplay in displays)
            {
                if (currentDisplay != otherDisplay &&
                    currentDisplay.PositionX == otherDisplay.PositionX &&
                    currentDisplay.PositionY == otherDisplay.PositionY &&
                    currentDisplay.Width == otherDisplay.Width &&
                    currentDisplay.Height == otherDisplay.Height)
                {
                    isDuplicated = true;
                    break;
                }
            }

            currentDisplay.DisplayMode = isDuplicated ? "Duplicated" : "Extended";
        }
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
}