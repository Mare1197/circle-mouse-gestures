namespace CircleMouse.Core;

public sealed record Resolution(Profile Profile, ActionDefinition Action);

public sealed class ProfileResolver
{
    public Resolution? Resolve(AppConfiguration configuration, string? executable, MouseTrigger trigger)
    {
        return configuration.Profiles
            .Where(profile => profile.Executable is null ||
                string.Equals(Path.GetFileName(profile.Executable), Path.GetFileName(executable), StringComparison.OrdinalIgnoreCase))
            .Where(profile => profile.Mappings.ContainsKey(trigger))
            .OrderByDescending(profile => profile.Executable is not null)
            .ThenByDescending(profile => profile.Priority)
            .Select(profile => new Resolution(profile, profile.Mappings[trigger]))
            .FirstOrDefault();
    }
}
