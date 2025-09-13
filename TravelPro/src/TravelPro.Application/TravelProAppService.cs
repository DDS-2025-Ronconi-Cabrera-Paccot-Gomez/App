using TravelPro.Localization;
using Volo.Abp.Application.Services;

namespace TravelPro;

/* Inherit your application services from this class.
 */
public abstract class TravelProAppService : ApplicationService
{
    protected TravelProAppService()
    {
        LocalizationResource = typeof(TravelProResource);
    }
}
