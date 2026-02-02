using Volo.Abp.Settings;

namespace TravelPro.Settings;

public class TravelProSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(TravelProSettings.MySetting1));
    }
}
