using R3;
using Termina.Input;
using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Plugin.Nodes;

public sealed class SubNavNode<TView> : LayoutNode
    where TView : struct, Enum
{
    private readonly ReactiveProperty<TView> _activeView;
    private readonly PageKeyBindings _keyBindings;
    private readonly ThemeDefinition _theme;
    private readonly (ConsoleKey Key, string Label, TView Value)[] _items;
    private IDisposable? _subscription;

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
    }

    public override Size Measure(Size available) => available with { Height = 1 };

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea)
            return;

        var ctx = context.CreateSubContext(bounds);
        ctx.Fill(0, 0, bounds.Width, 1);

        var x = 1;
        for (var i = 0; i < _items.Length; i++)
        {
            var item = _items[i];
            var keyName = item.Key.ToString();
            if (keyName.StartsWith("D") && keyName.Length == 2 && char.IsDigit(keyName[1]))
                keyName = keyName[1..];
            var label = $"{keyName}:{item.Label}";
            var isActive = EqualityComparer<TView>.Default.Equals(_activeView.Value, item.Value);

            if (isActive)
            {
                ctx.SetForeground(_theme.SelectionText);
                ctx.SetBackground(_theme.Selection);
            }
            else
            {
                ctx.SetForeground(_theme.TextDim);
            }

            ctx.WriteAt(x, 0, label);
            ctx.ResetColors();

            x += label.Length + 3;
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
