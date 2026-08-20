using System.Text.Json;

namespace CircleMouse.Core;

public sealed class ConfigurationStore(string path)
{
    public async Task<AppConfiguration> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            var defaults = AppConfiguration.CreateDefault();
            await SaveAsync(defaults, cancellationToken);
            return defaults;
        }

        await using var stream = File.OpenRead(path);
        var configuration = await JsonSerializer.DeserializeAsync<AppConfiguration>(stream, ConfigurationJson.Options, cancellationToken);
        if (configuration is null || configuration.Version != AppConfiguration.CurrentVersion)
            throw new InvalidDataException("The configuration is empty or uses an unsupported version.");
        return configuration;
    }

    public async Task SaveAsync(AppConfiguration configuration, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporaryPath = path + ".tmp";
        await using (var stream = File.Create(temporaryPath))
            await JsonSerializer.SerializeAsync(stream, configuration, ConfigurationJson.Options, cancellationToken);
        File.Move(temporaryPath, path, true);
    }
}
