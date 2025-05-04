using System.Diagnostics;
using System.Runtime.InteropServices;
using DisplayManager.Domain.Entities;

namespace DisplayManager.Infrastructure.WindowsDisplayAPI;

public class WindowsDisplayApiWrapper
{
    // ─────────────────────────────────────────────────────────────────────
    // █▀▀ CONSTANTES ET IMPORTS
    // ─────────────────────────────────────────────────────────────────────

    private const int ENUM_CURRENT_SETTINGS = -1;

    // Flags possibles pour dwFlags dans ChangeDisplaySettingsEx
    private const int CDS_UPDATEREGISTRY = 0x00000001;  // Met à jour le registre
    private const int CDS_TEST = 0x00000002;           // Teste la configuration sans l'appliquer
    private const int CDS_FULLSCREEN = 0x00000004;
    private const int CDS_GLOBAL = 0x00000008;
    private const int CDS_SET_PRIMARY = 0x00000010;

    // Valeurs de retour possibles
    private const int DISP_CHANGE_SUCCESSFUL = 0;
    private const int DISP_CHANGE_RESTART = 1;
    private const int DISP_CHANGE_FAILED = -1;
    private const int DISP_CHANGE_BADMODE = -2;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int ChangeDisplaySettingsEx(
        string lpszDeviceName,
        ref DEVMODE lpDevMode,
        IntPtr hwnd,
        uint dwflags,
        IntPtr lParam
    );

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

    [DllImport("user32.dll")]
    static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    // ─────────────────────────────────────────────────────────────────────
    // █▀▀ STRUCTS
    // ─────────────────────────────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────────────────
    // █▀▀ DÉTECTION DES ÉCRANS
    // ─────────────────────────────────────────────────────────────────────

    public List<Screen> DetectDisplays()
    {
        List<Screen> displays = [];

        int deviceNum = 0;
        DISPLAY_DEVICE displayDevice = new();
        displayDevice.cb = Marshal.SizeOf(displayDevice);

        while (EnumDisplayDevices("", (uint)deviceNum, ref displayDevice, 0))
        {
            DEVMODE devMode = new();
            devMode.dmSize = (short)Marshal.SizeOf(devMode);

            Screen info = new()
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
            displayDevice = new DISPLAY_DEVICE();
            displayDevice.cb = Marshal.SizeOf(displayDevice);
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

    // ─────────────────────────────────────────────────────────────────────
    // █▀▀ ACTIONS SUR LES ÉCRANS (SetPrimary, Activate, Deactivate)
    // ─────────────────────────────────────────────────────────────────────

    public static void SetPrimaryMonitor(Screen screen)
    {
        // Charger les settings actuels
        DEVMODE dm = new();
        dm.dmSize = (short)Marshal.SizeOf(dm);

        bool success = EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);
        if (!success)
        {
            Debug.WriteLine($"EnumDisplaySettings failed for {screen.DeviceName}");
            return;
        }

        // Positionner l'écran à (0, 0) pour en faire le principal
        dm.dmPositionX = 0;
        dm.dmPositionY = 0;

        // Appliquer la config
        int result = ChangeDisplaySettingsEx(
            screen.DeviceName,
            ref dm,
            IntPtr.Zero,
            CDS_UPDATEREGISTRY,
            IntPtr.Zero
        );

        if (result != DISP_CHANGE_SUCCESSFUL)
        {
            Debug.WriteLine($"SetPrimaryMonitor failed with code {result}");
        }
    }

    public static void ActivateMonitor(Screen screen)
    {
        // Pour activer un moniteur, on doit l'attacher au desktop en modifiant son DEVMODE
        DEVMODE dm = new();
        dm.dmSize = (short)Marshal.SizeOf(dm);

        bool success = EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);
        if (!success)
        {
            Debug.WriteLine($"EnumDisplaySettings failed for {screen.DeviceName}");
            return;
        }

        // Exemple : placer le moniteur à une position quelconque (ici 1920 * index par ex.)
        // Dans la pratique, vous calculerez la position ou ferez un arrangement selon la config.
        // Pour la démo, on va juste le réactiver au même endroit
        dm.dmFields = 0x180000; // DM_POSITION + DM_PELSWIDTH + DM_PELSHEIGHT par exemple
        // Vous devrez trouver les flags exacts pour activer et ré-attacher l'écran

        // ChangeDisplaySettingsEx
        int result = ChangeDisplaySettingsEx(screen.DeviceName, ref dm, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
        if (result != DISP_CHANGE_SUCCESSFUL)
        {
            Debug.WriteLine($"ActivateMonitor failed with code {result}");
        }
    }

    public static void DeactivateMonitor(Screen screen)
    {
        // Détacher un moniteur en le positionnant en "off"
        DEVMODE dm = new();
        dm.dmSize = (short)Marshal.SizeOf(dm);

        bool success = EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);
        if (!success)
        {
            Debug.WriteLine($"EnumDisplaySettings failed for {screen.DeviceName}");
            return;
        }

        // Pour vraiment détacher un moniteur, on peut positionner l'écran en mode "null".
        // On peut aussi appeler ChangeDisplaySettingsEx avec des flags spécifiques indiquant
        // que l'écran n'est plus attaché.
        // Ceci est un pseudo-code : la manipulation peut être différente selon les versions de Windows.

        dm.dmPositionX = -1; // Méthode de contournement, pas forcément fiable
        dm.dmPositionY = -1;
        
        int result = ChangeDisplaySettingsEx(screen.DeviceName, ref dm, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
        if (result != DISP_CHANGE_SUCCESSFUL)
        {
            Debug.WriteLine($"DeactivateMonitor failed with code {result}");
        }
    }
}