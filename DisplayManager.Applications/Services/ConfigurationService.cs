using DisplayManager.Domain.Entities;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace DisplayManager.Applications.Services;

public class ConfigurationService
{
    private readonly string configPath = "ScreenConfigs.json";

    public SaveConfigResult TrySaveConfiguration(DisplaysConfiguration config)
    {
        List<DisplaysConfiguration> configurations = LoadConfigurations();

        if (configurations.Any(c => c.ConfigName == config.ConfigName))
            return SaveConfigResult.Exists;

        if (configurations.Count == 0)
            config.IsDefaultConfig = true;// Première configuration, définir comme par défaut
        else            
            config.IsDefaultConfig = false;// Nouvelles configurations ne sont pas par défaut

        configurations.Add(config);
        File.WriteAllText(configPath, JsonConvert.SerializeObject(configurations, Formatting.Indented));
        return SaveConfigResult.Success;
    }

    public void SaveAllConfigurations(List<DisplaysConfiguration> configurations)
    {
        File.WriteAllText(configPath, JsonConvert.SerializeObject(configurations, Formatting.Indented));
    }

    public List<DisplaysConfiguration> LoadConfigurations()
    {
        if (!File.Exists(configPath))
            return [];

        string json = File.ReadAllText(configPath);
        return JsonConvert.DeserializeObject<List<DisplaysConfiguration>>(json) ?? [];
    }

    public bool DeleteConfiguration(string configName)
    {
        List<DisplaysConfiguration> configurations = LoadConfigurations();
        DisplaysConfiguration? configToDelete = configurations.FirstOrDefault(c => c.ConfigName == configName);

        if (configToDelete != null)
        {
            configurations.Remove(configToDelete);
            File.WriteAllText(configPath, JsonConvert.SerializeObject(configurations, Formatting.Indented));
            return true;
        }

        return false; // Retourne false si la configuration n'a pas été trouvée
    }

    public bool RenameConfiguration(string oldName, string newName)
    {
        List<DisplaysConfiguration> configurations = LoadConfigurations();
        DisplaysConfiguration? configToRename = configurations.FirstOrDefault(c => c.ConfigName == oldName);

        if (configToRename != null && !configurations.Any(c => c.ConfigName == newName))
        {
            configToRename.ConfigName = newName;
            SaveAllConfigurations(configurations);
            return true;
        }
        return false;
    }



    public enum SaveConfigResult
    {
        Success,
        Exists,
        Error // Vous pouvez utiliser Error pour gérer les exceptions ou autres erreurs
    }
}