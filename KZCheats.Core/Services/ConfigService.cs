using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class ConfigData
{
    public List<string> RecentFiles { get; set; } = new List<string>();
}

public static class ConfigService
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
    private static ConfigData _config;

    static ConfigService()
    {
        LoadConfig();
    }

    private static void LoadConfig()
    {
        if (File.Exists(ConfigFilePath))
        {
            string json = File.ReadAllText(ConfigFilePath);
            _config = JsonSerializer.Deserialize<ConfigData>(json) ?? new ConfigData();
        }
        else
        {
            _config = new ConfigData();
            SaveConfig();
        }
    }

    private static void SaveConfig()
    {
        try
        {
            string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
            System.Diagnostics.Debug.WriteLine("Config saved successfully!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
        }
    }


    public static List<string> GetRecentFiles()
    {
        return new List<string>(_config.RecentFiles);
    }

    public static void AddRecentFile(string filePath)
    {
        if (!_config.RecentFiles.Contains(filePath))
        {
            _config.RecentFiles.Add(filePath);
            SaveConfig();
        }
    }

    public static void RemoveRecentFile(string filePath)
    {
        if (_config.RecentFiles.Contains(filePath))
        {
            _config.RecentFiles.Remove(filePath);
            SaveConfig();
        }
    }

    public static void CleanupMissingFiles()
    {
        bool changed = false;

        _config.RecentFiles.RemoveAll(file =>
        {
            bool missing = !File.Exists(file);
            if (missing)
                changed = true;
            return missing;
        });

        if (changed)
            SaveConfig();
    }
}
