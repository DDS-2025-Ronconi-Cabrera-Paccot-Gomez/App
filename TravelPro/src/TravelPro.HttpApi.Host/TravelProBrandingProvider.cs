using Microsoft.Extensions.Localization;
using TravelPro.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace TravelPro;

[Dependency(ReplaceServices = true)]
public class TravelProBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<TravelProResource> _localizer;

    public TravelProBrandingProvider(IStringLocalizer<TravelProResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
