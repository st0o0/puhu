using R3;
using Termina.Input;
using Termina.Layout;
using Termina.Notifications;
using Termina.Reactive;
using Termina.Terminal;

namespace Servus.Plugin.Marketplace.Pages;

public sealed class MarketplacePage : ReactivePage<MarketplaceViewModel>
{
    private readonly IToastService _toastService;
    private KeyedDynamicLayoutNode<MarketplaceView>? _viewSwitcher;

    public MarketplacePage(IToastService toastService)
    {
        _toastService = toastService;
        FocusPolicy = FocusPolicy.FirstFocusable;
    }

    protected override void OnBound()
    {
        _viewSwitcher = Layouts.KeyedDynamic(
            () => ViewModel.ActiveView.Value,
            view => view switch
            {
                MarketplaceView.Browse => BuildBrowseView(),
                MarketplaceView.Installed => BuildInstalledView(),
                MarketplaceView.Sources => BuildSourcesView(),
                _ => Layouts.Empty()
            });
    }

    public override ILayoutNode BuildLayout()
    {
        return Layouts.Vertical(
            BuildTabBar(),
            _viewSwitcher!.Fill(),
            BuildKeyHints()
        );
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        KeyBindings.Register(ConsoleKey.Tab, () => CycleView(1));
        KeyBindings.Register(ConsoleKey.Tab, ConsoleModifiers.Shift, () => CycleView(-1));
        KeyBindings.Register(ConsoleKey.D1, () => ViewModel.SwitchView(MarketplaceView.Browse));
        KeyBindings.Register(ConsoleKey.D2, () => ViewModel.SwitchView(MarketplaceView.Installed));
        KeyBindings.Register(ConsoleKey.D3, () => ViewModel.SwitchView(MarketplaceView.Sources));

        ViewModel.ActiveView
            .Subscribe(_ =>
            {
                _viewSwitcher?.Invalidate();
                InvalidateLayout();
            })
            .DisposeWith(Subscriptions);

        ViewModel.StatusMessage
            .Where(msg => msg is not null)
            .Subscribe(msg =>
            {
                var isError = msg!.StartsWith("Error:");
                _toastService.Show(msg, new ToastOptions(
                    Color: isError ? Color.Red : Color.Green,
                    Icon: isError ? "✗" : "✓"));
            })
            .DisposeWith(Subscriptions);
    }

    private void CycleView(int direction)
    {
        var views = Enum.GetValues<MarketplaceView>();
        var current = (int)ViewModel.ActiveView.Value;
        var next = (current + direction + views.Length) % views.Length;
        ViewModel.SwitchView(views[next]);
    }

    private ILayoutNode BuildTabBar()
    {
        var tabs = Enum.GetValues<MarketplaceView>();
        var children = new List<ILayoutNode>();

        foreach (var tab in tabs)
        {
            var isActive = tab == ViewModel.ActiveView.Value;
            var label = isActive ? $"[{tab}]" : $" {tab} ";
            var node = new TextNode(label);
            if (isActive) node.WithForeground(Color.Cyan).Bold();
            children.Add(node);
        }

        children.Add(new TextNode("Marketplace").WithForeground(Color.DarkGray).AlignRight().WidthFill());

        return Layouts.Horizontal(children.ToArray()).Height(1);
    }

    private ILayoutNode BuildBrowseView()
    {
        return new TextNode("Browse view").WithForeground(Color.DarkGray);
    }

    private ILayoutNode BuildInstalledView()
    {
        return new TextNode("Installed view").WithForeground(Color.DarkGray);
    }

    private ILayoutNode BuildSourcesView()
    {
        return new TextNode("Sources view").WithForeground(Color.DarkGray);
    }

    private ILayoutNode BuildKeyHints()
    {
        return new TextNode("Tab Switch View  1 Browse  2 Installed  3 Sources")
            .WithForeground(Color.DarkGray).Height(1);
    }
}
