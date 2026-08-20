using System.Text.Json;
using System.Text.Json.Serialization;

namespace CircleMouse.Core;

public sealed record AppConfiguration(int Version, IReadOnlyList<Profile> Profiles)
{
    public const int CurrentVersion = 1;

    public static AppConfiguration CreateDefault() => new(CurrentVersion,
    [new Profile("Global", 0, null, new Dictionary<MouseTrigger, ActionDefinition>
    {
        [MouseTrigger.XButton1] = new KeyboardShortcutAction([VirtualKey.Control, VirtualKey.Shift, VirtualKey.T])
    })]);
}

public sealed record Profile(string Name, int Priority, string? Executable,
    IReadOnlyDictionary<MouseTrigger, ActionDefinition> Mappings);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(KeyboardShortcutAction), "keyboardShortcut")]
public abstract record ActionDefinition;

public sealed record KeyboardShortcutAction(IReadOnlyList<VirtualKey> Keys) : ActionDefinition;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MouseTrigger { XButton1 }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VirtualKey : ushort { Control = 0x11, Shift = 0x10, T = 0x54 }

public static class ConfigurationJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
