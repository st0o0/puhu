using Termina.Layout;
using Termina.Reactive;

namespace Servus.Plugin.Marketplace.Pages;

public sealed class MarketplacePage : ReactivePage<MarketplaceViewModel>
{
    public override ILayoutNode BuildLayout() => new EmptyNode();
}
