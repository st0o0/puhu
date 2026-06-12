using System.Reflection;
using System.Threading.Channels;
using Puhu.Plugin;
using Puhu.Plugin.Nodes;
using Puhu.Setup;
using Termina;
using Termina.Input;
using Termina.Layout;
using Termina.Navigation;
using Termina.Reactive;
using Termina.Terminal;

namespace Puhu.Tests;

public sealed class GlobalKeyHandlerTests : IDisposable
{
    private readonly TerminaApplication _app;
    private readonly GlobalKeyHandler _handler;
    private readonly R3.Subject<IInputEvent> _inputSubject;

    public GlobalKeyHandlerTests()
    {
        _app = new TerminaApplication(new VirtualTerminal());
        _app.RegisterRoute<StubPage, StubViewModel>("/a");
        _app.RegisterRoute<StubPage, StubViewModel>("/b");

        TabBarNode.RegisterTabs([
            new PluginTabInfo("A", "/a"),
            new PluginTabInfo("B", "/b")
        ]);

        _app.NavigateTo("/a");

        _inputSubject = (R3.Subject<IInputEvent>)typeof(TerminaApplication)
            .GetField("_inputSubject", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(_app)!;

        _handler = new GlobalKeyHandler(_app);
        _handler.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public void Tab_NavigatesToNextRoute()
    {
        SendKey(ConsoleKey.Tab);
        Assert.Equal("/b", _app.CurrentPath);
    }

    [Fact]
    public void ShiftTab_NavigatesToPreviousRoute()
    {
        SendKey(ConsoleKey.Tab, shift: true);
        Assert.Equal("/b", _app.CurrentPath);
    }

    [Fact]
    public void Tab_WrapsForward()
    {
        SendKey(ConsoleKey.Tab);
        SendKey(ConsoleKey.Tab);
        Assert.Equal("/a", _app.CurrentPath);
    }

    [Fact]
    public void Escape_DoesNotThrow()
    {
        SendKey(ConsoleKey.Escape);
    }

    private void SendKey(ConsoleKey key, bool shift = false)
    {
        _inputSubject.OnNext(new KeyPressed(
            new ConsoleKeyInfo('\0', key, shift, alt: false, control: false)));
    }

    public void Dispose() => _handler.Dispose();

    private sealed class StubViewModel : ReactiveViewModel;
    private sealed class StubPage : ReactivePage<StubViewModel>
    {
        public override ILayoutNode BuildLayout() => new TextNode("stub");
    }
}
