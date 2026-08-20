using CircleMouse.Core;
using CircleMouse.Windows;
using System.Threading.Channels;

namespace CircleMouse.App;

internal static class Program
{
    [STAThread]
    private static async Task Main()
    {
        ApplicationConfiguration.Initialize();
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CircleMouse", "config.json");
        var store = new ConfigurationStore(path);
        AppConfiguration configuration;
        try { configuration = await store.LoadAsync(); }
        catch { configuration = AppConfiguration.CreateDefault(); }

        using var context = new TrayContext(configuration);
        Application.Run(context);
    }
}

internal sealed class TrayContext : ApplicationContext
{
    private readonly AppConfiguration configuration;
    private readonly ProfileResolver resolver = new();
    private readonly ForegroundApplication foreground = new();
    private readonly SendInputActionExecutor executor = new();
    private readonly LowLevelMouseHook hook;
    private readonly NotifyIcon tray;
    private readonly Channel<MouseTrigger> inputQueue = Channel.CreateBounded<MouseTrigger>(new BoundedChannelOptions(64)
    {
        FullMode = BoundedChannelFullMode.DropWrite,
        SingleReader = true,
        SingleWriter = true
    });
    private readonly CancellationTokenSource shutdown = new();
    private volatile bool enabled = true;

    public TrayContext(AppConfiguration configuration)
    {
        this.configuration = configuration;
        var menu = new ContextMenuStrip();
        var toggle = menu.Items.Add("Mappings enabled", null, (_, _) => enabled = !enabled);
        toggle.Checked = true;
        toggle.CheckOnClick = true;
        menu.Items.Add("Exit", null, (_, _) => ExitThread());
        tray = new NotifyIcon { Icon = SystemIcons.Application, Text = "Circle Mouse", Visible = true, ContextMenuStrip = menu };
        hook = new LowLevelMouseHook(trigger => enabled && inputQueue.Writer.TryWrite(trigger));
        hook.Start();
        _ = ProcessInputAsync(shutdown.Token);
    }

    private async Task ProcessInputAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var trigger in inputQueue.Reader.ReadAllAsync(cancellationToken))
            {
                var resolution = resolver.Resolve(configuration, foreground.GetExecutableName(), trigger);
                if (resolution is not null) executor.Execute(resolution.Action);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    protected override void ExitThreadCore()
    {
        hook.Dispose();
        shutdown.Cancel();
        shutdown.Dispose();
        tray.Visible = false;
        tray.Dispose();
        base.ExitThreadCore();
    }
}
