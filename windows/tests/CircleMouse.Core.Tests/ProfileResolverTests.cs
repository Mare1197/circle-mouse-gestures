using CircleMouse.Core;

namespace CircleMouse.Core.Tests;

public sealed class ProfileResolverTests
{
    [Fact]
    public void ApplicationProfileOverridesGlobalProfile()
    {
        var globalAction = new KeyboardShortcutAction([VirtualKey.T]);
        var appAction = new KeyboardShortcutAction([VirtualKey.Control, VirtualKey.T]);
        var configuration = new AppConfiguration(1,
        [
            new Profile("Global", 100, null, new Dictionary<MouseTrigger, ActionDefinition> { [MouseTrigger.XButton1] = globalAction }),
            new Profile("Browser", 1, "chrome.exe", new Dictionary<MouseTrigger, ActionDefinition> { [MouseTrigger.XButton1] = appAction })
        ]);

        var result = new ProfileResolver().Resolve(configuration, @"C:\Program Files\Chrome\chrome.exe", MouseTrigger.XButton1);

        Assert.Same(appAction, result!.Action);
    }
}
