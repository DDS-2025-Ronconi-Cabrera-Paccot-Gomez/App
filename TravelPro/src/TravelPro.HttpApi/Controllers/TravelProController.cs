using TravelPro.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace TravelPro.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class TravelProController : AbpControllerBase
{
    protected TravelProController()
    {
        LocalizationResource = typeof(TravelProResource);
    }
}
