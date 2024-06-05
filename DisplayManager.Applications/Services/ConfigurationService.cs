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

    // Modification pour retourner un enum représentant le résultat de la tentative de sauvegarde
    public SaveConfigResult TrySaveConfiguration(List<Screen> screens, string configName)
    {
        Dictionary<string, List<Screen>> configurations = LoadConfigurations();

        if (configurations.ContainsKey(configName))
        {
            return SaveConfigResult.Exists; // Retourne Exists si le nom existe déjà
        }

        configurations[configName] = screens;
        File.WriteAllText(configPath, JsonConvert.SerializeObject(configurations, Formatting.Indented));
        return SaveConfigResult.Success; // Retourne Success si la sauvegarde est réussie
    }

    public Dictionary<string, List<Screen>> LoadConfigurations()
    {
        if (!File.Exists(configPath))
            return new Dictionary<string, List<Screen>>();

        string json = File.ReadAllText(configPath);
        return JsonConvert.DeserializeObject<Dictionary<string, List<Screen>>>(json) ?? new Dictionary<string, List<Screen>>();
    }

    public enum SaveConfigResult
    {
        Success,
        Exists,
        Error // Vous pouvez utiliser Error pour gérer les exceptions ou autres erreurs
    }
}