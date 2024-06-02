using DisplayManager.Domain.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace DisplayManager.Applications.Services;

public class ConfigurationService
{
    private readonly string configPath = "ScreenConfigs.json";

    public bool TrySaveConfiguration(List<Screen> screens, string configName, out string errorMessage)
    {
        Dictionary<string, List<Screen>> configurations = LoadConfigurations();

        if (configurations.ContainsKey(configName))
        {
            errorMessage = "Configuration name already exists.";
            return false;
        }

        configurations[configName] = screens;
        File.WriteAllText(configPath, JsonConvert.SerializeObject(configurations, Formatting.Indented));
        errorMessage = null;
        return true;
    }

    private Dictionary<string, List<Screen>> LoadConfigurations()
    {
        if (!File.Exists(configPath))
            return new Dictionary<string, List<Screen>>();

        string json = File.ReadAllText(configPath);
        return JsonConvert.DeserializeObject<Dictionary<string, List<Screen>>>(json) ?? new Dictionary<string, List<Screen>>();
    }
}