using McpFederationGateway.Interfaces;
using McpFederationGateway.Models;
using System.Text.Json;

namespace McpFederationGateway.Services;

public class ConfigurationService : IConfigurationService
{
    private const string ConfigEnvVar = "MICROAGENTS_CONFIG";
    private readonly string _userConfigPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AppJsonContext _jsonContext;

    public ConfigurationService()
    {
        var envPath = Environment.GetEnvironmentVariable(ConfigEnvVar);
        if (!string.IsNullOrEmpty(envPath))
        {
            _userConfigPath = envPath;
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _userConfigPath = Path.Combine(home, ".microagents", "config.json");
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        // Bind context to options
        _jsonOptions.TypeInfoResolver = AppJsonContext.Default; 
        // Actually, if we use the constructor pattern:
        _jsonContext = new AppJsonContext(_jsonOptions);
    }

    public async Task<GatewayConfig> GetConfigAsync()
    {
        if (!File.Exists(_userConfigPath))
        {
            return new GatewayConfig();
        }

        try
        {
            using var stream = File.OpenRead(_userConfigPath);
            var config = await JsonSerializer.DeserializeAsync(stream, _jsonContext.GatewayConfig);
            return config ?? new GatewayConfig();
        }
        catch (Exception)
        {
            // Fallback to empty config on error
            // In production, we might want to log this
            return new GatewayConfig();
        }
    }
}
