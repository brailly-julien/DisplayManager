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
    public WindowsDisplayApiWrapper()
    {
    }

    [DllImport("user32.dll")]
    static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

    private const int ENUM_CURRENT_SETTINGS = -1;
    private const int DISPLAY_DEVICE_ACTIVE = 0x1;
    private const int DISPLAY_DEVICE_PRIMARY_DEVICE = 0x4;

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

    public void DetectDevices()
    {
        try
        {
            int deviceNum = 0;
            DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
            displayDevice.cb = Marshal.SizeOf(displayDevice);
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(devMode);

            while (EnumDisplayDevices(null, (uint)deviceNum, ref displayDevice, 0))
            {
                bool isActive = (displayDevice.StateFlags & DISPLAY_DEVICE_ACTIVE) != 0;
                bool isPrimary = (displayDevice.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0;


                Debug.WriteLine($"Device Number: {deviceNum}");
                Debug.WriteLine($"Device Name: {displayDevice.DeviceName}");
                Debug.WriteLine($"Device String: {displayDevice.DeviceString}");
                Debug.WriteLine($"Device ID: {displayDevice.DeviceID}");
                Debug.WriteLine($"Device Key: {displayDevice.DeviceKey}");
                Debug.WriteLine($"State Flags: {Convert.ToString(displayDevice.StateFlags, 2).PadLeft(32, '0')}");
                Debug.WriteLine($"Active: {isActive}");
                if (EnumDisplaySettings(displayDevice.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    Debug.WriteLine($"Primary: {isPrimary}");
                    Debug.WriteLine($"Position: ({devMode.dmPositionX}, {devMode.dmPositionY})");
                    Debug.WriteLine($"Resolution: {devMode.dmPelsWidth}x{devMode.dmPelsHeight}");
                }
                Debug.WriteLine("---------------------------------------------------");

                deviceNum++;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"An error occurred: {e.Message}");
        }
    }
}
