using R3;
using Termina.Input;
using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Plugin.Nodes;

public sealed class SubNavNode<TView> : LayoutNode
    where TView : struct, Enum
{
    private readonly ReactiveProperty<TView> _activeView;
    private readonly PageKeyBindings _keyBindings;
    private readonly ThemeDefinition _theme;
    private readonly IThemeService? _themeService;
    private readonly (ConsoleKey Key, string Label, TView Value)[] _items;
    private IDisposable? _subscription;

    private ThemeDefinition CurrentTheme => _themeService?.Current ?? _theme;

    public int ItemCount => _items.Length;

    public SubNavNode(
        ReactiveProperty<TView> activeView,
        PageKeyBindings keyBindings,
        ThemeDefinition theme,
        params (ConsoleKey Key, string Label, TView Value)[] items)
    {
        _activeView = activeView;
        _keyBindings = keyBindings;
        _theme = theme;
        _items = items;

        HeightConstraint = new SizeConstraint.Fixed(1);
        WidthConstraint = new SizeConstraint.Fill();

        RegisterKeys();
    }

    public SubNavNode(
        ReactiveProperty<TView> activeView,
        PageKeyBindings keyBindings,
        IThemeService themeService,
        params (ConsoleKey Key, string Label, TView Value)[] items)
        : this(activeView, keyBindings, themeService.Current, items)
    {
        _themeService = themeService;
    }

    public override Size Measure(Size available) => available with { Height = 1 };

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea)
        {
            return;
        }

        var theme = CurrentTheme;
        var ctx = context.CreateSubContext(bounds);
        ctx.Fill(0, 0, bounds.Width, 1);

        var x = 1;
        foreach (var item in _items)
        {
            var keyName = item.Key.ToString();
            if (keyName.StartsWith('D') && keyName.Length == 2 && char.IsDigit(keyName[1]))
            {
                keyName = keyName[1..];
            }

            var label = item.Label.ToLowerInvariant();
            var isActive = EqualityComparer<TView>.Default.Equals(_activeView.Value, item.Value);

            if (isActive)
            {
                ctx.SetForeground(theme.SelectionText);
                ctx.SetBackground(theme.Selection);
                ctx.WriteAt(x, 0, $" {keyName} {label} ");
                x += keyName.Length + label.Length + 4;
            }
            else
            {
                ctx.SetForeground(theme.Accent);
                ctx.WriteAt(x, 0, keyName);
                ctx.SetForeground(theme.TextDim);
                ctx.WriteAt(x + keyName.Length, 0, $" {label}");
                x += keyName.Length + label.Length + 1;
            }

            ctx.ResetColors();
            x += 2;
        }
    }

    public override void OnActivate()
    {
        _subscription = _activeView.Subscribe(_ => { });
    }

    public override void OnDeactivate()
    {
        _subscription?.Dispose();
        _subscription = null;
    }

    private void RegisterKeys()
    {
        foreach (var item in _items)
        {
            var value = item.Value;
            _keyBindings.Register(item.Key, () => _activeView.Value = value);
        }
    }
}
